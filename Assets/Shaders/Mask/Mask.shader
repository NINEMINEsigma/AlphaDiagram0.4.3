Shader "AD/Effect/ImageMask"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _ToneColor("ToneColor",Color) = (1,1,1,1)
        _ToneIntensity("ToneIntensity", Range(-1,1)) = 0
        _Saturation("Saturation",Range(0,1)) = 1
        _Contrast("Contrast",Float) = 1

        _StencilComp ("Stencil Comparison", Range(0,8)) = 8
        _Stencil ("Stencil ID", Range(0,255)) = 0
        _StencilOp ("Stencil Operation", Range(0,8)) = 0
        _StencilWriteMask ("Stencil Write Mask", Range(0,255)) = 255
        _StencilReadMask ("Stencil Read Mask", Range(0,255)) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _DistanceKey("Gaussian Blur Key",Range(0,0.1))=0.001
        _Distance("Gaussian Blur Level",Range(0,1))=0
        
        _low_xClip("Left",Range(0,1))=0
        _high_xClip("Right",Range(0,1))=1
        _low_yClip("Bottom",Range(0,1))=0
        _high_yClip("Top",Range(0,1))=1
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        //_StencilComp==8 _StencilOp==2 mask
        //_StencilComp==3 _StencilOp==0 sub

        //_StencilComp
        //Never	1	�Ӳ���Ⱦ���ء�
        //Less	2	�ڲο�ֵС��ģ�建�����еĵ�ǰֵʱ��Ⱦ���ء�
        //Equal	3	�ڲο�ֵ����ģ�建�����еĵ�ǰֵʱ��Ⱦ���ء�
        //LEqual	4	�ڲο�ֵС�ڻ����ģ�建�����еĵ�ǰֵʱ��Ⱦ���ء�
        //Greater	5	�ڲο�ֵ����ģ�建�����еĵ�ǰֵʱ��Ⱦ���ء�
        //NotEqual	6	�ڲο�ֵ��ģ�建�����еĵ�ǰֵ��ͬʱ��Ⱦ���ء�
        //GEqual	7	�ڲο�ֵ���ڻ����ģ�建�����еĵ�ǰֵʱ��Ⱦ���ء�
        //Always	8	ʼ����Ⱦ���ء�

        //_StencilOp
        //Keep	0	����ģ�建�����ĵ�ǰ���ݡ�
        //Zero	1	�� 0 д��ģ�建������
        //Replace	2	���ο�ֵд�뻺������
        //IncrSat	3	�����������еĵ�ǰֵ�������ֵ�Ѿ��� 255���򱣳�Ϊ 255��
        //DecrSat	4	�ݼ��������еĵ�ǰֵ�������ֵ�Ѿ��� 0���򱣳�Ϊ 0��
        //Invert	5	���������е�ǰֵ������λ�󷴡�
        //IncrWrap	7	�����������еĵ�ǰֵ�������ֵ�Ѿ��� 255�����Ϊ 0��
        //DecrWrap	8	�ݼ��������еĵ�ǰֵ�������ֵ�Ѿ��� 0�����Ϊ 255�� 
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = IN.texcoord;
                
                #ifdef UNITY_HALF_TEXEL_OFFSET
                OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1) * OUT.vertex.w;
                #endif
                
                OUT.color = IN.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;
            half _ToneIntensity;
            half _Saturation;
            half _Contrast;
            fixed3 _ToneColor;
            half _Distance;
            half _DistanceKey;
            
            half _low_xClip;
            half _high_xClip;
            half _low_yClip;
            half _high_yClip;

            //�ص���Ƭ����ɫ���Ƕ�
            fixed4 frag(v2f IN) : SV_Target
            {
                clip(IN.texcoord.x-_low_xClip);
                clip(_high_xClip-IN.texcoord.x);
                clip(IN.texcoord.y-_low_yClip);
                clip(_high_yClip-IN.texcoord.y);
                
                fixed4 renderTex = tex2D(_MainTex, IN.texcoord);
                half4 color = (renderTex + _TextureSampleAdd) * IN.color;
                fixed3 finalColor;
                //��_ToneIntensityԽ����-1������ɫԽ�ӽ���ɫ����_ToneIntensityԽ����1������ɫԽ�ӽ�_ToneColor����_ToneIntensityΪ0����Ϊԭ��ɫ
                int test_tone=step(_ToneIntensity,0);
                finalColor=(color + color*_ToneIntensity)*test_tone+(1-test_tone)*(color + (_ToneColor-color)*_ToneIntensity);
                //�ҶȻ������鹫ʽ
                fixed gray = dot(fixed3(0.2154,0.7154,0.0721),finalColor);
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                finalColor = lerp(gray,finalColor,_Saturation);
                //lerp�����������Բ�ֵ����������������������ܶ�Ч�������������߶Աȶȣ�
                finalColor = lerp(0.5,finalColor,_Contrast);
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                //��˹ģ��
                 //#ifdef UNITY_UI_GAUSSIAN_BLUR
                half distance = _Distance*_DistanceKey;
                finalColor = finalColor*0.09474;
                finalColor += tex2D(_MainTex, half2(IN.texcoord.x + distance , IN.texcoord.y)) *0.11831;
                finalColor += tex2D(_MainTex, half2(IN.texcoord.x , IN.texcoord.y + distance )) *0.11831;
                finalColor += tex2D(_MainTex, half2(IN.texcoord.x - distance , IN.texcoord.y - distance )) *0.09474;
                finalColor += tex2D(_MainTex, half2(IN.texcoord.x + distance , IN.texcoord.y - distance )) *0.09474;
                finalColor += tex2D(_MainTex, half2(IN.texcoord.x - distance , IN.texcoord.y + distance )) *0.09474;
                finalColor += tex2D(_MainTex, half2(IN.texcoord.x - distance , IN.texcoord.y)) *0.11831;
                finalColor += tex2D(_MainTex, half2(IN.texcoord.x , IN.texcoord.y - distance )) *0.11831;
                // return fixed4(1,1,1, color.a);  
                
                  // #endif
                return fixed4(finalColor, color.a);  
            }
        ENDCG
        }
    }
}
