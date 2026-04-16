Shader "Game/FogOfWar"
{
	Properties
	{
		_FogColor		("Fog Color", Color)				= (0, 0, 0, 0.75)
		_NoiseTex		("Noise Tex", 2D)					= "gray" {}
		_NoiseIntensity	("Noise Intensity", Float)			= 0.15
		_NoiseScale		("Noise Scale", Float)				= 0.5
		_EdgeSoftness   ("Edge Softness", Range(0.01, 1))   = 0.35

		[Header(Halftone)]
		_DotDensity     ("Dot Density", Float)              = 8.0
		_DotMaxRadius   ("Dot Max Radius", Range(0.3, 1.0)) = 0.75
		_DotSoftness    ("Dot Edge Softness", Range(0.0, 0.15)) = 0.03
	}

	SubShader
	{
		Tags
		{
			"RenderType"		= "Transparent"
			"Queue"				= "Transparent+100"
            "RenderPipeline"	= "UniversalPipeline"
		}

		Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

		Pass
		{
			Name "FogOfWar"

			HLSLPROGRAM
			#pragma vertex Vert
            #pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#define MAX_UNITS 32

			struct Attributes
            {
                float4 positionOS : POSITION;
            };

			struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 worldPos   : TEXCOORD0;  // only need XY for 2D
            };

			TEXTURE2D(_VisibilityTex);
			SAMPLER(sampler_VisibilityTex);

			TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

			CBUFFER_START(UnityPerMaterial)
                half4  _FogColor;
                float  _NoiseIntensity;
                float  _NoiseScale;
                float  _EdgeSoftness;

				// Halftone parameters
                float  _DotDensity;
                float  _DotMaxRadius;
                float  _DotSoftness;

                float4 _MapParams; // (1.0/mapWidth, 1.0/mapHeight, mapWidth, mapHeight)
                float4 _GridOrigin;
                float4 _InvBasisRow0;
                float4 _InvBasisRow1;

				float4 _UnitEllipseRadius;
				float4 _UnitCenterOffset;
				float  _UnitMaskSoftness;
            CBUFFER_END

			float4 _UnitPositions[MAX_UNITS];
			int    _UnitCount;

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
            	// World → grid UV
                float2 offset = IN.worldPos - _GridOrigin.xy;
                float2 gridCoords = float2(
					dot(offset, _InvBasisRow0.xy),
					dot(offset, _InvBasisRow1.xy)
				);
                float2 visUV = gridCoords * _MapParams.xy;

                float inside = step(0.0, visUV.x) * step(0.0, visUV.y) * step(visUV.x, 1.0) * step(visUV.y, 1.0);

            	float visibility = SAMPLE_TEXTURE2D(_VisibilityTex, sampler_VisibilityTex, visUV).r;
                visibility *= inside;

                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.worldPos * _NoiseScale).r;
                noise = (noise - 0.5) * _NoiseIntensity;

                float bandCenter = 0.5 + noise;
                float halfBand = _EdgeSoftness * 0.5;
                float clearAmount = smoothstep(bandCenter - halfBand, bandCenter + halfBand, visibility);

				float unitMask = 0.0;
				float innerEdge = 1.0 - _UnitMaskSoftness;

				[loop]
				for (int i = 0; i < _UnitCount; i++)
				{
					float2 center = _UnitPositions[i].xy + _UnitCenterOffset.xy;
					float2 delta  = IN.worldPos - center;
					float2 scaled = delta / _UnitEllipseRadius.xy;
					float  dist   = length(scaled);
					float m = 1.0 - smoothstep(innerEdge, 1.0, dist);
					unitMask = max(unitMask, m);
				}

				float totalClear = max(clearAmount, unitMask);

            	float2 dotUV = IN.worldPos * _DotDensity;
            	float2 cellFrac = frac(dotUV) - 0.5;
            	float distToCenter = length(cellFrac);
            	float dotRadius = (1.0 - totalClear) * _DotMaxRadius;
				float dotAlpha = 1.0 - smoothstep(dotRadius - _DotSoftness, dotRadius + _DotSoftness, distToCenter);

                float fogAlpha = dotAlpha * _FogColor.a;

				return half4(_FogColor.rgb, fogAlpha);
            }


			ENDHLSL
		}
	}
}
