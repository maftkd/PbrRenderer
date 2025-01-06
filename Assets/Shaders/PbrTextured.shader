Shader "Unlit/PbrTextured"
{
    Properties
    {
        _Albedo ("Albedo", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        [Toggle] _MetallicInAlpha ("Metallic in Alpha", Range(0,1)) = 0
        _MetallicMap ("Metallic Map", 2D) = "black" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _AmbientOcclusionMap("Ambient Occlusion Map", 2D) = "white" {}
        _TextureST("Texture ST", Vector) = (1, 1, 0, 0)
        [Toggle] _FlipUVs ("Flip UVs", Range(0,1)) = 0
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


            fixed _FlipUVs;
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
                if(_FlipUVs)
                {
                    o.uv.y = 1 - o.uv.y;
                }
                return o;
            }

            sampler2D _BumpMap;
            sampler2D _Albedo;
            sampler2D _MetallicMap;
            sampler2D _RoughnessMap;
            fixed _MetallicInAlpha;
            fixed4 _TextureST;
            
            sampler2D _AmbientOcclusionMap;
            samplerCUBE _UnfilteredEnvironment;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = 0;
                float3 lightOut = 0;
                
                //float3 normal = normalize(i.worldNormal);
                //return fixed4(i.uv, 0, 1);
                half3 normal;
                half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.uv * _TextureST.xy));
                // transform normal from tangent to world space
                normal.x = dot(i.tspace0, tnormal);
                normal.y = dot(i.tspace1, tnormal);
                normal.z = dot(i.tspace2, tnormal);
                //return fixed4(normal, 1);

                float3 albedo = tex2D(_Albedo, i.uv * _TextureST.xy).rgb;
                albedo = pow(albedo, 2.2);

                float metallic = 0;
                if(_MetallicInAlpha)
                {
                    metallic = tex2D(_MetallicMap, i.uv * _TextureST.xy).a;
                }
                else
                {
                    metallic = tex2D(_MetallicMap, i.uv * _TextureST.xy).r;
                }
                float roughness = tex2D(_RoughnessMap, i.uv * _TextureST.xy).r;
                //return float4(metallic, roughness, 0, 1);
                
                float3 view = normalize(_WorldSpaceCameraPos - i.worldPos);
                
                float3 F0 = float3(0.04, 0.04, 0.04); 
                F0 = lerp(F0, albedo, metallic);

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

                    //cook-torrence brdf
                    float ndf = distributionGGX(normal, halfVec, roughness);
                    float geometryTerm = geometrySmith(normal, view, lightVec, roughness);
                    float3 fresnel = fresnelSchlick(max(dot(halfVec, view), 0.0), F0);

                    float3 specularRatio = fresnel;
                    float3 diffuseRatio = 1.0 - specularRatio;
                    diffuseRatio *= 1.0 - metallic; //prevent metallic from having diffuse component

                    float3 numerator = ndf * geometryTerm * fresnel;
                    float denominator = 4 * max(dot(normal, view), 0.0) * max(dot(normal, lightVec), 0.0) + 0.001;
                    float3 specular = numerator / denominator;
                    
                    //Add up outgoing radiance
                    float nDotL = saturate(dot(normal, lightVec));
                    lightOut += (diffuseRatio * albedo / UNITY_PI + specular) * radiance * nDotL;
                }
                
                float ao = tex2D(_AmbientOcclusionMap, i.uv * _TextureST.xy).r;

                float nDotV = saturate(dot(normal, view));
                float3 fresnelFactor = fresnelSchlickRoughness(nDotV, F0, roughness);
                
                float3 indirectSpecularRatio = fresnelFactor;
                float3 indirectDiffuseRatio = 1.0 - indirectSpecularRatio;
                indirectDiffuseRatio *= 1.0 - metallic;
                
                float4 irradiance = texCUBElod(_IndirectDiffuseMap, float4(normal, 0));
                //return irradiance;
                //irradiance.rgb = DecodeHDR(irradiance, float4(5,1,0,1));
                //return irradiance;
                float3 diffuse = irradiance.rgb * albedo;

                float3 reflection = reflect(-view, normal);
                float4 prefilteredColor = texCUBElod(_IndirectSpecularMap, float4(reflection, roughness * MAX_REFLECTION_LOD));
                //prefilteredColor.rgb = DecodeHDR(prefilteredColor, float4(5,1,0,1));
                //float4 debug = texCUBElod(_IndirectSpecularMap, float4(reflection, 0));
                //float4 debug = texCUBE(_IndirectSpecularMap, reflection);
                //float4 debug = texCUBElod(_MipCubemap, float4(reflection, 0));
                //return debug;
                float2 envBrdf = tex2D(_BrdfLut, float2(nDotV, roughness)).rg;
                float3 specular = prefilteredColor.rgb * (fresnelFactor * envBrdf.x + envBrdf.y);

                float3 ambient = (indirectDiffuseRatio * diffuse + specular) * ao;

                col.rgb = ambient + lightOut;

                return col;
            }
            ENDCG
        }
    }
}
