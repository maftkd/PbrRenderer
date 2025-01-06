using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformable : MonoBehaviour
{
    public static GameObject transformGizmo;

    void Awake()
    {
        if (transformGizmo == null)
        {
            transformGizmo = Instantiate(Resources.Load<GameObject>("TransformGizmo"));
            transformGizmo.SetActive(false);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableTransformation()
    {
        transformGizmo.SetActive(true);
        transformGizmo.transform.position = transform.position;
    }

    public void DisableTransformation()
    {
        transformGizmo.SetActive(false);
        
    }
}
