Shader "Game/Blackout"
{
	Properties
	{
		_BlackoutColor("Blackout Color", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Tags
		{
			"RenderType"     = "Transparent"
			"Queue"          = "Transparent+50"
			"RenderPipeline" = "UniversalPipeline"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

		Pass
		{
			Name "Blackout"

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 worldPos   : TEXCOORD0;
			};

			TEXTURE2D(_VisibilityTex);
			SAMPLER(sampler_VisibilityTex);

			CBUFFER_START(UnityPerMaterial)
				half4 _BlackoutColor;
				float4 _MapParams;    // (1/mapW, 1/mapH, mapW, mapH)
				float4 _GridOrigin;   // world-space grid origin (XY)
				float4 _InvBasisRow0; // inverse basis matrix row 0 (XY)
				float4 _InvBasisRow1; // inverse basis matrix row 1 (XY)
			CBUFFER_END

			Varyings Vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
				float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
				OUT.worldPos = worldPos.xy;
				return OUT;
			}

			half4 Frag(Varyings IN) : SV_Target
			{
				float2 offset = IN.worldPos - _GridOrigin.xy;
				float2 gridCoords = float2(
					dot(offset, _InvBasisRow0.xy),
					dot(offset, _InvBasisRow1.xy)
				);

				float2 visUV = gridCoords * _MapParams.xy;

				float inside = step(0.0, visUV.x) * step(0.0, visUV.y) * step(visUV.x, 1.0) * step(visUV.y, 1.0);

				float visibility = SAMPLE_TEXTURE2D(_VisibilityTex, sampler_VisibilityTex, visUV).r;
				visibility *= inside;

				float alpha = (1.0 - visibility) * inside;

				return half4(_BlackoutColor.rgb, alpha);
			}

			ENDHLSL
		}
	}
}
