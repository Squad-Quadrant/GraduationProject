Shader "Game/AnimatedBackground"
{
    Properties
    {
		[HideInInspector] _MainTex ("Base (RGB)", 2D) = "white" {}

        [Header(Mesh Gradient)]
        _GradColor0 ("Color 0 (dominant dark)", Color) = (0.018, 0.030, 0.060, 1)
        _GradColor1 ("Color 1 (cool mid)",      Color) = (0.040, 0.080, 0.130, 1)
        _GradColor2 ("Color 2 (steel blue)",    Color) = (0.055, 0.105, 0.155, 1)
        _GradColor3 ("Color 3 (deep indigo)",   Color) = (0.025, 0.045, 0.090, 1)
        _GradDriftSpeed  ("Drift Speed",  Range(0, 0.5)) = 0.06
        _GradDriftRadius ("Drift Radius", Range(0, 0.6)) = 0.25
        _GradFalloff     ("Blend Falloff (higher = sharper blobs)", Range(1, 30)) = 5

        [Header(Grid)]
        _GridColor         ("Grid Color",         Color) = (0.45, 0.55, 0.65, 0.12)
        _GridDensity       ("Grid Density (cells per screen height)", Range(1, 30)) = 6
        _GridLineThickness ("Grid Line Thickness",  Range(0.001, 0.05)) = 0.006
        _GridDashCount     ("Dashes per cell edge", Range(1, 40))       = 12

        [Header(Contours)]
        _ContourColor     ("Contour Color",     Color) = (0.55, 0.70, 0.85, 0.22)
        _ContourScale     ("Noise Scale",       Range(0.5, 10)) = 2.5
        _ContourLevels    ("Level Count",       Range(4, 40))   = 14
        _ContourThickness ("Line Thickness",    Range(0.01, 5.0)) = 1.5
        _ContourEvolveSpeed ("Evolve Speed (time axis)", Range(0, 1)) = 0.08

    	[Header(Contour Hierarchy)]
        _ContourMajorEvery    ("Major Line Every N", Range(2, 10)) = 5
        _ContourMajorScale    ("Major Line Width Multiplier", Range(1, 5)) = 2.5
        _ContourMinorIntensity("Minor Line Intensity", Range(0, 1)) = 0.55

        // 基于 noise 值的线宽微调（破除完美均匀）
        _ContourElevationMod ("Elevation Modulation", Range(0, 1)) = 0.4

    	[Header(Parallax)]
        [HideInInspector] _ParallaxOffset ("Parallax Offset", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        ZWrite Off
        Cull   Off
        Blend  SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            float4 _GradColor0, _GradColor1, _GradColor2, _GradColor3;
            float  _GradDriftSpeed, _GradDriftRadius, _GradFalloff;

            float4 _GridColor;
            float  _GridDensity, _GridLineThickness, _GridDashCount;

            float4 _ContourColor;
            float  _ContourScale, _ContourLevels, _ContourThickness, _ContourEvolveSpeed;

            float  _ContourMajorEvery, _ContourMajorScale, _ContourMinorIntensity;
            float  _ContourElevationMod;

			float4 _ParallaxOffset;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // 3D hash -> [0,1)
            // 做法和 2D 版本同理：线性组合 + frac 混淆。
            // 魔数经过视觉验证，没有唯一正解。
            float hash3(float3 p)
            {
                p = frac(p * float3(443.897, 441.423, 437.195));
                p += dot(p, p.yzx + 19.19);
                return frac((p.x + p.y) * p.z);
            }

            // 3D Value Noise
            float valueNoise3(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                float3 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

                float c000 = hash3(i + float3(0,0,0));
                float c100 = hash3(i + float3(1,0,0));
                float c010 = hash3(i + float3(0,1,0));
                float c110 = hash3(i + float3(1,1,0));
                float c001 = hash3(i + float3(0,0,1));
                float c101 = hash3(i + float3(1,0,1));
                float c011 = hash3(i + float3(0,1,1));
                float c111 = hash3(i + float3(1,1,1));

                float x00 = lerp(c000, c100, u.x);
                float x10 = lerp(c010, c110, u.x);
                float x01 = lerp(c001, c101, u.x);
                float x11 = lerp(c011, c111, u.x);

                float y0 = lerp(x00, x10, u.y);
                float y1 = lerp(x01, x11, u.y);

                return lerp(y0, y1, u.z);
            }

            float fbm3(float3 p)
            {
                float v = 0;
                float a = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += a * valueNoise3(p);
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            float3 sampleMeshGradient(float2 uv, float t)
            {
                float s = _GradDriftSpeed;
                float r = _GradDriftRadius;

                float2 p0 = float2(0.25, 0.30) + float2(sin(t * s * 1.13), cos(t * s * 0.97)) * r;
                float2 p1 = float2(0.75, 0.35) + float2(cos(t * s * 0.87), sin(t * s * 1.23)) * r;
                float2 p2 = float2(0.30, 0.75) + float2(sin(t * s * 1.07), sin(t * s * 0.83)) * r;
                float2 p3 = float2(0.80, 0.70) + float2(cos(t * s * 0.93), cos(t * s * 1.17)) * r;

                float d0 = dot(uv - p0, uv - p0);
                float d1 = dot(uv - p1, uv - p1);
                float d2 = dot(uv - p2, uv - p2);
                float d3 = dot(uv - p3, uv - p3);

                float w0 = exp(-d0 * _GradFalloff);
                float w1 = exp(-d1 * _GradFalloff);
                float w2 = exp(-d2 * _GradFalloff);
                float w3 = exp(-d3 * _GradFalloff);

                float wSum = w0 + w1 + w2 + w3;
                float3 col = (_GradColor0.rgb * w0
                            + _GradColor1.rgb * w1
                            + _GradColor2.rgb * w2
                            + _GradColor3.rgb * w3) / wSum;
                return col;
            }

            float sampleDashedGrid(float2 uv)
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 cellUV = float2(uv.x * aspect, uv.y) * _GridDensity;

                float2 f = frac(cellUV);
                float2 dist = min(f, 1.0 - f);

                float vLine = step(dist.x, _GridLineThickness);
                float hLine = step(dist.y, _GridLineThickness);

                float vDash = step(0.5, frac(cellUV.y * _GridDashCount));
                float hDash = step(0.5, frac(cellUV.x * _GridDashCount));

                return max(vLine * vDash, hLine * hDash);
            }

            float sampleContours(float2 uv, float t)
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = float2(uv.x * aspect, uv.y) * _ContourScale;

                float n = fbm3(float3(p, t * _ContourEvolveSpeed));
                float v = n * _ContourLevels;

                float lineIdx = floor(v);
                float majorEvery = max(_ContourMajorEvery, 1.0);
                float modResult = lineIdx - majorEvery * floor(lineIdx / majorEvery);
                float isMajor = 1.0 - step(0.5, modResult);

                float elevationMod = lerp(1.0 - _ContourElevationMod,
                                          1.0 + _ContourElevationMod,
                                          saturate(n));

                float widthMult = lerp(1.0, _ContourMajorScale, isMajor) * elevationMod;

                float f = frac(v);
                float distToLine = min(f, 1.0 - f);

                float w = fwidth(v);
                float edge = _ContourThickness * widthMult * w * 0.5;

                float line1 = 1.0 - smoothstep(0.0, edge, distToLine);

                float intensity = lerp(_ContourMinorIntensity, 1.0, isMajor);

                return line1 * intensity;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y;

                float2 parallaxUV = uv + _ParallaxOffset.xy;

                float3 col = sampleMeshGradient(parallaxUV, t);

                float grid = sampleDashedGrid(uv);
                col = lerp(col, _GridColor.rgb, grid * _GridColor.a);

                float contour = sampleContours(parallaxUV, t);
                col = lerp(col, _ContourColor.rgb, contour * _ContourColor.a);

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
