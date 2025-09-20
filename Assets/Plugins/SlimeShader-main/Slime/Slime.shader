Shader "Custom/SlimeURP_FullFlowRefraction"
{
    Properties
    {
        _SlimeColor ("Slime Color", Color) = (0.3,1,0.4,0.6)
        _MainTex ("Main Texture", 2D) = "white" {}
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" {}
        _FlowAmount ("Flow Amount", Range(0,0.1)) = 0.05
        _FlowSpeed ("Flow Speed", Range(0,5)) = 1
        _NormalFlowSpeed ("Normal Flow Speed", Range(0,2)) = 0.5
        _NormalFlowStrength ("Normal Flow Strength", Range(0,2)) = 1
        _FresnelPower ("Fresnel Power", Range(0.1,5)) = 2
        _FresnelStrength ("Fresnel Strength", Range(0,1)) = 0.3
        _RefractionStrength ("Refraction Strength", Range(0,0.1)) = 0.05
        _WaveFrequency ("Wave Frequency", Range(1,20)) = 5
        _Alpha ("Alpha",Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1001" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _SlimeColor;
                float _FlowAmount;
                float _FlowSpeed;
                float _NormalFlowSpeed;
                float _NormalFlowStrength;
                float _FresnelPower;
                float _FresnelStrength;
                float _RefractionStrength;
                float _WaveFrequency;
                float _Alpha;
            CBUFFER_END

            // フラクタルノイズ関数
            float hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453123); }
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f*f*(3.0-2.0*f);
                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));
                return lerp(lerp(a,b,f.x), lerp(c,d,f.x), f.y);
            }
            float fbm(float2 p)
            {
                float value=0; float amp=0.5; float freq=1;
                for(int i=0;i<4;i++) { value+=amp*noise(p*freq); amp*=0.5; freq*=2; }
                return value;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float3 worldPosOriginal : TEXCOORD4;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.worldPosOriginal = posWS;

                float time = _Time.y * _FlowSpeed;

                // フラクタル＋波でぬめぬめ
                float n = fbm(posWS.xz*0.5 + time*0.1);
                float wave = sin(posWS.x*_WaveFrequency + time)*cos(posWS.z*_WaveFrequency + time);
                posWS += normalWS * (wave + n) * _FlowAmount;

                OUT.positionCS = TransformWorldToHClip(posWS);
                OUT.positionWS = posWS;
                OUT.normalWS = normalWS;
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(posWS);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y * _NormalFlowSpeed;

                // 法線アニメーション
                float2 uv1 = IN.uv + float2(time*0.1, time*0.15);
                float2 uv2 = IN.uv + float2(-time*0.08, time*0.12);
                float3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv1));
                float3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv2));
                float3 normalTS = normalize(n1 + n2*0.5)*_NormalFlowStrength;

                float3 bitangentWS = cross(IN.normalWS, float3(0,1,0)); // 簡易
                float3x3 TBN = float3x3(float3(1,0,0), bitangentWS, IN.normalWS);
                float3 normalWS = normalize(mul(normalTS,TBN));

                // テクスチャ
                float4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float3 albedo = texCol.rgb * _SlimeColor.rgb;

                // 屈折
                float2 screenUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                screenUV += normalWS.xy*_RefractionStrength;
                float3 bg = SampleSceneColor(screenUV);

                // フレネル
                float fresnel = pow(1.0 - saturate(dot(IN.viewDirWS, normalWS)), _FresnelPower);

                // 内部散乱っぽい光
                float glow = fbm(IN.worldPosOriginal.xz*2.0 + time*0.3)*0.3;

                float3 color = lerp(albedo, bg*_SlimeColor.rgb, fresnel*0.5) + fresnel*_FresnelStrength*_SlimeColor.rgb + glow*_SlimeColor.rgb;

                return float4(color, _SlimeColor.a * _Alpha);
            }
            ENDHLSL
        }
    }
}
