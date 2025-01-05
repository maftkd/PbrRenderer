#if UNITY_EDITOR || UNITY_EDITOR_OSX
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class EnvironmentMapBaker : MonoBehaviour
{
    public string filename;

    public GameObject proxyGeo;

    public Shader diffuseConvolution;
    public Shader specularConvolution;
    //public Shader baseCubemapSampler;

    public int baseMapSize;
    public int diffuseMapSize;
    public int specularMapSize;

    public Cubemap rawCubemap;

    void RenderBaseCubemap()
    {
        Camera cam = GetComponent<Camera>();
        //this size is fixed as this method is only used to renderer temporary cubemap
        RenderTexture cubeRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf);
        cubeRT.dimension = TextureDimension.Cube;
        Shader.SetGlobalTexture("_UnfilteredEnvironment", cubeRT);
        cam.RenderToCubemap(cubeRT);
    }

    [ContextMenu("Bake Diffuse")]
    public void BakeDiffuse()
    {
        Camera cam = GetComponent<Camera>();

        //RenderBaseCubemap();
        Shader.SetGlobalTexture("_UnfilteredEnvironment", rawCubemap);
        
        proxyGeo.SetActive(true);
        cam.SetReplacementShader(diffuseConvolution, "RenderType");
        
        Cubemap indirectDiffuseMap = new Cubemap(diffuseMapSize, TextureFormat.RGBAHalf, false);
        cam.RenderToCubemap(indirectDiffuseMap);
        
        cam.ResetReplacementShader();
        proxyGeo.SetActive(false);
        
        AssetDatabase.CreateAsset(indirectDiffuseMap, $"Assets/Textures/{filename}_diffuse.asset");
    }
    
    [ContextMenu("Bake Specular")]
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
        
        AssetDatabase.CreateAsset(cubemap, $"Assets/Textures/{filename}_specular.asset");
    }
    
}
#endif
