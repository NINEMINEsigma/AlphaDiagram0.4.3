Shader "AD/SequenceFramesX/6X6"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainFrames ("Frames", 2D) = "while" {}
        
        [IntRange] _CurrentFramesCount ("Current",Range(0,35)) = 0
        
        [Header(Stencil)]
        //[Enum(Never,1,Less,2,Equal,3,LEqual,4,Greater,5,NotEqual,6,GEqual,7,AlwaysRender,8)] 
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
        [IntRange] _Stencil ("Stencil ID", Range(0,255)) = 0
        //[Enum(Keep,1,Zero,2,Replace,3,IncrSat,4,DecrSat,5,Invert,6,IncrWrap,7,DecrWrap,8)]
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Operation", Float) = 0
        [IntRange] _StencilWriteMask ("Stencil Write Mask", Range(0,255)) = 255
        [IntRange] _StencilReadMask ("Stencil Read Mask", Range(0,255)) = 255

        [Header(ColorMask)]
        [IntRange] _ColorMask ("Color Mask", Range(0,16)) = 15

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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _MainFrames;
            float4 _MainFrames_ST;
            uint _CurrentFramesCount;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainFrames);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                uint _XFramesCount = 6 , _YFramesCount = 6;
                uint counter = clamp(_CurrentFramesCount,0,_XFramesCount*_YFramesCount);
                uint xc = counter % _XFramesCount;
                uint yc = _YFramesCount - (counter - xc) / _YFramesCount;
                float x = _XFramesCount, y = _YFramesCount;
                // sample the texture
                fixed4 col = tex2D(_MainFrames, float2((i.uv.x + xc)/x, (i.uv.y + yc)/y));
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
