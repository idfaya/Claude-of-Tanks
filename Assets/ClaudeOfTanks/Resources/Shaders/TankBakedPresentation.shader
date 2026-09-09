Shader "ClaudeOfTanks/TankBakedPresentation"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _SrcBlend ("Source Blend", Float) = 1
        _DstBlend ("Destination Blend", Float) = 0
        _ZWrite ("Z Write", Float) = 1
        _Cull ("Cull", Float) = 2
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
        fixed4 _EmissionColor;
        half _Metallic;
        half _Glossiness;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color : COLOR;
        };

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 tint = _Color * input.color;
            output.Albedo = tint.rgb;
            output.Alpha = 1;
            output.Emission = _EmissionColor.rgb;
            output.Metallic = _Metallic;
            output.Smoothness = _Glossiness;
        }
        ENDCG
    }

    FallBack "Standard"
}
