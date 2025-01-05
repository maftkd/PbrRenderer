Shader "Hidden/IconGenerator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            samplerCUBE_half _UnfilteredEnvironment;

            fixed4 frag (v2f i) : SV_Target
            {
                float3 ray = 0;
                ray.z = 1;
                ray.x = i.uv.x * 2 - 1;
                ray.y = i.uv.y * 2 - 1;
                ray = normalize(ray);
                fixed4 col = texCUBE(_UnfilteredEnvironment, ray);
                float4 decodeInstructions = float4(5, 1, 0, 1);
                col.rgb = DecodeHDR(col, decodeInstructions);
                col.a = 1;
                
                return col;
            }
            ENDCG
        }
    }
}
