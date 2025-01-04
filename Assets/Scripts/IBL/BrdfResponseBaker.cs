using System.Collections;
using System.Collections.Generic;
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

        RenderTexture.active = active;
        
        AssetDatabase.CreateAsset(brdfLUTTexture, "Assets/Textures/brdfLUT.asset");
    }
}
