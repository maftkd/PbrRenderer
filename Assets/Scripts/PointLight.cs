using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointLight : MonoBehaviour
{
    [ColorUsage(false, true)]
    public Color color;

    private LightingManager _lightingManager;

    public Renderer billboard;
    // Start is called before the first frame update
    void Start()
    {
        _lightingManager = transform.parent.GetComponent<LightingManager>();
        UpdateLightingDataAndFindPointLights();
        SetColor(color);
    }

    private void OnDestroy()
    {
        UpdateLightingDataAndFindPointLights();
    }

    public void UpdateLightingDataAndFindPointLights()
    {
        _lightingManager.UpdatePointLightData(true);
    }
    
    public void UpdateLightingData()
    {
        _lightingManager.UpdatePointLightData();
    }

    public void ShowLightingPanel()
    {
        if (_lightingManager == null)
        {
            _lightingManager = transform.parent.GetComponent<LightingManager>();
        }
        _lightingManager.ShowLightingPanel(this);
    }

    public void HideLightingPanel()
    {
        _lightingManager.HideLightingPanel();
    }

    public void SetColor(Color c)
    {
        color = c;
        billboard.material.SetColor("_Tint", c);
        UpdateLightingData();
    }
}
