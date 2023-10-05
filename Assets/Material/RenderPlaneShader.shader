Shader "Unlit/RenderPlaneShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D FinalDepth;
            float4x4 _ShadowVP;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 shadowPos = mul(_ShadowVP, float4(i.worldPos, 1));
                shadowPos.xy = shadowPos.xy * 0.5 + 0.5;
                //shadowPos.y = 1 - shadowPos.y;

                float d = 1- tex2D(FinalDepth, shadowPos.xy).r;
                
                fixed4 col = tex2D(_MainTex, i.uv);

                col.rgb *= 1 - d;
                
                return col;
            }
            ENDCG
        }
    }
}
