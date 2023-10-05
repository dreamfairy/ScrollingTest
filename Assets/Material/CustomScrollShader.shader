Shader "Unlit/CustomScrollShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ZWrite On
            ZTest Always
            CGPROGRAM
            #pragma enable_d3d11_debug_symbols 
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
                o.uv = v.uv - float2(_BlitST.z, _BlitST.w);
                return o;
            }

            float4 frag (v2f i, float depth : SV_Depth) : SV_Target
            {
                float4 col = tex2D(_SrcDepth, i.uv);
                depth = col.r;

                return col;
            }
            ENDCG
        }
    }
}
