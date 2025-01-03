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

            fixed4 _Albedo;
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
                    float3 lightPos = float3(_PointLightData[l * 6], _PointLightData[l * 6 + 1], _PointLightData[l * 6 + 2]);
                    float3 lightColor = float3(_PointLightData[l * 6 + 3], _PointLightData[l * 6 + 4], _PointLightData[l * 6 + 5]);
                    float3 lightVec = lightPos - i.worldPos;
                    float lightDist = length(lightVec);
                    lightVec = lightVec / lightDist;
                    float3 halfVec = normalize(view + lightVec);
                    float attenuation = 1.0 / (lightDist * lightDist);
                    float3 radiance = lightColor * attenuation;
                    //simple test
                    //col.rgb += _Color.rgb * radiance * saturate(dot(normalize(i.worldNormal), normalize(lightVec)));

                    //cook-torrence brdf
                    float ndf = DistributionGGX(normal, halfVec, _Roughness);
                    float geometryTerm = GeometrySmith(normal, view, lightVec, _Roughness);
                    float3 fresnel = fresnelSchlick(max(dot(halfVec, view), 0.0), F0);

                    float3 specularRatio = fresnel;
                    float3 diffuseRatio = 1.0 - specularRatio;
                    diffuseRatio *= 1.0 - _Metallic; //prevent metallic from having diffuse component

                    float3 numerator = ndf * geometryTerm * fresnel;
                    float denominator = 4 * max(dot(normal, view), 0.0) * max(dot(normal, lightVec), 0.0) + 0.001;
                    float3 specular = numerator / denominator;
                    
                    //Add up outgoing radiance
                    float nDotL = saturate(dot(normal, lightVec));
                    lightOut += (diffuseRatio * _Albedo / UNITY_PI + specular) * radiance * nDotL;
                }

                float ao = 0.05;
                float3 ambient = float3(0.03, 0.03, 0.03) * _Albedo * ao;
                col.rgb = ambient + lightOut;
                
                return col;
            }
            ENDCG
        }
    }
}
