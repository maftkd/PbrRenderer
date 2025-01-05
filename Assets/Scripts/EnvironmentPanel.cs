using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentPanel : MonoBehaviour
{
    public Cubemap[] environments;
    public EnvironmentMapBaker mapBaker;

    public Material skyboxMaterial;
    // Start is called before the first frame update
    void Start()
    {
        Cubemap cur = environments[0];
        mapBaker.BakeMap(cur);
        
        skyboxMaterial.SetTexture("_Cubemap", cur);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
