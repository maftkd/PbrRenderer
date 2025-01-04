#if UNITY_EDITOR || UNITY_EDITOR_OSX
using UnityEditor;
using UnityEngine;

public class BrdfResponseBaker : MonoBehaviour
{
    public Shader brdfResponseBakerShader;
    [ContextMenu("Bake Brdf Response")]
    public void BakeBrdfResponse()
    {
        RenderTexture brdfLUT = new RenderTexture(512, 512, 0, RenderTextureFormat.RGHalf);
        Graphics.Blit(null, brdfLUT, new Material(brdfResponseBakerShader));
        
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = brdfLUT;
        
        Texture2D brdfLUTTexture = new Texture2D(512, 512, TextureFormat.RGHalf, false);
        brdfLUTTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        brdfLUTTexture.Apply();
        brdfLUTTexture.wrapMode = TextureWrapMode.Clamp;

        RenderTexture.active = active;
        
        AssetDatabase.CreateAsset(brdfLUTTexture, "Assets/Textures/brdfLUT.asset");
    }
}
#endif