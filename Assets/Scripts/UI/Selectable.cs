using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Selectable : MonoBehaviour
{
    public static Selectable selected;
    public UnityEvent onSelect;
    public UnityEvent onDeselect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        if (selected != null)
        {
            selected.Deselect();
        }

        selected = this;
        onSelect?.Invoke();
        Debug.Log("Selected: " + gameObject.name);
    }

    public void Deselect()
    {
        onDeselect?.Invoke();
    }

    private void OnMouseUpAsButton()
    {
        Select();
    }
}
