Shader "Unlit/SimpleSkybox"
{
    Properties
    {
        _Cubemap ("Cubemap", CUBE) = "" {}
        
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 eyeRay : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.eyeRay = normalize(v.vertex.xyz);
                return o;
            }

            samplerCUBE_half _Cubemap;
            half4 _Cubemap_HDR;

            fixed4 frag (v2f i) : SV_Target
            {
                float3 eyeRay = normalize(i.eyeRay);

                //sample hdr map
                fixed4 envColor = texCUBE(_Cubemap, eyeRay);
                envColor.rgb = DecodeHDR (envColor, _Cubemap_HDR);
                
                //encode to gamma since we gamma correct everything in post
                envColor = pow(envColor, 2.2);
                return envColor;
            }
            ENDCG
        }
    }
}
