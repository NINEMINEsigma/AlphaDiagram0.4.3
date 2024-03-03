Shader "AD/InkGradient"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _StartTex ("Start", 2D) = "white" {}
        _TargetTex ("End", 2D) = "white" {}
        _Process ("Process", Range(0,1)) = 0
        _Process2 ("Process Pro", Range(0,1)) = 0
    }
    SubShader
    {
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent"  }

            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZWrite Off
        
        Pass
        {
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
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _StartTex;
            float4 _StartTex_ST;
            sampler2D _TargetTex;
            float4 _TargetTex_ST;
            half _Process;
            half _Process2;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                half ProcessMain = clamp(_Process , 0 ,1 ) * 0.5 , ProcessPro = clamp(_Process2 , 0 , 1 ) * 0.5;
                half MixProcess =  ProcessMain + ProcessPro;
                fixed4 renderTex = tex2D(_StartTex, IN.texcoord) * (1 - MixProcess) + tex2D(_TargetTex, IN.texcoord) * MixProcess;
                half4 color = (renderTex + _TextureSampleAdd) * IN.color;
                return color;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}
