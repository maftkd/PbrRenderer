using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Draggable : MonoBehaviour
{
    private bool _dragging = false;
    public Transform moveTarget;

    private Camera mainCam;

    public UnityEvent<Vector3> OnDrag;

    public GameObject axisGameObject;

    private Vector3 _offset;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        if (axisGameObject != null)
        {
            axisGameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_dragging)
        {
            
            moveTarget.transform.position = GetPointOnLineNearMouse() + _offset;
            
            OnDrag?.Invoke(moveTarget.position);
            
            if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
                if (axisGameObject != null)
                {
                    axisGameObject.SetActive(false);
                }
            }
        }
    }

    private Vector3 GetPointOnLineNearMouse()
    {
        //Determine plane containing the axis we want to move in that is also mostly perpendicular to the camera
        Vector3 perpendicularVec = Vector3.Cross(mainCam.transform.forward, transform.up);
        Vector3 castPlaneNormal = Vector3.Cross(perpendicularVec, transform.up);
        Plane plane = new Plane(castPlaneNormal, moveTarget.position);
        
        //cast mouse to plane
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Vector3 planeTargetPos = Vector3.zero;
        if (plane.Raycast(ray, out float distance))
        {
            planeTargetPos = ray.GetPoint(distance);
        }
        
        //now we want to calculate the nearest point on a perpendicular plane to the planeTargetPos
        Vector3 diffToPerpendicular = Vector3.Project(planeTargetPos - moveTarget.position, perpendicularVec);
        return planeTargetPos - diffToPerpendicular;
    }

    void OnMouseDown()
    {
        Debug.Log("Yoo");
        _dragging = true;
        if (axisGameObject != null)
        {
            axisGameObject.SetActive(true);
        }

        Vector3 initialPos = GetPointOnLineNearMouse();
        _offset = moveTarget.position - initialPos;
    }


    private void OnDrawGizmos()
    {
    }
}
