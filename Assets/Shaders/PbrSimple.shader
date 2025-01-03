Shader "Unlit/PbrSimple"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
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

            fixed4 _Color;
            float _Roughness;
            float _Metallic;

            //these should be moved to a cginc file
            float _PointLightData[96]; // 6 * 16
            float _PointLightCount;

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
                for(int l = 0; l < _PointLightCount; l++)
                {
                    float3 lightPos = float3(_PointLightData[l * 6], _PointLightData[l * 6 + 1], _PointLightData[l * 6 + 2]);
                    float3 lightColor = float3(_PointLightData[l * 6 + 3], _PointLightData[l * 6 + 4], _PointLightData[l * 6 + 5]);
                    float3 lightVec = lightPos - i.worldPos;

                    col.rgb += _Color.rgb * lightColor * saturate(dot(normalize(i.worldNormal), normalize(lightVec)));
                }
                
                return col;
            }
            ENDCG
        }
    }
}
