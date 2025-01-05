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

    private GameObject _envButtonPrefab;

    private List<Button> _buttons = new();
    // Start is called before the first frame update
    void Start()
    {
        //tmp set initial environment
        
        //setup gui
        _envButtonPrefab = transform.GetChild(0).gameObject;

        for (int i = 0; i < environments.Length; i++)
        {
            GameObject envButton = Instantiate(_envButtonPrefab, transform);
            envButton.SetActive(true);
            Button butt = envButton.GetComponent<Button>();
            butt.onClick.AddListener(delegate { SendBakeRequest(butt); });
            _buttons.Add(butt);
        }
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
