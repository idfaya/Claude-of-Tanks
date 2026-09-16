Shader "ClaudeOfTanks/MapHorizonDetail"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _CotHorizonBanding ("Banding", Float) = 0.12
        _CotHorizonDetailStrength ("Detail Strength", Float) = 0.55
        _CotHorizonStyle ("Style", Float) = 0
        _CotHorizonMaxHeight ("Max Height", Float) = 220
        _CotHorizonTextureRange ("Texture Range", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 150
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _CotHorizonBanding;
            float _CotHorizonDetailStrength;
            float _CotHorizonStyle;
            float _CotHorizonMaxHeight;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                fixed4 color : COLOR;
                UNITY_FOG_COORDS(3)
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float SignedNoise(float2 p)
            {
                return ValueNoise(p) * 2.0 - 1.0;
            }

            float TriplanarMottle(float3 worldPos, float3 normal, float scale)
            {
                float3 weights = abs(normal);
                weights /= max(weights.x + weights.y + weights.z, 0.0001);
                float nx = SignedNoise(worldPos.zy * scale + float2(11.7, 3.1));
                float ny = SignedNoise(worldPos.xz * scale + float2(0.5, 19.3));
                float nz = SignedNoise(worldPos.xy * scale + float2(7.9, 13.4));
                return nx * weights.x + ny * weights.y + nz * weights.z;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 n = normalize(i.worldNormal);
                float height01 = saturate(i.worldPos.y / max(_CotHorizonMaxHeight, 1.0));
                float steep = smoothstep(0.28, 0.72, 1.0 - saturate(n.y));
                float cap = smoothstep(0.82, 0.97, saturate(n.y));
                float mesa = smoothstep(1.5, 2.5, _CotHorizonStyle);
                float alpine = 1.0 - smoothstep(1.1, 1.9, abs(_CotHorizonStyle - 1.0));

                float skirt = 1.0 - smoothstep(0.16, 0.32, height01);
                float tangentWall = steep * (1.0 - cap);
                float3 albedo = tex2D(_MainTex, i.uv).rgb;
                float textureCalm = saturate(skirt * 0.88 + tangentWall * mesa * 0.58);
                half albedoLuma = dot(albedo, half3(0.2126h, 0.7152h, 0.0722h));
                albedo = lerp(albedo, lerp(albedoLuma.xxx, half3(0.82h, 0.80h, 0.76h), 0.72h), textureCalm);
                float broad = TriplanarMottle(i.worldPos, n, 0.0060);
                float fine = TriplanarMottle(i.worldPos, n, 0.0210);
                float uvFine = SignedNoise(i.uv * float2(48.0, 22.0));
                float detail = broad * 0.13 + fine * 0.09 + uvFine * 0.07 * (1.0 - steep * 0.6);
                detail *= 1.0 - skirt * (0.82 + mesa * 0.16);
                detail *= 1.0 - tangentWall * mesa * 0.42;

                float bedWarp = broad * 5.2 + fine * 2.1;
                float beds = sin(i.worldPos.y * 0.42 + bedWarp) * 0.62 +
                    sin(i.worldPos.y * 0.13 + bedWarp * 0.45 + 1.7) * 0.38;
                float marker = smoothstep(0.78, 0.96, sin(i.worldPos.y * 0.075 + broad * 1.6));
                float banding = _CotHorizonBanding * (0.28 + mesa * 0.78 + alpine * 0.28);
                float mesaLowerWall = mesa * tangentWall * (1.0 - smoothstep(0.30, 0.70, height01));
                banding *= 1.0 - skirt * 0.78;
                banding *= 1.0 - tangentWall * mesa * 0.34;
                banding *= 1.0 - mesaLowerWall * 0.62;

                float slopeRock = steep * (0.35 + 0.65 * mesa);
                float capFix = cap * mesa;
                float rockExposure = slopeRock * (1.0 - smoothstep(0.62, 0.92, height01));
                float shade = 1.0 + detail * _CotHorizonDetailStrength;
                shade *= 1.0 + beds * banding - marker * banding * 0.22;
                shade *= 1.0 - rockExposure * 0.055 + capFix * fine * 0.04;
                shade = clamp(shade, 0.84, 1.10);

                float3 color = albedo * i.color.rgb * _Color.rgb;
                color *= shade;
                color = lerp(color, color * float3(0.88, 0.84, 0.77), rockExposure * 0.16 * mesa);
                color = lerp(color, color * float3(0.88, 0.93, 1.06), steep * alpine * 0.28);
                color = lerp(color, i.color.rgb * _Color.rgb, skirt * (0.62 + mesa * 0.16));
                color = lerp(color, i.color.rgb * _Color.rgb, mesaLowerWall * 0.52);
                color = lerp(color, color * float3(1.08, 1.05, 1.00), tangentWall * mesa * 0.18);
                fixed4 outputColor = fixed4(saturate(color), i.color.a * _Color.a);
                UNITY_APPLY_FOG(i.fogCoord, outputColor);
                return outputColor;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
