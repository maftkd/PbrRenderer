using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Draggable : MonoBehaviour
{
    private bool _dragging = false;
    public Transform moveTarget;

    private Camera mainCam;

    public UnityEvent<Vector3> OnDrag;

    private Vector3 _debugPoint;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (_dragging)
        {
            //need to figure out where to move the target based on mouse position and a line in 3d space
            //in this case we can assume the direction we want to move along is the transforms forward
            //we can get the mouse position in world space by using the camera
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            Vector3 perpendicularVec = Vector3.Cross(mainCam.transform.forward, transform.up);
            Vector3 castPlaneNormal = Vector3.Cross(perpendicularVec, transform.up);
            _debugPoint = transform.position + castPlaneNormal;
            Plane plane = new Plane(castPlaneNormal, moveTarget.position);
            if (plane.Raycast(ray, out float distance))
            {
                moveTarget.position = ray.GetPoint(distance);
            }
            
            OnDrag?.Invoke(moveTarget.position);

            if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Yoo");
        _dragging = true;
    }

    private void OnDrawGizmos()
    {
        if (_debugPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _debugPoint);
        }
    }
}
