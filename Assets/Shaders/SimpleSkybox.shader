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

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 envColor = texCUBE(_Cubemap, i.eyeRay).rgb;
                //float luminance = dot(envColor, float3(0.299, 0.587, 0.114));
                //return step(0.99, luminance);
                envColor = pow(envColor, 2.2);
                return fixed4(envColor, 1.0);
            }
            ENDCG
        }
    }
}
