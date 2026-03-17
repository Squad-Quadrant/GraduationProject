Shader "Game/FogOfWar"
{
	Properties
	{
		_FogColor		("Fog Color", Color)				= (0, 0, 0, 0.75)
		_NoiseTex		("Noise Tex", 2D)					= "gray" {}
		_NoiseIntensity	("Noise Intensity", Float)	= 0.15
		_NoiseScale		("Noise Scale", Float)				= 0.5
		_EdgeSoftness   ("Edge Softness", Range(0.01, 1))  = 0.35
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
                float4 _MapParams; // (1.0/mapWidth, 1.0/mapHeight, mapWidth, mapHeight)
                float4 _GridOrigin;
                float4 _InvBasisRow0;
                float4 _InvBasisRow1;
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

                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.worldPos * _NoiseScale).r;
                noise = (noise - 0.5) * _NoiseIntensity;

                float bandCenter = 0.5 + noise;
                float halfBand = _EdgeSoftness * 0.5;
                float clearAmount = smoothstep(bandCenter - halfBand, bandCenter + halfBand, visibility);

            	float fogAlpha = (1.0 - clearAmount) * _FogColor.a;

                return half4(_FogColor.rgb, fogAlpha);
            }


			ENDHLSL
		}
	}
}
