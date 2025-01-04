Shader "Unlit/PbrSimple"
{
    Properties
    {
        _Albedo ("Albedo", Color) = (1,1,1,1)
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.5
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
            #include "PbrCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float4 _Albedo;
            float _Roughness;
            float _Metallic;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = 0;
                float3 lightOut = 0;
                
                float3 normal = normalize(i.worldNormal);
                float3 view = normalize(_WorldSpaceCameraPos - i.worldPos);
                
                float3 F0 = float3(0.04, 0.04, 0.04); 
                F0 = lerp(F0, _Albedo.rgb, _Metallic);

                //reflectance equation
                for(int l = 0; l < _PointLightCount; l++)
                {
                    //per light radiance
                    int index = l * 6;
                    float3 lightPos = float3(_PointLightData[index], _PointLightData[index + 1], _PointLightData[index + 2]);
                    float3 lightColor = float3(_PointLightData[l * 6 + 3], _PointLightData[l * 6 + 4], _PointLightData[l * 6 + 5]);
                    float3 lightVec = lightPos - i.worldPos;
                    float lightDist = length(lightVec);
                    lightVec = lightVec / lightDist;
                    float3 halfVec = normalize(view + lightVec);
                    float attenuation = 1.0 / (lightDist * lightDist);
                    float3 radiance = lightColor * attenuation;
                    //simple test
                    //col.rgb += _Albedo.rgb * radiance * saturate(dot(normalize(i.worldNormal), normalize(lightVec)));
                    //col.rgb += 0.25;

                    //cook-torrence brdf
                    float ndf = distributionGGX(normal, halfVec, _Roughness);
                    float geometryTerm = geometrySmith(normal, view, lightVec, _Roughness);
                    float3 fresnel = fresnelSchlick(max(dot(halfVec, view), 0.0), F0);

                    float3 specularRatio = fresnel;
                    float3 diffuseRatio = 1.0 - specularRatio;
                    diffuseRatio *= 1.0 - _Metallic; //prevent metallic from having diffuse component

                    float3 numerator = ndf * geometryTerm * fresnel;
                    float denominator = 4 * max(dot(normal, view), 0.0) * max(dot(normal, lightVec), 0.0) + 0.001;
                    float3 specular = numerator / denominator;
                    
                    //Add up outgoing radiance
                    float nDotL = saturate(dot(normal, lightVec));
                    lightOut += (diffuseRatio * _Albedo.rgb / UNITY_PI + specular) * radiance * nDotL;
                }
                
                float ao = 1;

                float nDotV = saturate(dot(normal, view));
                float fresnelFactor = fresnelSchlickRoughness(nDotV, F0, _Roughness);
                
                float3 indirectSpecularRatio = fresnelFactor;
                float3 indirectDiffuseRatio = 1.0 - indirectSpecularRatio;
                indirectDiffuseRatio *= 1.0 - _Metallic;
                
                float3 irradiance = texCUBE(_IndirectDiffuseMap, normal).rgb;
                //return fixed4(irradiance, 1);
                float irradianceLuminance = irradiance.r * 0.3 + irradiance.g * 0.6 + irradiance.b * 0.1;
                //return step(0.8, irradianceLuminance);
                irradiance = pow(irradiance, 2.2);
                float3 diffuse = irradiance * _Albedo.rgb;// / UNITY_PI;

                float3 reflection = reflect(-view, normal);
                const float MAX_REFLECTION_LOD = 8.0;
                float3 prefilteredColor = texCUBElod(_IndirectSpecularMap, float4(reflection, _Roughness * MAX_REFLECTION_LOD)).rgb;
                //prefilteredColor = pow(prefilteredColor, 2.2);
                float prefilteredIlluminance = prefilteredColor.r * 0.3 + prefilteredColor.g * 0.6 + prefilteredColor.b * 0.1;
                //return step(1, prefilteredIlluminance);
                
                float2 envBrdf = tex2D(_BrdfLut, float2(nDotV, _Roughness)).rg;
                //return float4(envBrdf, 0, 1);
                //envBrdf = pow(envBrdf, 2.2);
                float3 specular = prefilteredColor * (fresnelFactor * envBrdf.x + envBrdf.y);
                //return float4(specular, 1);
                
                float3 ambient = (indirectDiffuseRatio * diffuse + specular) * ao;
                //float3 ambient = (indirectDiffuseRatio * diffuse) * ao;

                //return pow(fixed4(lightOut, 1), 1 / 2.2);
                //return fixed4(lightOut, 1);
                col.rgb = ambient + lightOut;
                //col.rgb = lightOut;

                col.rgb = pow(col.rgb, 1.0 / 2.2);
                //col.rgb = pow(col.rgb, 2.2);

                return col;
            }
            ENDCG
        }
    }
}
