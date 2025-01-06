using UnityEngine;

public class LightingManager : MonoBehaviour
{
    public Texture2D brdfLUT;
    private PointLight[] _pointLights;
    
    // Start is called before the first frame update
    private static int MAX_LIGHTS = 16;

    public LightingPanel lightingPanel;
    
    void Start()
    {
        Shader.SetGlobalTexture("_BrdfLut", brdfLUT);
    }

    public void UpdatePointLightData(bool updateList = false)
    {
        if (_pointLights == null || updateList)
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

    public void ShowLightingPanel(PointLight pointLight)
    {
        lightingPanel.ShowPanel(pointLight);
    }

    public void HideLightingPanel()
    {
        if (lightingPanel != null)
        {
            lightingPanel.HidePanel();
        }
    }
}
