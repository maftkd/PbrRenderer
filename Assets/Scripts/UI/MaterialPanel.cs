using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialPanel : MonoBehaviour
{
    public Material[] materials;
    
    private List<Button> _buttons = new();
    
    public Renderer [] targetRenderers;
    // Start is called before the first frame update
    void Start()
    {
        
        GameObject matButtonPrefab = transform.GetChild(0).gameObject;
        
        for (int i = 0; i < materials.Length; i++)
        {
            GameObject matButton = Instantiate(matButtonPrefab, transform);
            matButton.SetActive(true);
            Button butt = matButton.GetComponent<Button>();
            butt.onClick.AddListener(delegate { ChangeMaterial(butt); });
            RawImage img = matButton.GetComponent<RawImage>();
            img.texture = materials[i].GetTexture("_Albedo");
            _buttons.Add(butt);
        }
    }

    void ChangeMaterial(Button b)
    {
        Material m = materials[_buttons.IndexOf(b)];
        foreach (Renderer r in targetRenderers)
        {
            r.material = m;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
