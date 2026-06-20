Shader "Game/CRT"
{
    Properties
    {
        [Header(Scanline)]
        [Toggle(_SCANLINE_HARD)] _ScanlineHard ("Hard Scanline (step mode)", Float) = 0
        _ScanlineIntensity    ("Scanline Intensity",                  Range(0, 1)) = 0.4
        _ScanlineDensity      ("Scanline Density (lines per screen)", Float)       = 300
        _ScanlineDarkResponse ("Dark Area Response (0 = hide in dark)", Range(0, 1)) = 0.3

        [Header(Rolling Bar)]
        _RollSpeed     ("Roll Speed (screens per sec)", Range(0, 0.5)) = 0.1
        _RollSharpness ("Roll Bar Sharpness",           Range(1, 64))  = 16
        _RollIntensity ("Roll Bar Intensity",           Range(0, 0.5)) = 0.15

        [Header(Brightness Flicker)]
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.1)) = 0.02

        [Header(Inter Line Chroma)]
        _ChromaShift ("Chroma Shift (UV)", Range(0, 0.01)) = 0.001
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        struct Attributes
        {
            uint vertexID : SV_VertexID;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv         : TEXCOORD0;
        };

        TEXTURE2D_X(_CRTSourceTex);
        SAMPLER(sampler_linear_clamp_CRTSourceTex);

        Varyings vert(Attributes IN)
        {
            Varyings output;
            output.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
            output.uv         = GetFullScreenTriangleTexCoord(IN.vertexID);
            return output;
        }
        ENDHLSL

        // Pass 0: 把 cameraColor 原样拷到临时 RT
        Pass
        {
            Name "Copy"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(Varyings IN) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_CRTSourceTex, sampler_linear_clamp_CRTSourceTex, IN.uv);
            }
            ENDHLSL
        }

        // Pass 1: CRT 主变换
        Pass
        {
            Name "CRT"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ _SCANLINE_HARD

            float _ScanlineIntensity;
            float _ScanlineDensity;
            float _ScanlineDarkResponse;
            float _RollSpeed;
            float _RollSharpness;
            float _RollIntensity;
            float _FlickerIntensity;
            float _ChromaShift;

            half4 frag(Varyings IN) : SV_Target
            {
                // ===== D. 行间色差 =====
                // 偶数扫描线 R 通道向左、B 向右；奇数扫描线 R 向右、B 向左
                // 视觉上模拟电子枪三色汇聚不完美——扫描线之间会出现微弱的红/蓝色彩泄漏
                // 注意：这一步必须在采样阶段完成，因为不同通道用不同的 UV 重新采样
                float lineIndex = floor(IN.uv.y * _ScanlineDensity);
                float oddSign = frac(lineIndex * 0.5) * 4.0 - 1.0; // -1 或 +1
                float2 chromaUVR = IN.uv + float2(oddSign * _ChromaShift, 0);
                float2 chromaUVB = IN.uv - float2(oddSign * _ChromaShift, 0);

                half r = SAMPLE_TEXTURE2D_X(_CRTSourceTex, sampler_linear_clamp_CRTSourceTex, chromaUVR).r;
                half g = SAMPLE_TEXTURE2D_X(_CRTSourceTex, sampler_linear_clamp_CRTSourceTex, IN.uv).g;
                half b = SAMPLE_TEXTURE2D_X(_CRTSourceTex, sampler_linear_clamp_CRTSourceTex, chromaUVB).b;
                half3 col = half3(r, g, b);

                // ===== 扫描线核心：sin² 平滑（默认）或 step 硬切（_SCANLINE_HARD 开启时） =====
                // 两种模式的"明暗周期数"都对齐到 _ScanlineDensity，切换时密度感不变
                #if defined(_SCANLINE_HARD)
                    float scan = step(0.5, frac(IN.uv.y * _ScanlineDensity));
                #else
                    float scan = pow(sin(IN.uv.y * _ScanlineDensity * PI), 2.0);
                #endif

                // ===== C. 扫描线响应画面亮度 =====
                // 暗部扫描线衰减到 _ScanlineDarkResponse，亮部保持满强度
                // BT.601 luminance 公式：人眼对 G 最敏感、B 最不敏感
                half luma = dot(col, half3(0.299, 0.587, 0.114));
                float scanResponse = lerp(_ScanlineDarkResponse, 1.0, luma);
                col *= lerp(1.0, scan, _ScanlineIntensity * scanResponse);

                // ===== A. 垂直滚动亮带 =====
                // frac 让"uv.y - 时间偏移"在 [0,1] 区间循环
                // sin(phase * PI) 在 phase=0.5 处取峰值（= 1）
                // pow N 把钟形锐化成窄亮带；N 越大带越窄
                // 时间项符号为减 → uv.y 峰值随 t 增大 → 亮带向屏幕上方移动 = 自下而上滚动
                float scrollPhase = frac(IN.uv.y - _Time.y * _RollSpeed);
                float roll = pow(sin(scrollPhase * PI), _RollSharpness);
                col *= 1.0 + roll * _RollIntensity;

                // ===== B. 整体亮度抖动 =====
                // 三个无理数比例频率的 sin 叠加，归一化到 [-1,1]
                // 比单一 sin 更没有规律感，比纯随机更平滑——接近"信号不稳"的视觉感受
                float flicker = (sin(_Time.y * 8.7) + sin(_Time.y * 13.1) + sin(_Time.y * 5.3) * 0.6) / 2.6;
                col *= 1.0 + flicker * _FlickerIntensity;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
