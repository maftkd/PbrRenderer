using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LightingManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static int MAX_LIGHTS = 16;
    void Start()
    {
        PointLight[] pointLights = GetComponentsInChildren<PointLight>();
        float[] pointLightData = new float[MAX_LIGHTS * 6];
        for (int i = 0; i < pointLights.Length && i < MAX_LIGHTS; i++)
        {
            pointLightData[i * 6] = pointLights[i].transform.position.x;
            pointLightData[i * 6 + 1] = pointLights[i].transform.position.y;
            pointLightData[i * 6 + 2] = pointLights[i].transform.position.z;
            pointLightData[i * 6 + 3] = pointLights[i].color.r;
            pointLightData[i * 6 + 4] = pointLights[i].color.g;
            pointLightData[i * 6 + 5] = pointLights[i].color.b;
        }
        for(int i = pointLights.Length; i < MAX_LIGHTS; i++)
        {
            pointLightData[i * 6] = 0;
            pointLightData[i * 6 + 1] = 0;
            pointLightData[i * 6 + 2] = 0;
            pointLightData[i * 6 + 3] = 0;
            pointLightData[i * 6 + 4] = 0;
            pointLightData[i * 6 + 5] = 0;
        }
        
        Shader.SetGlobalFloatArray("_PointLightData", pointLightData);
        Shader.SetGlobalFloat("_PointLightCount", pointLights.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
