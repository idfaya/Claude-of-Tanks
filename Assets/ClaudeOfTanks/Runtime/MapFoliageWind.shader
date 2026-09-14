Shader "ClaudeOfTanks/MapFoliageWindCutout"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.38
        _WindStrength ("Wind Strength", Float) = 0.25
        _WindSpeed ("Wind Speed", Float) = 1.7
        _WindScale ("Wind Scale", Float) = 0.045
        _WindPhase ("Wind Phase", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 150
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff;
            float _WindStrength;
            float _WindSpeed;
            float _WindScale;
            float _WindPhase;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                float windWeight = saturate(v.color.a) * saturate(v.uv.y);
                float phase = (v.vertex.x + v.vertex.z) * _WindScale +
                    _Time.y * _WindSpeed + _WindPhase;
                float sway = sin(phase) * _WindStrength * windWeight;
                v.vertex.x += sway;
                v.vertex.z += cos(phase * 0.73) * sway * 0.35;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv) * _Color;
                clip(color.a - _Cutoff);
                return color;
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent Cutout"
}
