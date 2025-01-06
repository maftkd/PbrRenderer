using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointLight : MonoBehaviour
{
    [ColorUsage(false, true)]
    public Color color;

    private LightingManager _lightingManager;
    // Start is called before the first frame update
    void Start()
    {
        _lightingManager = transform.parent.GetComponent<LightingManager>();
        UpdateLightingDataAndFindPointLights();
        
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
}
