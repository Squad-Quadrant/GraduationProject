Shader "Pditine/WarFog"
{
    // 这是一个方向性遮罩效果，给一个方向，和一个进度值，可以实现从某个方向逐渐显示或隐藏的效果
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _NoiseTex("_NoiseTex",2D) = "white"{}
        _Progress("Progress",Range(-0.2,1)) = 0
        _DirectionX("DirectionX", Range(0, 1)) = 1
        _DirectionY("DirectionY", Range(0, 1)) = 0
        _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.1
        _NoiseScale("Noise Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float _Progress;
            float _DirectionX;
            float _DirectionY;
            // 新增 CGPROGRAM 变量
            float _NoiseIntensity;
            float _NoiseScale;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                float2 uvDirection = i.uv - float2(0.5, 0.5);
                float2 inDirection = normalize(float2(_DirectionX, _DirectionY));
                
                float offset = -(_Progress - 0.5) * pow(2, 0.5);
                uvDirection = uvDirection + inDirection * offset;
                float cutOff = uvDirection.x * _DirectionX + uvDirection.y * _DirectionY;
                
                // 添加扰动
                float noiseVal = tex2D(_NoiseTex, i.uv * _NoiseScale).r;
                float noiseModifier = noiseVal * _NoiseIntensity;
                cutOff -= noiseModifier;
                
                clip(cutOff);
                
                return color;
            }
            ENDCG
        }
    }
}