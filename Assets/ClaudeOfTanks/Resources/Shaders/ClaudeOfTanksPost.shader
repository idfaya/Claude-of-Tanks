Shader "Hidden/ClaudeOfTanks/Post"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
        _AoTex ("Ambient Occlusion", 2D) = "white" {}
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
        sampler2D _AoTex;
        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
        sampler2D _CameraDepthNormalsTexture;
        float4 _MainTex_TexelSize;
        float2 _BlurDirection;
        float _BloomThreshold;
        float _BloomStrength;
        float _Exposure;
        float _Contrast;
        float _Saturation;
        float _Vignette;
        float _AoSampleCount;
        float _AoIntensity;
        float _AoRadiusM;
        float _TanHalfFov;
        float _CameraAspect;
        float _AerialDensity;
        float _AerialHazeDensity;
        float _AerialStrength;
        half4 _FogColor;
        float3 _SunDirectionVS;

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

        float RawDepth(float2 uv)
        {
            return SAMPLE_DEPTH_TEXTURE(
                _CameraDepthTexture,
                uv);
        }

        float EyeDepth(float2 uv)
        {
            return LinearEyeDepth(RawDepth(uv));
        }

        half3 ApplyAerial(
            half3 scene,
            float2 uv)
        {
            if (_AerialStrength <= 0.001)
                return scene;
            float rawDepth = RawDepth(uv);
            if (Linear01Depth(rawDepth) >= 0.9999)
                return scene;

            float depth = EyeDepth(uv);
            float extinctionX = depth * _AerialDensity;
            float extinction = 1.0 -
                exp(-extinctionX * extinctionX);
            half luminance = dot(
                scene,
                half3(0.2126h, 0.7152h, 0.0722h));
            half3 distant = lerp(
                scene,
                luminance.xxx *
                    half3(0.90h, 0.97h, 1.08h),
                0.62h);
            scene = lerp(
                scene,
                distant,
                extinction * _AerialStrength);

            float2 clip = uv * 2.0 - 1.0;
            float3 viewRay = normalize(float3(
                clip.x * _TanHalfFov * _CameraAspect,
                clip.y * _TanHalfFov,
                1.0));
            float sunAmount = pow(
                saturate(dot(
                    viewRay,
                    normalize(_SunDirectionVS))),
                5.0);
            half3 cool = _FogColor.rgb *
                half3(0.86h, 0.95h, 1.13h);
            half3 warm = _FogColor.rgb *
                half3(1.16h, 1.035h, 0.86h);
            half3 haze = lerp(cool, warm, sunAmount);
            half hazeLuminance = max(
                dot(
                    haze,
                    half3(0.2126h, 0.7152h, 0.0722h)),
                0.0001h);
            haze *= min(1.0h, 0.55h / hazeLuminance);
            float hazeDistance = max(depth - 85.0, 0.0);
            float hazeX =
                hazeDistance * _AerialHazeDensity;
            float scatter = 1.0 -
                exp(-hazeX * hazeX);
            scatter *= lerp(
                0.25,
                1.0,
                smoothstep(0.0, 0.05, luminance));
            return lerp(
                scene,
                haze,
                scatter * _AerialStrength);
        }

        half4 Composite(Varyings input) : SV_Target
        {
            half3 scene = tex2D(_MainTex, input.uv).rgb;
            half3 bloom = tex2D(_BloomTex, input.uv).rgb;
            half ao = tex2D(_AoTex, input.uv).r;
            scene *= ao;
            scene = ApplyAerial(scene, input.uv);
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

        half4 AmbientOcclusion(
            Varyings input) : SV_Target
        {
            float rawDepth = RawDepth(input.uv);
            if (Linear01Depth(rawDepth) >= 0.9999 ||
                _AoSampleCount < 1.0)
            {
                return half4(1.0h, 1.0h, 1.0h, 1.0h);
            }

            float eyeDepth = LinearEyeDepth(rawDepth);
            float centerDepth01;
            float3 centerNormal;
            DecodeDepthNormal(
                tex2D(
                    _CameraDepthNormalsTexture,
                    input.uv),
                centerDepth01,
                centerNormal);
            float2 screenTexel =
                1.0 / _ScreenParams.xy;
            float depthDx =
                EyeDepth(saturate(
                    input.uv +
                    float2(screenTexel.x, 0.0))) -
                eyeDepth;
            float depthDy =
                EyeDepth(saturate(
                    input.uv +
                    float2(0.0, screenTexel.y))) -
                eyeDepth;
            float pixelRadius = clamp(
                _AoRadiusM /
                    max(eyeDepth, 0.1) *
                    _ScreenParams.y /
                    max(2.0 * _TanHalfFov, 0.01),
                1.0,
                18.0);
            float occlusion = 0.0;
            float validSamples = 0.0;
            [unroll]
            for (int i = 0; i < 12; i++)
            {
                if (i >= (int)_AoSampleCount)
                    break;
                float sampleIndex = i + 0.5;
                float angle =
                    sampleIndex * 2.39996323;
                float radius =
                    (sampleIndex / _AoSampleCount) *
                    pixelRadius;
                float2 offsetPixels = float2(
                    cos(angle),
                    sin(angle)) *
                    radius;
                float2 sampleUv =
                    saturate(
                        input.uv +
                        offsetPixels * screenTexel);
                float sampleRaw = RawDepth(sampleUv);
                if (Linear01Depth(sampleRaw) >= 0.9999)
                    continue;
                float sampleEye =
                    LinearEyeDepth(sampleRaw);
                float sampleDepth01;
                float3 sampleNormal;
                DecodeDepthNormal(
                    tex2D(
                        _CameraDepthNormalsTexture,
                        sampleUv),
                    sampleDepth01,
                    sampleNormal);
                float expectedDepth =
                    eyeDepth +
                    depthDx * offsetPixels.x +
                    depthDy * offsetPixels.y;
                float intrusion =
                    expectedDepth - sampleEye;
                float bias = max(
                    0.02,
                    eyeDepth * 0.0006);
                if (intrusion <= bias ||
                    intrusion >= _AoRadiusM)
                {
                    continue;
                }
                occlusion += smoothstep(
                        bias,
                        min(
                            _AoRadiusM,
                            bias + 0.35),
                        intrusion) *
                    smoothstep(
                        0.035,
                        0.35,
                        1.0 -
                            saturate(dot(
                                centerNormal,
                                sampleNormal))) *
                    saturate(
                        1.0 -
                        intrusion / _AoRadiusM);
                validSamples += 1.0;
            }
            float ao = 1.0 -
                occlusion /
                max(validSamples, 1.0) *
                _AoIntensity *
                1.6;
            float distanceFade =
                1.0 - smoothstep(
                    120.0,
                    280.0,
                    eyeDepth);
            ao = lerp(
                1.0,
                max(saturate(ao), 0.45),
                distanceFade);
            return half4(ao, ao, ao, 1.0h);
        }

        half4 BlurAmbientOcclusion(
            Varyings input) : SV_Target
        {
            float centerDepth = EyeDepth(input.uv);
            half total = 0.0h;
            half weightTotal = 0.0h;
            [unroll]
            for (int i = -2; i <= 2; i++)
            {
                float2 sampleUv = input.uv +
                    _MainTex_TexelSize.xy *
                    _BlurDirection *
                    i;
                float sampleDepth = EyeDepth(sampleUv);
                half spatial = i == 0
                    ? 0.40h
                    : (abs(i) == 1 ? 0.24h : 0.06h);
                half depthWeight = saturate(
                    1.0 -
                    abs(sampleDepth - centerDepth) /
                    max(0.08, centerDepth * 0.015));
                half weight = spatial * depthWeight;
                total += tex2D(
                    _MainTex,
                    sampleUv).r * weight;
                weightTotal += weight;
            }
            half ao = total / max(weightTotal, 0.0001h);
            return half4(ao, ao, ao, 1.0h);
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

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment AmbientOcclusion
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment BlurAmbientOcclusion
            ENDCG
        }
    }

    Fallback Off
}
