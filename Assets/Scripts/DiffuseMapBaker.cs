using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class DiffuseMapBaker : MonoBehaviour
{
    public string filename;

    public GameObject proxyGeo;

    public Shader diffuseConvolution;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Test Render")]
    public void TestRender()
    {
        Camera cam = GetComponent<Camera>();

        RenderTexture cubeRT = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBHalf);
        cubeRT.dimension = TextureDimension.Cube;
        Shader.SetGlobalTexture("_UnfilteredEnvironment", cubeRT);

        cam.RenderToCubemap(cubeRT);
        
        proxyGeo.SetActive(true);
        cam.SetReplacementShader(diffuseConvolution, "RenderType");
        
        Cubemap indirectDiffuseMap = new Cubemap(256, TextureFormat.RGBAHalf, false);
        cam.RenderToCubemap(indirectDiffuseMap);
        
        cam.ResetReplacementShader();
        proxyGeo.SetActive(false);
        /*
        Cubemap cubemap = new Cubemap(256, TextureFormat.RGBAHalf, false);
        cam.RenderToCubemap(cubemap);
        
        AssetDatabase.CreateAsset(cubemap, $"Assets/Textures/{filename}.asset");
        */
        
        AssetDatabase.CreateAsset(indirectDiffuseMap, $"Assets/Textures/{filename}_diffuse.asset");
    }
}
