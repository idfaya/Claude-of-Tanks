Shader "ClaudeOfTanks/TankBakedPresentation"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "bump" {}
        _RoughnessMap ("Roughness", 2D) = "white" {}
        _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _SrcBlend ("Source Blend", Float) = 1
        _DstBlend ("Destination Blend", Float) = 0
        _ZWrite ("Z Write", Float) = 1
        _Cull ("Cull", Float) = 2
        _UseCamo ("Use Camo", Float) = 0
        _HasMainTex ("Has Main Texture", Float) = 0
        _HasNormalMap ("Has Normal Map", Float) = 0
        _HasRoughnessMap ("Has Roughness Map", Float) = 0
        _CamoScale ("Camo Scale", Float) = 0.34
        _FlatLighting ("Flat Lighting", Float) = 0
        _EmissionBoost ("Emission Boost", Float) = 0.12
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull [_Cull]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _RoughnessMap;
        fixed4 _EmissionColor;
        half _Metallic;
        half _Glossiness;
        half _UseCamo;
        half _HasMainTex;
        half _HasNormalMap;
        half _HasRoughnessMap;
        half _CamoScale;
        half _FlatLighting;
        half _EmissionBoost;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color : COLOR;
        };

        float hash21(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        fixed3 camoTint(float2 uv)
        {
            float scale = max(_CamoScale, 0.01);
            float2 p = uv / scale;
            float n1 = hash21(floor(p * 3.0));
            float n2 = hash21(floor(p * 7.0 + 19.7));
            fixed3 dark = fixed3(0.52, 0.58, 0.43);
            fixed3 mid = fixed3(0.77, 0.84, 0.63);
            fixed3 warm = fixed3(0.46, 0.40, 0.30);
            fixed3 tone = n1 < 0.36 ? dark : (n1 < 0.72 ? mid : warm);
            return lerp(tone, fixed3(1, 1, 1), n2 * 0.08);
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 tint = _Color * input.color;
            fixed3 tex = tex2D(_MainTex, input.uv_MainTex).rgb;
            fixed3 camo = camoTint(input.uv_MainTex);
            fixed3 mapped = lerp(camo, tex, saturate(_HasMainTex));
            output.Albedo = lerp(tint.rgb, tint.rgb * mapped, saturate(max(_UseCamo, _HasMainTex)));
            output.Alpha = 1;
            output.Emission =
                _EmissionColor.rgb +
                output.Albedo * saturate(_FlatLighting) * _EmissionBoost;
            output.Metallic = _Metallic;
            fixed roughness = tex2D(_RoughnessMap, input.uv_MainTex).g;
            output.Smoothness = lerp(_Glossiness, 1.0 - roughness, saturate(_HasRoughnessMap));
            output.Normal = lerp(
                fixed3(0, 0, 1),
                UnpackNormal(tex2D(_NormalMap, input.uv_MainTex)),
                saturate(_HasNormalMap));
        }
        ENDCG
    }

    FallBack "Standard"
}
