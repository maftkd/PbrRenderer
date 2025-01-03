using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLayout : MonoBehaviour
{
    public Vector2 spacing;
    public GameObject sphere;
    public int levels;
    // Start is called before the first frame update
    void Start()
    {
        for (int y = 0; y < levels; y++)
        {
            float yNorm = y / (float)(levels - 1);
            float yPos = Mathf.Lerp(-spacing.y, spacing.y, yNorm);
            for (int x = 0; x < levels; x++)
            {
                float xNorm = x / (float)(levels - 1);
                float xPos = Mathf.Lerp(-spacing.x, spacing.x, xNorm);
                Vector3 pos = new Vector3(xPos, yPos, 0);
                
                GameObject sphere = Instantiate(this.sphere, pos, Quaternion.identity, transform);
                Material mat = sphere.GetComponent<Renderer>().material;
                mat.SetFloat("_Metallic", yNorm);
                mat.SetFloat("_Roughness", xNorm);
            }
        }
        
        sphere.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(spacing.x, spacing.y, 0) * 2f);
    }
}
