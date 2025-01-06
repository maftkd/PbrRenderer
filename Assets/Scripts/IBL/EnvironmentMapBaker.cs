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
    public Shader blitShader;

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
            StartCoroutine(BakeMaps());
        }
    }

    private IEnumerator BakeMaps()
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
        
        //bake specular map
        int mapSize = specularMapSize;
        RenderTexture indirectSpecularMap = new RenderTexture(mapSize, mapSize, 0, RenderTextureFormat.ARGBHalf);
        indirectSpecularMap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        indirectSpecularMap.useMipMap = true;
        indirectSpecularMap.autoGenerateMips = false;
        int numMips = indirectSpecularMap.mipmapCount;
        Debug.Log("Num mips: " + numMips);
        
        proxyGeo.SetActive(true);
        cam.SetReplacementShader(specularConvolution, "RenderType");
        Material mat = new Material(blitShader);
        for (int i = 0; i < numMips; i++)
        {
            Shader.SetGlobalFloat("_BakeRoughness", (float)i / (numMips - 1));
            
            if(SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.Basic)
            {
                RenderTexture mipCubemap = new RenderTexture(mapSize >> i, mapSize >> i, 0, RenderTextureFormat.ARGBHalf);
                mipCubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
                cam.RenderToCubemap(mipCubemap);
                
                for(int face = 0; face < 6; face++)
                {
                    Graphics.CopyTexture(mipCubemap, face, 0, indirectSpecularMap, face, i);
                    step++;
                    UpdateProgressText(step);
                    yield return null;
                }
            }
            else
            {
                RenderTexture mipTexture = new RenderTexture(mapSize >> i, mapSize >> i, 8, RenderTextureFormat.ARGBHalf);
                
                for(int face = 0; face < 6; face++)
                {
                    //render to a single face of the cubemap at a time
                    cam.targetTexture = mipTexture;
                    cam.transform.rotation = GetRotationForFace((CubemapFace)face);
                    cam.Render();
                    
                    //set source of blit
                    mat.SetTexture("_MainTex", mipTexture);
                    
                    //set target to specific face & mip of indirect specular map
                    Graphics.SetRenderTarget(indirectSpecularMap, i, (CubemapFace)face);
                    
                    //textures need to be flipped certain ways before being rendered onto the cubemap
                    bool flipX = face == 2 || face == 3;
                    bool flipY = !flipX;
                    mat.SetFloat("_FlipX", flipX ? 1 : 0);
                    mat.SetFloat("_FlipY", flipY ? 1 : 0);
                    
                    //render full screen quad given above target texture and source texture set in material
                    GL.PushMatrix();
                    GL.LoadOrtho();
                    mat.SetPass(0);
                    GL.Begin(GL.QUADS);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(0, 0, 0);
                    GL.TexCoord2(0, 1);
                    GL.Vertex3(0, 1, 0);
                    GL.TexCoord2(1, 1);
                    GL.Vertex3(1, 1, 0);
                    GL.TexCoord2(1, 0);
                    GL.Vertex3(1, 0, 0);
                    GL.End();
                    GL.PopMatrix();

                    step++;
                    UpdateProgressText(step);
                    yield return null;
                }
            }
        }
        cam.ResetReplacementShader();
        proxyGeo.SetActive(false);
        
        Shader.SetGlobalTexture("_IndirectSpecularMap", indirectSpecularMap);

        _baking = false;
        loadingPanel.SetActive(false);
        DoneBaking?.Invoke();
    }

    private Quaternion GetRotationForFace(CubemapFace face)
    {
        switch (face)
        {
            case CubemapFace.NegativeX:
                return Quaternion.LookRotation(Vector3.left, Vector3.up);
            case CubemapFace.PositiveY:
                return Quaternion.LookRotation(Vector3.up, Vector3.forward);
            case CubemapFace.NegativeY:
                return Quaternion.LookRotation(Vector3.down, Vector3.back);
            case CubemapFace.PositiveZ:
                return Quaternion.LookRotation(Vector3.forward, Vector3.up);
            case CubemapFace.NegativeZ:
                return Quaternion.LookRotation(Vector3.back, Vector3.up);
            case CubemapFace.PositiveX:
            default:
                return Quaternion.LookRotation(Vector3.right, Vector3.up);
        }
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
        Debug.Log($"Step {curStep} / 61");
        float percentage = (float)curStep / 61;
        percentage *= 100;
        progressText.text = "Processing Environment: " + percentage.ToString("0")+"%";
    }
    
}
