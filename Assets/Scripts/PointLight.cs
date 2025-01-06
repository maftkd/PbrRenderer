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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLightingData()
    {
        _lightingManager.UpdatePointLightData();
    }
}
