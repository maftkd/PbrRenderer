using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DiffuseMapBaker : MonoBehaviour
{
    public string filename;
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
        
        Cubemap cubemap = new Cubemap(256, TextureFormat.RGBAHalf, false);
        
        cam.RenderToCubemap(cubemap);
        
        AssetDatabase.CreateAsset(cubemap, $"Assets/Textures/{filename}.asset");
    }
}
