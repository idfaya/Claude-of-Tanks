Shader "ClaudeOfTanks/MapTerrainDetail"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normal", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 0.25
        _Glossiness ("Smoothness", Range(0,1)) = 0.06
        _Metallic ("Metallic", Range(0,1)) = 0
        _CotSplatSandMacro ("Sand Macro", Float) = 0
        _CotSplatRippleAmp ("Ripple Amp", Float) = 0
        _CotSplatMicroAmp ("Micro Amp", Float) = 1
        _CotSplatMidRelief ("Mid Relief", Float) = 1
        _CotSplatTownWear ("Town Wear", Float) = 1
        _CotTerrainRole ("Terrain Role", Float) = 0
        _CotTerrainShaderApplied ("Shader Applied", Float) = 1
        _CotTerrainCloudShade ("Cloud Shade", Float) = 0.16
        _CotTerrainMaxLuminance ("Texture Max Luminance", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 220
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half4 _Color;
        half _Glossiness;
        half _Metallic;
        half _BumpScale;
        half _CotSplatSandMacro;
        half _CotSplatRippleAmp;
        half _CotSplatMicroAmp;
        half _CotSplatMidRelief;
        half _CotSplatTownWear;
        half _CotTerrainRole;
        half _CotTerrainCloudShade;
        half _CotTerrainMaxLuminance;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
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

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
            float3 normal = normalize(IN.worldNormal);
            float2 xz = IN.worldPos.xz;
            float distanceM = distance(IN.worldPos, _WorldSpaceCameraPos.xyz);
            float slope = 1.0 - saturate(normal.y);
            float terrainRole = 1.0 - smoothstep(1.5, 2.5, _CotTerrainRole);
            float roadRole = smoothstep(1.5, 2.5, _CotTerrainRole) *
                (1.0 - smoothstep(3.5, 4.5, _CotTerrainRole));

            float macroA = ValueNoise(xz * 0.0024 + float2(0.13, 0.83));
            float macroB = ValueNoise(xz * 0.0009 + float2(0.77, 0.31));
            float midA = SignedNoise(xz * 0.017 + float2(0.07, 0.31));
            float midB = SignedNoise(xz * 0.0052 + float2(0.71, 0.23));
            float fine = SignedNoise(xz * 0.071 + float2(0.19, 0.41));
            float midBand = smoothstep(20.0, 65.0, distanceM) *
                (1.0 - smoothstep(260.0, 820.0, distanceM));
            float nearBand = 1.0 - smoothstep(8.0, 42.0, distanceM);

            float openSand = saturate(_CotSplatSandMacro) * terrainRole *
                (1.0 - roadRole) * (1.0 - smoothstep(0.24, 0.48, slope));
            float gravel = smoothstep(0.54, 0.84, macroA + fine * 0.10) * openSand;
            float scour = smoothstep(0.62, 0.92, macroB) * openSand * (1.0 - gravel);
            albedo = lerp(albedo, albedo * float3(0.76, 0.715, 0.65), gravel * 0.58);
            albedo = lerp(albedo, albedo * float3(1.015, 1.005, 0.97), scour * 0.20);

            float luminance = dot(albedo, float3(0.2126, 0.7152, 0.0722));
            float shoulder = smoothstep(0.52, 0.78, luminance) * openSand;
            albedo *= 1.0 - shoulder * 0.26;
            albedo *= 1.0 - openSand * 0.34;

            float relief = (midA * 0.055 + midB * 0.075) *
                saturate(_CotSplatMidRelief) * midBand * (1.0 - slope * 0.7);
            float micro = fine * 0.13 * saturate(_CotSplatMicroAmp) * nearBand;
            albedo *= 1.0 + relief + micro;

            if (_CotSplatRippleAmp > 0.001)
            {
                float phase = dot(xz, normalize(float2(0.8, 0.6)));
                float ripple = sin(phase * 0.55 + macroB * 4.0) * 0.075 +
                    sin(phase * 0.24 + macroA * 5.0) * 0.11;
                float rippleW = saturate(_CotSplatRippleAmp) * openSand *
                    smoothstep(42.0, 150.0, distanceM) *
                    (1.0 - smoothstep(180.0, 420.0, distanceM));
                albedo *= 1.0 + ripple * rippleW;
            }

            float strata = (sin(IN.worldPos.y * 0.42 + midA * 4.0) * 0.6 +
                sin(IN.worldPos.y * 0.13 + midB * 3.0) * 0.4);
            float strataW = smoothstep(0.34, 0.58, slope) *
                saturate(_CotSplatSandMacro) * 0.10;
            albedo *= 1.0 + strata * strataW;

            float roadWear = roadRole * saturate((_CotSplatTownWear - 1.0) * 0.38);
            albedo *= 1.0 - roadWear * (0.12 + ValueNoise(xz * 0.12) * 0.12);
            float cloudShade =
                ValueNoise(xz * 0.00294 + float2(4.7, 8.1)) * 0.62 +
                ValueNoise(xz * 0.00763 + float2(0.9, 6.4)) * 0.38;
            float cloudPatch = smoothstep(0.38, 0.76, cloudShade);
            float cloudDistance = smoothstep(40.0, 120.0, distanceM) *
                (1.0 - smoothstep(1050.0, 1500.0, distanceM));
            float cloudSurface = terrainRole * (1.0 - roadRole) *
                (1.0 - slope * 0.45);
            albedo *= 1.0 -
                saturate(_CotTerrainCloudShade) *
                cloudPatch *
                cloudDistance *
                cloudSurface;
            float terrainCap = lerp(_CotTerrainMaxLuminance + 0.08, 0.50, openSand);
            albedo = min(albedo, float3(terrainCap, terrainCap, terrainCap));

            o.Albedo = saturate(albedo);
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a;
        }
        ENDCG
    }

    Fallback "Standard"
}
