Shader "OMSB/ImageGrayscaleRadial"
{
    Properties
    {
        [PerRendererData] _MainTex("Main Tex", 2D) = "white" {}
        _RevealAmount("Reveal Progress", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _RevealAmount;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.texcoord, center); // 0 (中心) から 約0.707 (隅)

                fixed4 tex = tex2D(_MainTex, i.texcoord) * i.color;

                float grayVal = dot(tex.rgb, float3(0.3, 0.59, 0.11));
                float3 gray = float3(grayVal, grayVal, grayVal);

                // 円形に元の色を戻す：中心からの距離が _RevealAmount 以下ならカラー
                float blend = smoothstep(_RevealAmount, _RevealAmount - 0.1, dist);
                tex.rgb = lerp(gray, tex.rgb, blend);

                return tex;
            }
            ENDCG
        }
    }
}
