Shader "Hidden/ClaudeOfTanks/Post"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _BloomTex;
        float4 _MainTex_TexelSize;
        float2 _BlurDirection;
        float _BloomThreshold;
        float _BloomStrength;
        float _Exposure;
        float _Contrast;
        float _Saturation;
        float _Vignette;

        struct Attributes
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 position : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Varyings Vert(Attributes input)
        {
            Varyings output;
            output.position = UnityObjectToClipPos(input.vertex);
            output.uv = input.uv;
            return output;
        }

        half4 Prefilter(Varyings input) : SV_Target
        {
            half3 color = tex2D(_MainTex, input.uv).rgb;
            half brightness = max(color.r, max(color.g, color.b));
            half contribution = saturate(
                (brightness - _BloomThreshold) /
                max(brightness, 0.0001h));
            return half4(color * contribution, 1.0h);
        }

        half4 Blur(Varyings input) : SV_Target
        {
            float2 offset = _MainTex_TexelSize.xy * _BlurDirection;
            half3 color = tex2D(_MainTex, input.uv).rgb * 0.227027h;
            color += tex2D(
                _MainTex,
                input.uv + offset * 1.384615).rgb * 0.316216h;
            color += tex2D(
                _MainTex,
                input.uv - offset * 1.384615).rgb * 0.316216h;
            color += tex2D(
                _MainTex,
                input.uv + offset * 3.230769).rgb * 0.070270h;
            color += tex2D(
                _MainTex,
                input.uv - offset * 3.230769).rgb * 0.070270h;
            return half4(color, 1.0h);
        }

        half3 AcesFit(half3 value)
        {
            const half a = 2.51h;
            const half b = 0.03h;
            const half c = 2.43h;
            const half d = 0.59h;
            const half e = 0.14h;
            return saturate(
                (value * (a * value + b)) /
                (value * (c * value + d) + e));
        }

        half4 Composite(Varyings input) : SV_Target
        {
            half3 scene = tex2D(_MainTex, input.uv).rgb;
            half3 bloom = tex2D(_BloomTex, input.uv).rgb;
            half3 color = AcesFit(
                (scene + bloom * _BloomStrength) * exp2(_Exposure));
            half luminance = dot(
                color,
                half3(0.2126h, 0.7152h, 0.0722h));
            color = lerp(luminance.xxx, color, _Saturation);
            color = (color - 0.5h) * _Contrast + 0.5h;
            float2 centered = input.uv * 2.0 - 1.0;
            half vignette = 1.0h -
                _Vignette *
                smoothstep(0.28h, 1.15h, dot(centered, centered));
            return half4(saturate(color * vignette), 1.0h);
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Prefilter
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Blur
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Composite
            ENDCG
        }
    }

    Fallback Off
}
