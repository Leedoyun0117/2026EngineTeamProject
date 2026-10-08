Shader "LDY/CrtScreen"
{
    Properties
    {
        _ContentTex ("Content", 2D) = "black" {}
        _Curvature ("Curvature", Float) = 0.03
        _ScanlineIntensity ("Scanline Intensity", Float) = 0.12
        _ScanlineCount ("Scanline Count", Float) = 360
        _VignetteIntensity ("Vignette Intensity", Float) = 0.2
        _VignetteSoftness ("Vignette Softness", Float) = 0.5
        _FlickerIntensity ("Flicker Intensity", Float) = 0.008
        _NoiseIntensity ("Noise Intensity", Float) = 0.006
        _CrtTime ("CRT Time", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Cull Off
        ZWrite Off

        Pass
        {
            Name "CrtScreen"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ContentTex);
            SAMPLER(sampler_ContentTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _ContentTex_ST;
                float _Curvature;
                float _ScanlineIntensity;
                float _ScanlineCount;
                float _VignetteIntensity;
                float _VignetteSoftness;
                float _FlickerIntensity;
                float _NoiseIntensity;
                float _CrtTime;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // CrtCurvature.Warp(C#)와 같은 식을 유지할 것.
            float2 CrtWarp(float2 uv, float k)
            {
                float2 p = uv * 2.0 - 1.0;
                p += float2(p.x * p.y * p.y, p.y * p.x * p.x) * k;
                return p * 0.5 + 0.5;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = CrtWarp(input.uv, _Curvature);
                if (any(uv < 0.0) || any(uv > 1.0))
                    return half4(0, 0, 0, 1);

                float3 color = SAMPLE_TEXTURE2D(_ContentTex, sampler_ContentTex, uv).rgb;

                float scan = sin(uv.y * _ScanlineCount * 6.2831853) * 0.5 + 0.5;
                color *= 1.0 - _ScanlineIntensity * scan;

                float flicker = Hash21(float2(floor(_CrtTime * 30.0), 7.13)) * 2.0 - 1.0;
                color *= 1.0 + _FlickerIntensity * flicker;

                float vignette = smoothstep(1.0 - _VignetteSoftness, 1.0, length(input.uv - 0.5) * 1.4142);
                color *= 1.0 - _VignetteIntensity * vignette;

                float noise = Hash21(floor(input.uv * float2(320.0, 240.0)) + floor(_CrtTime * 24.0));
                color += (noise - 0.5) * _NoiseIntensity;

                return half4(saturate(color), 1);
            }
            ENDHLSL
        }
    }
}
