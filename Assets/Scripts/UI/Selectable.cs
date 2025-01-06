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

    private bool _isSelected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_isSelected)
        {
            if((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.D))
            {
                GameObject dupe = Instantiate(gameObject, transform.position + Vector3.one * 0.1f, transform.rotation,
                    transform.parent);
                dupe.GetComponent<Selectable>().Select();
            }
        }
        
    }

    public void Select()
    {
        if (selected != null)
        {
            selected.Deselect();
        }

        selected = this;
        _isSelected = true;
        onSelect?.Invoke();
        Debug.Log("Selected: " + gameObject.name);
    }

    public void Deselect()
    {
        _isSelected = false;
        onDeselect?.Invoke();
    }

    private void OnMouseUpAsButton()
    {
        Select();
    }
}
