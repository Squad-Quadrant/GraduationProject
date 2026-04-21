Shader "Game/OutlinePostProcessing"
{
	Properties
	{
		[HDR] _OutlineColor("Outline Color", Color) = (1,1,1,1)
		_OutlineWidth("Outline Width", Range(0, 0.005)) = 0.002
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
			{
				uint vertexID : SV_VertexID;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				half2 uv         : TEXCOORD0;
				half2 offsets[8]  : TEXCOORD1; // 8 offsets for sampling neighboring pixels
			};

			TEXTURE2D_X(_OutlineMask);
			SAMPLER(sampler_linear_clamp_OutlineMask);

			half4 _OutlineColor;
			float _OutlineWidth;

			Varyings vert(Attributes IN)
			{
				Varyings output;
				output.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
				output.uv = GetFullScreenTriangleTexCoord(IN.vertexID);

				const half aspect_ratio = _ScreenParams.x / _ScreenParams.y;
				const half diagonal_correction = 0.7071;
				output.offsets[0] = half2(-1, aspect_ratio) * _OutlineWidth * diagonal_correction; // Top-left
				output.offsets[1] = half2(0, aspect_ratio) * _OutlineWidth;  // Top
				output.offsets[2] = half2(1, aspect_ratio) * _OutlineWidth * diagonal_correction;  // Top-right
				output.offsets[3] = half2(-1, 0) * _OutlineWidth; // Left
				output.offsets[4] = half2(1, 0) * _OutlineWidth;  // Right
				output.offsets[5] = half2(-1, -aspect_ratio) * _OutlineWidth * diagonal_correction; // Bottom-left
				output.offsets[6] = half2(0, -aspect_ratio) * _OutlineWidth;  // Bottom
				output.offsets[7] = half2(1, -aspect_ratio) * _OutlineWidth * diagonal_correction;  // Bottom-right
				return output;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				const half kernel_x[8] = {
					-1, 0, 1,
					-2,    2,
					-1, 0, 1
				};
				const half kernel_y[8] = {
					-1, -2, -1,
					 0,      0,
					 1,  2,  1
				};
				half gx = 0;
				half gy = 0;
				for (int i = 0; i < 8; i++)
				{
					half mask = SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, IN.uv + IN.offsets[i]).a;
					gx += mask * kernel_x[i];
					gy += mask * kernel_y[i];
				}
				const half alpha = SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, IN.uv).a;
				half4 color = _OutlineColor;
				color.a = saturate(abs(gx) + abs(gy)) * (1 - alpha); // Outline strength based on edge detection and original alpha
				return color;
			}

			ENDHLSL
		}
	}
}
