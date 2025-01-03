Shader "Unlit/DiffuseConvolution"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 eyeRay : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.eyeRay = normalize(v.vertex.xyz);
                return o;
            }

            samplerCUBE_half _UnfilteredEnvironment;

            fixed4 frag (v2f i) : SV_Target
            {
                float3 irradiance = 0;

                float3 normal = normalize(i.eyeRay);
                float3 up = float3(0.0, 1.0, 0.0);
                float3 right = normalize(cross(up, normal));
                up = normalize(cross(normal, right));

                float sampleDelta = 0.025;
                float nrSamples = 0;

                for(float phi = 0.0; phi < UNITY_TWO_PI; phi += sampleDelta)
                {
                    for(float theta = 0.0; theta < UNITY_HALF_PI; theta += sampleDelta)
                    {
                        //spherical to cartesian in tangent space
                        float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
                        //tangent space to world
                        float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;
                        
                        irradiance += texCUBE(_UnfilteredEnvironment, sampleVec).rgb * cos(theta) * sin(theta);
                        nrSamples++;
                    }
                }


                irradiance = UNITY_PI * irradiance * (1.0 / nrSamples);
                return float4(irradiance, 1.0);
                
                //float3 envColor = texCUBE(_UnfilteredEnvironment, i.eyeRay).rgb;
                //return fixed4(envColor,1);
                return 0;
            }
            ENDCG
        }
    }
}
