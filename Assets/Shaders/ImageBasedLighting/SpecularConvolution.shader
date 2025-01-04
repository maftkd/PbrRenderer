Shader "Unlit/SpecularConvolution"
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

            fixed4 frag (v2f IN) : SV_Target
            {
                float3 normal = normalize(IN.localPos);
                float3 reflection = normal;
                float3 view = reflection;

                const uint numSamples = 1024;
                float totalWeight = 0;
                float3 prefilteredColor = 0;

                for(uint i = 0; i < numSamples; i++)
                {
                    float2 xi = hammersley(i, numSamples);
                    float3 halfway = importanceSampleGGX(xi, normal, _BakeRoughness);
                    float3 light = normalize(2 * dot(view, halfway) * halfway - view);

                    float nDotL = saturate(dot(normal, light));
                    if(nDotL > 0)
                    {
                        prefilteredColor += texCUBE(_UnfilteredEnvironment, light).rgb * nDotL;
                        totalWeight += nDotL;
                    }
                }

                prefilteredColor /= totalWeight;
                return float4(prefilteredColor.rgb, 1);
            }
            ENDCG
        }
    }
}
