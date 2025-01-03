using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToneMapper : MonoBehaviour, IPostProcessLayer
{
    public Shader shader;
    private Material material;
    [Range(0,5)]
    public float exposure;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null)
        {
            material = new Material(shader);
        }

        material.SetFloat("_Exposure", exposure);
        
        Graphics.Blit(source, destination, material);
    }

}
