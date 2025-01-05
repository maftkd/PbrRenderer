Shader "Unlit/BaseCubemapSampler"
{
    Properties
    {
    }
    SubShader
    {
        Cull front
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "ImportanceSampling.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.localPos = v.vertex.xyz;
                return o;
            }

            float _BakeRoughness;
            samplerCUBE_half _UnfilteredEnvironment;
            float4 _UnfilteredEnvironment_HDR;

            float4 frag (v2f IN) : SV_Target
            {
                float3 normal = normalize(IN.localPos);
                float4 col = texCUBE(_UnfilteredEnvironment, normal);
                col.rgb = DecodeHDR(col, _UnfilteredEnvironment_HDR);
                return col;
                
                return float4(normal, 1);
            }
            ENDCG
        }
    }
}
