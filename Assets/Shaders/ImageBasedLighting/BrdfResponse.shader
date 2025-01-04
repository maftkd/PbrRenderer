Shader "Hidden/BrdfResponse"
{
    Properties
    {
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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float GeometrySchlickGGX(float NdotV, float roughness)
            {
                // note that we use a different k for IBL
                float a = roughness;
                float k = (a * a) / 2.0;

                float nom   = NdotV;
                float denom = NdotV * (1.0 - k) + k;

                return nom / denom;
            }
            // ----------------------------------------------------------------------------
            float GeometrySmith(float3 normal, float3 view, float3 light, float roughness)
            {
                float NdotV = max(dot(normal, view), 0.0);
                float NdotL = max(dot(normal, light), 0.0);
                float ggx2 = GeometrySchlickGGX(NdotV, roughness);
                float ggx1 = GeometrySchlickGGX(NdotL, roughness);

                return ggx1 * ggx2;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float nDotV = IN.uv.x;
                float roughness = IN.uv.y;
                
                float3 view = 0;
                view.x = sqrt(1 - nDotV * nDotV);
                view.y = 0;
                view.z = nDotV;

                float scale = 0;
                float offset = 0;

                float3 normal = float3(0,0,1);
                uint sampleCount = 1024;
                for(uint i = 0; i < sampleCount; i++)
                {
                    float2 xi = hammersley(i, sampleCount);
                    float3 halfway = importanceSampleGGX(xi, normal, roughness);
                    float3 light = normalize(2 * dot(view, halfway) * halfway - view);

                    float nDotL = saturate(light.z);
                    float nDotH = saturate(halfway.z);
                    float vDotH = saturate(dot(view, halfway));

                    if(nDotL > 0)
                    {
                        float geometryTerm = GeometrySmith(normal, view, light, roughness);
                        float g_vis = (geometryTerm * vDotH) / (nDotH * nDotV);
                        float fc = pow(1 - vDotH, 5);

                        scale += (1 - fc) * g_vis;
                        offset += fc * g_vis;
                    }
                }

                scale /= (float)sampleCount;
                offset /= (float)sampleCount;

                return float4(scale, offset, 0, 1);
            }
            ENDCG
        }
    }
}
