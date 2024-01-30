Shader "AD/infTutorialDouble"
{
    Properties
    {
        //_PlaneLevel("High",float) = 0
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
            ZWrite Off
            Cull off
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            //float _PlaneLevel;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct Varings
            {
                float4 positionCS : SV_POSITION;
                float3 nearPoint : TEXCOORD0;
                float3 farPoint : TEXCOORD1;
            };
            float3 TransformHClipToWorld(float3 positionCS, float4x4 inv_VP) 
            {
                float4 unprojectedPoint =  mul(inv_VP, float4(positionCS, 1.0));
                return unprojectedPoint.xyz / unprojectedPoint.w;
            }
            Varings vert(Attributes input)
            {
                //��shaderר����quad mesh
                //����ʹ��4�������uvֵ���б任 ��Ϊ ���пռ������
                //��֤����һ������ȫ��Ļ����Ⱦ
                Varings o;
                float2 uv = input.uv * 2.0 - 1.0;
                //Ĭ������£�Zndc = 1��Զƽ��
                half farPlane = 1;
                half nearPlane = 0;

                #if defined(UNITY_REVERSED_Z)
                    //��ʱ��ᷴתz
                    farPlane = 1 - farPlane;
                    nearPlane = 1 - nearPlane;
                #endif

                float4 position = float4(uv, farPlane, 1);
                float3 nearPoint = TransformHClipToWorld(float3(position.xy, nearPlane), UNITY_MATRIX_I_VP); 
                float3 farPoint = TransformHClipToWorld(float3(position.xy, farPlane), UNITY_MATRIX_I_VP); 
                o.positionCS = position;
                o.nearPoint = nearPoint;
                o.farPoint = farPoint;
                return o;
            }
            float computeViewZ(float3 pos) {
                float4 clip_space_pos = mul(UNITY_MATRIX_VP, float4(pos.xyz, 1.0));
                float viewZ = clip_space_pos.w; //����projection�����壬positionCS.w = viewZ
                return viewZ;
            }
            half Grid(float2 uv){
                float2 derivative = fwidth(uv);
                uv = frac(uv - 0.5); //���Ķ���
                uv = abs(uv - 0.5);
                uv = uv / derivative;
                float min_value = min(uv.x, uv.y);
                half grid = 1.0 - min(min_value, 1.0);
                return grid;
            }
            half4 frag(Varings input) : SV_TARGET{
                //�����ƽ��
                float t = -input.nearPoint.y / (input.farPoint.y - input.nearPoint.y);
                //float3 positionWS = input.nearPoint + t * (input.farPoint - input.nearPoint);
                //half ground = step(_PlaneLevel, t);
                float3 positionWS = input.nearPoint + t * (input.farPoint - input.nearPoint);
                half ground = step(-0.05, t);

                float3 cameraPos = _WorldSpaceCameraPos;
                float fromOrigin = abs(cameraPos.y);

                float viewZ = computeViewZ(positionWS);
                float2 uv = positionWS.xz;
                //����grid
                float fading = max(0.0, 1.0 - viewZ / 150);
                half smallGrid = Grid(uv) * lerp(1, 0, min(1.0, fromOrigin / 100)); 
                half middleGrid  = Grid(uv * 0.1) * lerp(1, 0, min(1.0, fromOrigin / 300)); 
                half largeGrid = Grid(uv * 0.01) * lerp(1, 0, min(1.0, fromOrigin / 800)); 

                //�ϲ�����
                half grid = smallGrid + middleGrid + largeGrid;
                return half4(0.5, 0.5, 0.5 ,ground * grid * fading * 0.5); //˳���ɫ����һ��
            }
            ENDHLSL
        }
    }
}