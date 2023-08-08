Shader "Unlit/CustomScrollShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _SrcDepth;
            float4 _BlitST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv + _BlitST.zw;
                return o;
            }

            float frag (v2f i) : SV_Depth
            {
                float4 col = tex2D(_SrcDepth, i.uv);
                return col.r;
            }
            ENDCG
        }
    }
}
