using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class EnvironmentMapBaker : MonoBehaviour
{
    public string filename;

    public GameObject proxyGeo;

    public Shader diffuseConvolution;
    public Shader mipTestShader;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Bake Diffuse")]
    public void BakeDiffuse()
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
    
    [ContextMenu("Test Mips")]
    public void TestMips()
    {
        Camera cam = GetComponent<Camera>();
        int mapSize = 256;

        Cubemap cubemap = new Cubemap(mapSize, TextureFormat.RGBAHalf, true);
        cubemap.filterMode = FilterMode.Trilinear;
        int numMips = cubemap.mipmapCount;
        
        proxyGeo.SetActive(true);
        cam.SetReplacementShader(mipTestShader, "RenderType");
        Color testCol;
        for(int i = 0; i < numMips; i++)
        {
            float hue = (float)i / (numMips - 1);
            testCol = Color.HSVToRGB(hue, 1, 1);
            Shader.SetGlobalColor("_MipTestColor", testCol);
            Cubemap mipCubemap = new Cubemap(mapSize >> i, TextureFormat.RGBAHalf, false);
            cam.RenderToCubemap(mipCubemap);

            for (int face = 0; face < 6; face++)
            {
                Graphics.CopyTexture(mipCubemap, face, 0, cubemap, face, i);
            }
        }
        
        cam.ResetReplacementShader();
        proxyGeo.SetActive(false);
        
        AssetDatabase.CreateAsset(cubemap, $"Assets/Textures/{filename}_mipTest.asset");
    }
}
