Shader "AD/uvTutorial"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{
            "RenderPipeline"="UniversalRenderPipeline"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "Queue"="Transparent"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite off
            Cull off
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes{
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varings{
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _fragSize;
            float _frageCenter;

            Varings vert(Attributes input){
                Varings o;
                o.positionCS = TransformObjectToHClip(input.positionOS);
                o.uv = input.uv;
                return o;
            }
            half4 frag(Varings input) : SV_TARGET{
                float2 uv = input.uv * 10;
                float2 derivative = fwidth(uv);
                uv = frac(uv);
                uv = abs(uv - 0.5);
                uv = uv / derivative; //���￪ʼ�����������ţ�dԽ��uvԽС�����п����С���alphaֵ��
                float min_value = min(uv.x, uv.y); 
                half alpha = 1.0 - min(min_value, 1.0); //ʵ�ʵ��߿��ǿ���x,y��ģ�
                //Զ��x, y���uvֵ��min�����1��ȡ������0
                return half4(1.0, 0.0, 0.0, alpha);
            }
            ENDHLSL
        }
    }
}