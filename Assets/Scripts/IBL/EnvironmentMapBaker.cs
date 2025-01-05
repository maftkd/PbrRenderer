using UnityEngine;

public class EnvironmentMapBaker : MonoBehaviour
{
    public GameObject proxyGeo;

    public Shader diffuseConvolution;
    public Shader specularConvolution;

    public int diffuseMapSize;
    public int specularMapSize;

    public Cubemap rawCubemap;

    void Start()
    {
        BakeDiffuse();
        BakeSpecular();
    }

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
        
        Shader.SetGlobalTexture("_IndirectSpecularMap", cubemap);
    }
    
}
