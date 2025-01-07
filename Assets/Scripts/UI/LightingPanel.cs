using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightingPanel : MonoBehaviour
{
    private PointLight _targetLight;
    public Slider hueSlider;
    public Slider saturationSlider;
    public Slider brightnessSlider;

    private Camera _mainCam;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        _mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //this can happen if a light is destroyed
        if (_targetLight == null)
        {
            HidePanel();
            return;
        }
        //cast point lights world space position to screen space position in order to position this
        Vector3 screenPos = _mainCam.WorldToScreenPoint(_targetLight.transform.position);
        transform.position = screenPos + Vector3.down * 150;
    }

    public void ShowPanel(PointLight light)
    {
        gameObject.SetActive(true);
        _targetLight = light;
        
        Color.RGBToHSV(_targetLight.color, out float h, out float s, out float v);
        hueSlider.value = h;
        saturationSlider.value = s;
        
        Vector3 colorVec = new Vector3(_targetLight.color.r, _targetLight.color.g, _targetLight.color.b);
        float factor = colorVec.magnitude;
        float intensity = Mathf.Log(factor, 2);
        brightnessSlider.value = intensity;
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    public void ChangeSaturation(Slider slider)
    {
        Color col = _targetLight.color;
        Color.RGBToHSV(col, out float h, out float s, out float v);
        col = Color.HSVToRGB(h, slider.value, v);
        _targetLight.SetColor(col);
    }

    public void ChangeHue(Slider slider)
    {
        Color col = _targetLight.color;
        Color.RGBToHSV(col, out float h, out float s, out float v);
        col = Color.HSVToRGB(slider.value, s, v);
        _targetLight.SetColor(col);
    }
    
    private const byte k_MaxByteForOverexposedColor = 191; //internal Unity const
    public void ChangeBrightness(Slider slider)
    {
        Color col = _targetLight.color;

        float intensity = slider.value;
        float factor = Mathf.Pow(2,intensity);
        Vector3 colorVec = new Vector3(col.r, col.g, col.b);
        colorVec = colorVec.normalized * factor;
        col.r = colorVec.x;
        col.g = colorVec.y;
        col.b = colorVec.z;
        _targetLight.SetColor(col);
    }
}
