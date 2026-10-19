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
        _PowerOff ("Power Off (0 on, 1 off)", Range(0, 1)) = 0
        _PowerFill ("Power Fill", Vector) = (1, 1, 0, 0)
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
                float _PowerOff;
                float4 _PowerFill;
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

            // 전원 꺼짐 마스크. 곡률 변환 전의 화면 UV로 계산하는 출력 마스킹이라 콘텐츠 좌표(클릭 판정)에 영향이 없다.
            // 세로로 줄어 가로선이 되고, 가로로 줄어 점이 된다. 켜질 때는 같은 값을 거꾸로 지난다.
            float PowerMask(float2 screenUv, float p, float2 fill, out float glow)
            {
                glow = 0.0;
                if (p <= 0.0)
                    return 1.0;
                if (p >= 1.0)
                    return 0.0;

                const float split = 0.65;
                const float lineHalf = 0.004;
                float a = saturate(p / split);
                float b = saturate((p - split) / (1.0 - split));
                float2 ext = float2(lerp(0.5 * fill.x, lineHalf, b * b), lerp(0.5 * fill.y, lineHalf, a * a));
                float2 edge = max(ext * 0.15, 0.0015);
                float2 d = abs(screenUv - 0.5);
                float mask = (1.0 - smoothstep(ext.x - edge.x, ext.x, d.x)) * (1.0 - smoothstep(ext.y - edge.y, ext.y, d.y));

                glow = smoothstep(0.25, 0.7, p) * (1.0 - smoothstep(0.85, 1.0, p));
                return mask * (1.0 - smoothstep(0.92, 1.0, p));
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

                float glow;
                float power = PowerMask(input.uv, _PowerOff, _PowerFill.xy, glow);
                color = saturate(color + glow * power) * power;

                return half4(saturate(color), 1);
            }
            ENDHLSL
        }
    }
}
