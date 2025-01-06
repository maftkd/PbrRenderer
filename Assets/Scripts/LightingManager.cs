using UnityEngine;

public class LightingManager : MonoBehaviour
{
    public Texture2D brdfLUT;
    private PointLight[] _pointLights;
    
    // Start is called before the first frame update
    private static int MAX_LIGHTS = 16;
    void Start()
    {
        UpdatePointLightData();
        Shader.SetGlobalTexture("_BrdfLut", brdfLUT);
    }

    public void UpdatePointLightData()
    {
        if (_pointLights == null)
        {
            _pointLights = GetComponentsInChildren<PointLight>();
        }
        float[] pointLightData = new float[MAX_LIGHTS * 6];
        for (int i = 0; i < _pointLights.Length && i < MAX_LIGHTS; i++)
        {
            pointLightData[i * 6] = _pointLights[i].transform.position.x;
            pointLightData[i * 6 + 1] = _pointLights[i].transform.position.y;
            pointLightData[i * 6 + 2] = _pointLights[i].transform.position.z;
            pointLightData[i * 6 + 3] = _pointLights[i].color.r;
            pointLightData[i * 6 + 4] = _pointLights[i].color.g;
            pointLightData[i * 6 + 5] = _pointLights[i].color.b;
        }
        for(int i = _pointLights.Length; i < MAX_LIGHTS; i++)
        {
            pointLightData[i * 6] = 0;
            pointLightData[i * 6 + 1] = 0;
            pointLightData[i * 6 + 2] = 0;
            pointLightData[i * 6 + 3] = 0;
            pointLightData[i * 6 + 4] = 0;
            pointLightData[i * 6 + 5] = 0;
        }
        
        Shader.SetGlobalFloatArray("_PointLightData", pointLightData);
        Shader.SetGlobalFloat("_PointLightCount", _pointLights.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
