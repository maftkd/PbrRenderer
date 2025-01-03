using UnityEngine;

public class LightingManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PointLight[] pointLights = GetComponentsInChildren<PointLight>();
        float[] pointLightData = new float[pointLights.Length * 6];
        for (int i = 0; i < pointLights.Length; i++)
        {
            pointLightData[i * 6] = pointLights[i].transform.position.x;
            pointLightData[i * 6 + 1] = pointLights[i].transform.position.y;
            pointLightData[i * 6 + 2] = pointLights[i].transform.position.z;
            pointLightData[i * 6 + 3] = pointLights[i].color.r;
            pointLightData[i * 6 + 4] = pointLights[i].color.g;
            pointLightData[i * 6 + 5] = pointLights[i].color.b;
        }
        Shader.SetGlobalFloatArray("_PointLightData", pointLightData);
        Shader.SetGlobalFloat("_PointLightCount", pointLights.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
