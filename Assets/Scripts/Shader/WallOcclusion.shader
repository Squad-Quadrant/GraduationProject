Shader "Game/WallOcclusion"
{
	Properties
	{
		_SilhouetteStrength("Silhouette Strength", Range(0, 2)) = 0.6
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		Cull Off
		ZWrite Off
		ZTest Always
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
				half2 uv : TEXCOORD0;
			};

			TEXTURE2D_X(_UnitColorTex);
			SAMPLER(sampler_linear_clamp_UnitColorTex);
			TEXTURE2D_X(_WallMaskTex);
			SAMPLER(sampler_linear_clamp_WallMaskTex);

			float _SilhouetteStrength;

			Varyings vert(Attributes IN)
			{
				Varyings output;
				output.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
				output.uv         = GetFullScreenTriangleTexCoord(IN.vertexID);
				return output;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				half4 unitColor = SAMPLE_TEXTURE2D_X(_UnitColorTex, sampler_linear_clamp_UnitColorTex, IN.uv);
				half wallMask = SAMPLE_TEXTURE2D_X(_WallMaskTex, sampler_linear_clamp_WallMaskTex, IN.uv).r;

				half overlap = unitColor.a * wallMask;

				// final = rgb * alpha + cameraColor * (1 - alpha)
				half3 rgb   = unitColor.rgb;
				half  alpha = overlap * _SilhouetteStrength;

				return half4(rgb, alpha);
			}
			ENDHLSL
		}
	}
}
