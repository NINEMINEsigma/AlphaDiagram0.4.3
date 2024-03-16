Shader "Minnes/NoteBase"
{
    Properties
    {
        [Header(Main)]
        _MainTex ("Texture", 2D) = "white" {}
        [PerRendererData] _BaseColor("Renderer Color",Color) = (1,1,1,1)
        [PerRendererData] _SelectColor("Select Color",Color) = (1,1,1,1)
        //_ToneColor("ToneColor",Color) = (1,1,1,1)
        //_ToneIntensity("ToneIntensity", Range(-1,1)) = 0
        //_Saturation("Saturation",Range(0,1)) = 1
        //_Contrast("Contrast",Float) = 1
        [Header(Stencil)]
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
        [IntRange] _Stencil ("Stencil ID", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Operation", Float) = 0
        [IntRange] _StencilWriteMask ("Stencil Write Mask", Range(0,255)) = 255
        [IntRange] _StencilReadMask ("Stencil Read Mask", Range(0,255)) = 255
        [Header(ColorMask)]
        [IntRange] _ColorMask ("Color Mask", Range(0,16)) = 15
        [Header(AlphaClip)]
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
        LOD 100
        
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
            
            fixed4 _BaseColor;
            fixed4 _SelectColor;
            float4 _ClipRect;

            sampler2D _MainTex;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = IN.texcoord;
                
                #ifdef UNITY_HALF_TEXEL_OFFSET
                OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1) * OUT.vertex.w;
                #endif
                
                OUT.color = IN.color * _BaseColor;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 renderTex = tex2D(_MainTex, IN.texcoord);
                //fixed4 renderTex = fixed4(IN.texcoord.x,1,IN.texcoord.y,1);
                //TODO
                //renderTex.b = renderTex.g = renderTex.r = step(renderTex.r, 0.1);
                //TODO END
                fixed4 color = renderTex * IN.color;
                fixed3 finalColor = color;
                fixed gray = dot(fixed3(0.2154,0.7154,0.0721),finalColor);
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                float cutter = 0.3;
                float isSelect = step(IN.texcoord.x,cutter) + step(1-cutter,IN.texcoord.x) + step(IN.texcoord.y,cutter) + step(1-cutter,IN.texcoord.y);
                isSelect = step(1,isSelect);
                finalColor = isSelect * _SelectColor * finalColor + ( 1 - isSelect ) * finalColor;

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return fixed4(finalColor,_BaseColor.a);  
            }
        ENDCG
        }
    }
}
