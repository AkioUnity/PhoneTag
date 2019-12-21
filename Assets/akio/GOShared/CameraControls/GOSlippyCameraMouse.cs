using System.Collections;
using System.Collections.Generic;
using GoMap;
using GoShared;
using UnityEngine;

public class GOSlippyCameraMouse : MonoBehaviour
{

    public InputPhase dragPhase = InputPhase.Stop;
    private Vector3 mousePosition = Vector3.zero;
    float dragSpeed;

    public InputPhase zoomPhase = InputPhase.Stop;
    private float lastZoomValue = 0;
    private float zoomValue = 0;

    public float zoomDistanceMin = 200f;
    public float zoomDistanceMax = 1000f;
    private float zoomSpeed;
    private float zoomMouseMultiplier = 80;

    public enum InputPhase
    {
        Began,
        Moved,
        Decelerating,
        Stop
    }

    Vector3 hit_position = Vector3.zero;
    Vector3 current_position = Vector3.zero;
    Vector3 camera_position = Vector3.zero;
    Vector3 direction = Vector3.zero;

    void Start()
    {
        if (Application.isMobilePlatform)
        {
            enabled = false;
            return;
        }
    }

    void LateUpdate()
    {

        //Drag detection (Horizontal motion)
        dragPhase = DetectDragPhase();
        switch (dragPhase)
        {
            case InputPhase.Began:
                break;
            case InputPhase.Moved:
                DragCamera();
                break;
            case InputPhase.Decelerating:
                DecelerateDrag();
                break;
            default: break;
        }

        //Zoom detection (Vertical motion)
        zoomPhase = DetectZoomPhase();
        switch (zoomPhase)
        {
            case InputPhase.Began:
            case InputPhase.Moved:
                ZoomCamera();
                break;
            case InputPhase.Decelerating:
                break;
            default: break;
        }

    }

    #region Drag

    public InputPhase DetectDragPhase()
    {

        if (Camera.main == null || GOUtils.IsPointerOverUI() || !transform)
            return InputPhase.Stop;

        if (Input.GetMouseButtonDown(0))
        {
            hit_position = Input.mousePosition;
            camera_position = transform.position;
            return InputPhase.Began;
        }

        else if (Input.GetMouseButton(0))
        {
            current_position = Input.mousePosition;
            return InputPhase.Moved;
        } 
        else {

            hit_position = camera_position = current_position = Vector3.zero;
            return InputPhase.Decelerating;

        }
    }


    //Drag camera on 2D plane
    void DragCamera()
    {

        // From the Unity3D docs: "The z position is in world units from the camera."  In my case I'm using the y-axis as height
        // with my camera facing back down the y-axis.  You can ignore this when the camera is orthograhic.
        current_position.z = hit_position.z = camera_position.y;

        direction = Camera.main.ScreenToWorldPoint(current_position) - Camera.main.ScreenToWorldPoint(hit_position);
        // Invert direction to that terrain appears to move with the mouse.
        direction = direction * -1;

        Vector3 position = camera_position + direction;

        float distance = (transform.position - position).magnitude;
        dragSpeed = Mathf.Max(1,distance) / Time.deltaTime / 100;

        transform.position = position;
    }

    //Decelerate camera speed
    void DecelerateDrag()
    {

        if (dragSpeed <= 0.001)
        {
            dragPhase = InputPhase.Stop;
            return;
        }

        Vector3 v = new Vector3(direction.x,0, direction.z).normalized * dragSpeed;

        transform.position += v;

        dragSpeed = Mathf.Lerp(dragSpeed, 0, Time.deltaTime * 7);

    }

    #endregion

    #region zoom

    InputPhase DetectZoomPhase()
    {
    
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && lastZoomValue == 0)
        {
            zoomValue = -scroll * zoomMouseMultiplier;
            return InputPhase.Began;
        }
        else if (scroll != 0)
        {
            zoomValue = -scroll * zoomMouseMultiplier;
            return InputPhase.Moved;
        }
        else
        {
            return InputPhase.Stop;
        }
    }


    //Drag camera on 2D plane
    void ZoomCamera()
    {
        if (lastZoomValue == 0)
        {
            lastZoomValue = zoomValue;
            return;
        }

        zoomSpeed = (zoomValue - lastZoomValue) / Time.deltaTime;

        //Zoom
        float delta = (-zoomValue + lastZoomValue);

        float newY = Mathf.Clamp(transform.position.y + zoomValue, zoomDistanceMin, zoomDistanceMax);

        //Move camera
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        //Reset last position
        lastZoomValue = zoomValue;

    }

    #endregion

}
