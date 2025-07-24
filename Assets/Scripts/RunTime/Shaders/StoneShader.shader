Shader "URP/PetrifyByMaskTexture"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Main Texture", 2D) = "white" {}
        _StoneTex("Stone Texture", 2D) = "white" {}
        _PetrifyMask("Petrify Mask", 2D) = "white" {}
        _PetrifyProgress("Petrify Progress", Range(0,1)) = 0.0
        _BoundaryBlurAmount("Boundary Blur Amount", Range(0,50)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _StoneTex;
            sampler2D _PetrifyMask;
            float4 _MainTex_ST;
            float4 _StoneTex_ST;
            float4 _PetrifyMask_ST;

            float4 _Color;
            float _PetrifyProgress;
            float _BoundaryBlurAmount;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // マスク（青チャンネル）取得
                float maskValue = tex2D(_PetrifyMask, uv).b;
                float blurOffset = 51 - _BoundaryBlurAmount;
                maskValue = maskValue * (blurOffset - 1) / blurOffset;
                float rate = blurOffset * (_PetrifyProgress - maskValue);
                rate = saturate(rate); // clamp between 0 and 1

                float4 mainCol = tex2D(_MainTex, uv);
                float4 stoneCol = tex2D(_StoneTex, uv);
                float4 finalCol = lerp(mainCol, stoneCol, rate) * _Color;

                return finalCol;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
