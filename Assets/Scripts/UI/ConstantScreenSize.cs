using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantScreenSize : MonoBehaviour
{
    public float widthInPercentage;

    private Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(mainCam.transform.position, transform.position);
        float width = Mathf.Tan(mainCam.fieldOfView * Mathf.Deg2Rad) * dist * 2;
        float height = width / mainCam.aspect;
        float widthInWorld = width * widthInPercentage / 100;
        float heightInWorld = height * widthInPercentage / 100;
        transform.localScale = widthInWorld * Vector3.one;

    }
}
