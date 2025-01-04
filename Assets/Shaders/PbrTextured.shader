Shader "Unlit/PbrTextured"
{
    Properties
    {
        _Albedo ("Albedo", Color) = (1,1,1,1)
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.5
        _BumpMap("Normal Map", 2D) = "bump" {}
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
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                half3 tspace0 : TEXCOORD2; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD3; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD4; // tangent.z, bitangent.z, normal.z
                float2 uv : TEXCOORD5;
            };

            float4 _Albedo;
            float _Roughness;
            float _Metallic;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(worldNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, worldNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, worldNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, worldNormal.z);
                
                o.uv = v.uv;
                return o;
            }

            sampler2D _BumpMap;
            fixed4 _BumpMap_ST;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = 0;
                float3 lightOut = 0;
                
                //float3 normal = normalize(i.worldNormal);
                //return fixed4(i.uv, 0, 1);
                half3 normal;
                half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.uv * _BumpMap_ST.xy));
                // transform normal from tangent to world space
                normal.x = dot(i.tspace0, tnormal);
                normal.y = dot(i.tspace1, tnormal);
                normal.z = dot(i.tspace2, tnormal);
                //return fixed4(normal, 1);
                
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
                float3 fresnelFactor = fresnelSchlickRoughness(nDotV, F0, _Roughness);
                
                float3 indirectSpecularRatio = fresnelFactor;
                float3 indirectDiffuseRatio = 1.0 - indirectSpecularRatio;
                indirectDiffuseRatio *= 1.0 - _Metallic;
                
                float3 irradiance = texCUBE(_IndirectDiffuseMap, normal).rgb;
                float3 diffuse = irradiance * _Albedo.rgb;// / UNITY_PI;

                float3 reflection = reflect(-view, normal);
                const float MAX_REFLECTION_LOD = 7.0;
                float3 prefilteredColor = texCUBElod(_IndirectSpecularMap, float4(reflection, _Roughness * MAX_REFLECTION_LOD)).rgb;
                float2 envBrdf = tex2D(_BrdfLut, float2(nDotV, _Roughness)).rg;
                float3 specular = prefilteredColor * (fresnelFactor * envBrdf.x + envBrdf.y);
                
                float3 ambient = (indirectDiffuseRatio * diffuse + specular) * ao;
                
                col.rgb = ambient + lightOut;

                return col;
            }
            ENDCG
        }
    }
}
