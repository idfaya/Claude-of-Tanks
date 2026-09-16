Shader "ClaudeOfTanks/MapCloudDeck"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,0.55)
        _MainTex ("Cloud Texture", 2D) = "white" {}
        _CotCloudAltitude ("Virtual Deck Altitude", Float) = 620
        _CotCloudScale ("Texture Scale M", Float) = 3200
        _CotCloudHazeColor ("Haze Color", Color) = (0.78,0.84,0.89,1)
        _CotCloudHazeK ("Haze Rate", Float) = 0.00023
        _CotCloudSunRot ("Sun Rotation", Vector) = (1,0,0,0)
        _CotCloudYFade ("Horizon Fade", Vector) = (0.007,0.034,0,0)
        _CotCloudShadeStrength ("Mass Shade", Float) = 0.28
    }

    SubShader
    {
        Tags { "Queue"="Transparent-20" "RenderType"="Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _CotCloudAltitude;
            float _CotCloudScale;
            fixed4 _CotCloudHazeColor;
            float _CotCloudHazeK;
            float4 _CotCloudSunRot;
            float4 _CotCloudYFade;
            float _CotCloudShadeStrength;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float4 wp = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = wp.xyz;
                o.vertex = mul(UNITY_MATRIX_VP, wp);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldPos - _WorldSpaceCameraPos.xyz);
                float dy = max(dir.y, 0.0001);
                float t = max(_CotCloudAltitude - _WorldSpaceCameraPos.y, 1.0) / dy;
                float2 p = _WorldSpaceCameraPos.xz + dir.xz * t;
                float2 rot = _CotCloudSunRot.xy;
                float2 uv = float2(
                    p.x * rot.x - p.y * rot.y,
                    p.x * rot.y + p.y * rot.x) / max(_CotCloudScale, 1.0);
                uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;

                fixed4 cloud = tex2D(_MainTex, uv);
                float sunSide = tex2D(_MainTex, uv + float2(0.0, -0.045)).a;
                cloud.rgb *= 1.0 -
                    _CotCloudShadeStrength *
                    saturate(sunSide - cloud.a * 0.55);

                float2 macroUv = float2(
                    uv.x * 0.31 - uv.y * 0.17,
                    uv.x * 0.17 + uv.y * 0.31) + float2(0.37, 0.71);
                float macro = tex2D(_MainTex, macroUv).a;
                cloud.a *= 0.62 + 0.38 * smoothstep(0.05, 0.72, macro);

                float hazeX = t * _CotCloudHazeK;
                float haze = 1.0 - exp(-hazeX * hazeX);
                float body = saturate(cloud.a * 1.35);
                float luma = dot(cloud.rgb, fixed3(0.2126, 0.7152, 0.0722));
                fixed3 shapedCloud = cloud.rgb * _Color.rgb;
                shapedCloud *= lerp(0.68, 1.16, luma);
                shapedCloud = lerp(
                    shapedCloud,
                    fixed3(1.0, 0.98, 0.94) * _Color.rgb,
                    body * 0.22);
                fixed3 color = lerp(shapedCloud, _CotCloudHazeColor.rgb, haze);
                float alpha = cloud.a * _Color.a;
                alpha *= smoothstep(_CotCloudYFade.x, _CotCloudYFade.y, dir.y);
                alpha *= 1.0 - 0.58 * smoothstep(0.5, 0.96, haze);
                alpha *= 1.18;
                return fixed4(saturate(color), saturate(alpha));
            }
            ENDCG
        }
    }

    Fallback "Unlit/Transparent"
}
