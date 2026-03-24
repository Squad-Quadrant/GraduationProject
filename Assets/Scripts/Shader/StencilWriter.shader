Shader "Game/StencilWriter"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {}
	}
	SubShader
	{
		ColorMask 0
		ZWrite Off
		Stencil {
			Ref 1
			Comp Always
			Pass Replace
		}

		Pass
		{
		}
	}
}
