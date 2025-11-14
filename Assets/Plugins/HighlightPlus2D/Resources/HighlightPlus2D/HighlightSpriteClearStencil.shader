Shader "HighlightPlus2D/Sprite/ClearStencil" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
}
    SubShader
    {
        Tags { "Queue"="Transparent+4" "RenderType"="Transparent" "DisableBatching"="True" }

        // Create mask
        Pass
        {
			Stencil {
                Ref 32
                ReadMask 32
                WriteMask 32
                Comp always
                Pass zero
            }
            ColorMask 0
            ZWrite Off
            Offset -1, -1
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			struct V2F
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

            V2F vert (appdata v)
            {
                V2F o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(V2F, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                v.vertex.xy *= 2.0;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (float4 position : SV_POSITION) : SV_Target
            {
                return 0;
            }
            ENDCG
        }

    }
}