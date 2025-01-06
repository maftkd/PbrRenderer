using System;
using System.Collections;
using TMPro;
#if UNITY_EDITOR || UNITY_EDITOR_OSX
using UnityEditor;
#endif
using UnityEngine;

public class EnvironmentMapBaker : MonoBehaviour
{
    public GameObject proxyGeo;

    public Shader diffuseConvolution;
    public Shader specularConvolution;

    public int diffuseMapSize;
    public int specularMapSize;

    public Cubemap rawCubemap;
    public Cubemap[] rawCubemaps;

    private bool _baking = false;
    public TextMeshProUGUI progressText;
    public GameObject loadingPanel;
    public Action DoneBaking;

    public int debugFace;

    void Start()
    {
        //BakeDiffuse();
        //BakeSpecular();
    }

    public void BakeMap(Cubemap cubemap)
    {
        if (!_baking)
        {
            rawCubemap = cubemap;
            StartCoroutine(BakeMap());
        }
    }

    private IEnumerator BakeMap()
    {
        loadingPanel.SetActive(true);
        _baking = true;
        int step = 0;
        UpdateProgressText(step);
        yield return null;
        
        Camera cam = GetComponent<Camera>();

        Shader.SetGlobalTexture("_UnfilteredEnvironment", rawCubemap);
        
        proxyGeo.SetActive(true);
        cam.SetReplacementShader(diffuseConvolution, "RenderType");
        
        RenderTexture indirectDiffuseMap = new RenderTexture(diffuseMapSize, diffuseMapSize, 0, RenderTextureFormat.ARGBHalf);
        indirectDiffuseMap.wrapMode = TextureWrapMode.Clamp;
        indirectDiffuseMap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        cam.RenderToCubemap(indirectDiffuseMap, debugFace > 0 ? debugFace : 63);
        
        cam.ResetReplacementShader();
        proxyGeo.SetActive(false);
        
        Shader.SetGlobalTexture("_IndirectDiffuseMap", indirectDiffuseMap);

        step++;
        UpdateProgressText(step);
        yield return null;

        _baking = false;
        loadingPanel.SetActive(false);
        DoneBaking?.Invoke();
    }

    /*
    public void BakeDiffuse()
    {
        Camera cam = GetComponent<Camera>();

        //RenderBaseCubemap();
        Shader.SetGlobalTexture("_UnfilteredEnvironment", rawCubemap);
        
        proxyGeo.SetActive(true);
        cam.SetReplacementShader(diffuseConvolution, "RenderType");
        
        //Cubemap indirectDiffuseMap = new Cubemap(diffuseMapSize, TextureFormat.RGBAHalf, false);
        RenderTexture indirectDiffuseMap = new RenderTexture(diffuseMapSize, diffuseMapSize, 0, RenderTextureFormat.ARGBHalf);
        indirectDiffuseMap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        cam.RenderToCubemap(indirectDiffuseMap);
        
        cam.ResetReplacementShader();
        proxyGeo.SetActive(false);
        
        Shader.SetGlobalTexture("_IndirectDiffuseMap", indirectDiffuseMap);
    }
    */

    [ContextMenu("Bake all specular maps")]
    public void BakeAllSpecular()
    {
        foreach (Cubemap cubemap in rawCubemaps)
        {
            rawCubemap = cubemap;
            Debug.Log("Baking: " + cubemap.name);
            BakeSpecular();
        }
    }
    
    [ContextMenu("Bake Indirect Specular")]
    public void BakeSpecular()
    {
        Camera cam = GetComponent<Camera>();
        
        //RenderBaseCubemap();
        Shader.SetGlobalTexture("_UnfilteredEnvironment", rawCubemap);
        
        int mapSize = specularMapSize;

        Cubemap cubemap = new Cubemap(mapSize, TextureFormat.RGBAHalf, true);
        cubemap.filterMode = FilterMode.Trilinear;
        int numMips = cubemap.mipmapCount;
        
        proxyGeo.SetActive(true);
        cam.SetReplacementShader(specularConvolution, "RenderType");
        for(int i = 0; i < numMips; i++)
        {
            float roughness = (float)i / (numMips - 1);
            Shader.SetGlobalFloat("_BakeRoughness", roughness);
            Cubemap mipCubemap = new Cubemap(mapSize >> i, TextureFormat.RGBAHalf, false);
            cam.RenderToCubemap(mipCubemap);

            for (int face = 0; face < 6; face++)
            {
                Graphics.CopyTexture(mipCubemap, face, 0, cubemap, face, i);
            }
        }
        
        cam.ResetReplacementShader();
        proxyGeo.SetActive(false);
        
        //Shader.SetGlobalTexture("_IndirectSpecularMap", cubemap);
        
#if unity_editor || UNITY_EDITOR_OSX
        AssetDatabase.CreateAsset(cubemap, $"Assets/Textures/Specular_{rawCubemap.name}.asset");
#endif
    }

    void UpdateProgressText(int curStep)
    {
        Debug.Log($"Step {curStep} / 11");
        float percentage = (float)curStep / 11;
        percentage *= 100;
        progressText.text = "Processing Environment: " + percentage.ToString("0")+"%";
    }
    
}
