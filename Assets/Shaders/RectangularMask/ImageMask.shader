Shader "Unlit/Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _k("K",float)=2
        _d("Pos",Range(-1,1))=0
        _l("l",Range(0,0.5))=0.5
        _h("h",Range(0.5,1))=0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            float _k;
            float _d;
            float _w;
            float _h;
            float _l;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            clip(_k*i.uv.x-i.uv.y+_d);
            clip(i.uv.y-(i.uv.x-0.5)*_k+_d);
            clip(step(_l,i.uv.y)-0.1);
            clip(step(i.uv.y,_h)-0.1);
            //clip(0.5-(i.uv.x*_k-0.5)*2-0.01);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
