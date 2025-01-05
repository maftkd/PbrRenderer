using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentPanel : MonoBehaviour
{
    public Cubemap[] environments;
    public EnvironmentMapBaker mapBaker;

    public Material skyboxMaterial;

    public Shader iconGenShader;

    private List<Button> _buttons = new();
    // Start is called before the first frame update
    void Start()
    {
        //tmp set initial environment
        
        //setup gui
        GameObject envButtonPrefab = transform.GetChild(0).gameObject;

        RenderTexture active = RenderTexture.active;
        RenderTexture iconRT = RenderTexture.GetTemporary(128, 128, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = iconRT;
        Material mat = new Material(iconGenShader);
        for (int i = 0; i < environments.Length; i++)
        {
            GameObject envButton = Instantiate(envButtonPrefab, transform);
            envButton.SetActive(true);
            Button butt = envButton.GetComponent<Button>();
            butt.onClick.AddListener(delegate { SendBakeRequest(butt); });
            RawImage img = envButton.GetComponent<RawImage>();
            Shader.SetGlobalTexture("_UnfilteredEnvironment", environments[i]);
            Graphics.Blit(null, iconRT, mat);
            Texture2D icon = new Texture2D(128, 128);
            icon.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            icon.Apply();
            img.texture = icon;
            _buttons.Add(butt);
        }
        RenderTexture.active = active;
        iconRT.Release();
    }

    void SendBakeRequest(Button button)
    {
        int envIndex = _buttons.IndexOf(button);
        Cubemap cubemap = environments[envIndex];
        mapBaker.BakeMap(cubemap);
        skyboxMaterial.SetTexture("_Cubemap", cubemap);
        foreach (Button butt in _buttons)
        {
            butt.interactable = false;
        }

        mapBaker.DoneBaking += OnBakeComplete;
    }

    void OnBakeComplete()
    {
        foreach (Button butt in _buttons)
        {
            butt.interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
