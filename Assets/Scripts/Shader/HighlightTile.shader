Shader "Game/HighlightTile"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed (UV/s)", Vector) = (0.1, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _ScrollSpeed;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // TRANSFORM_TEX: 应用 Material 上 _MainTex 的 Tiling/Offset（Sprite 通常是 (1,1,0,0)）
                // _Time.y 是游戏开始以来的秒数，乘以速度向量得到滚动偏移
                float2 scrollOffset = _Time.y * _ScrollSpeed.xy;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex) + scrollOffset;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // sprite 纹理（由 Tilemap 运行时注入） × 顶点色（由 HighlightLayer 设置的 per-cell 颜色）
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return tex * IN.color;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
