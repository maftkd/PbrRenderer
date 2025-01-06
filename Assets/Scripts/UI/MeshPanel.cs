using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshPanel : MonoBehaviour
{
    [System.Serializable]
    public class MeshTexturePair
    {
        public GameObject mesh;
        public Texture2D texture;
    }
    
    public List<MeshTexturePair> meshTextures = new();
    
    private List<Button> _buttons = new();
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject meshButtonPrefab = transform.GetChild(0).gameObject;
        
        foreach(MeshTexturePair pair in meshTextures)
        {
            GameObject meshButton = Instantiate(meshButtonPrefab, transform);
            meshButton.SetActive(true);
            Button butt = meshButton.GetComponent<Button>();
            butt.onClick.AddListener(delegate { ChangeMesh(pair.mesh); });
            RawImage img = meshButton.GetComponent<RawImage>();
            img.texture = pair.texture;
            _buttons.Add(butt);
        }
        
        ChangeMesh(meshTextures[0].mesh);
    }

    void ChangeMesh(GameObject go)
    {
        foreach(MeshTexturePair pair in meshTextures)
        {
            if (pair.mesh == go)
            {
                pair.mesh.SetActive(true);
            }
            else
            {
                pair.mesh.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
