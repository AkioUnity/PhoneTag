using System.Collections;
using System.Collections.Generic;
using GoMap;
using GoShared;
using UnityEngine;

public class GOSlippyCameraTouch : MonoBehaviour
{

    public bool unityRemote = false;

    //DRAG
    public InputPhase dragPhase = InputPhase.Stop;
    public bool singleTouchDragOnly = false;

    Vector3 hit_position = Vector3.zero;
    Vector3 current_position = Vector3.zero;
    Vector3 camera_position = Vector3.zero;
    Vector3 direction = Vector3.zero;

    public float dragSpeed;
    private Touch dragTouch;

    public InputPhase zoomPhase = InputPhase.Stop;
    private float lastZoomValue = 0;
    private float zoomValue = 0;
    private float pinchDelta = 0;
    public float pinchMultiplier = 200;

    private float lastTouchDistance = 0;
    public float zoomDistanceMin = 200f;
    public float zoomDistanceMax = 1000f;
    private float zoomSpeed;

    public InputPhase tiltPhase = InputPhase.Stop;
    private float lastTiltValue = 0;
    private float tiltSpeed = 0;
    private float tiltDelta = 0;

    //Drag inertia parameters, make them public to edit
    private float zoomMouseMultiplier = 80;
    private float smoothness = 0.1f;
    private float decay = 7;
    private float maxDragSpeed = 400;

    private float threshold = 100;



    public enum InputPhase
    {
        Began,
        Moved,
        Ended,
        Decelerating,
        Stop
    }

    void Start()
    {
        if (!Application.isMobilePlatform && !unityRemote)
        {
            enabled = false;
            return;
        }
    }

    void LateUpdate()
    {

        ////Tilt detection (Vertical motion) (Only on mobile)
        //tiltPhase = DetectTiltPhase();
        //switch (tiltPhase)
        //{
        //    case InputPhase.Began:
        //    case InputPhase.Moved:
        //        TiltCamera();
        //        break;
        //    case InputPhase.Decelerating:
        //        DecelerateTilt();
        //        break;
        //    default: break;
        //}

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

        //Pinch detection (Vertical motion)
        zoomPhase = DetectZoomPhase();
        switch (zoomPhase)
        {
            case InputPhase.Began:
                break;
            case InputPhase.Moved:
                ZoomCamera();
                break;
            case InputPhase.Decelerating:
                DecelerateZoom();
                break;
            default: break;
        }


    }

    #region Drag

    public InputPhase DetectDragPhase()
    {

        if (Camera.main == null || GOUtils.IsPointerOverUI() || !transform)
            return InputPhase.Stop;

        if (Input.touchCount == 0)
            return dragPhase;

        Touch touch = Input.GetTouch(0);
        dragTouch = touch;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                hit_position = touch.position;
                camera_position = transform.position;
                return InputPhase.Began;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                current_position = touch.position;
                return InputPhase.Moved;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
            default:
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
        position.y = transform.position.y;

        float distance = (transform.position - position).magnitude;
        dragSpeed = Mathf.Max(1, distance) / Time.deltaTime;

        dragSpeed = Mathf.Clamp(dragSpeed, 0, 10);

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

        Vector3 v = new Vector3(direction.x, 0, direction.z).normalized * dragSpeed;

        transform.position += v;

        dragSpeed = Mathf.Lerp(dragSpeed, 0, Time.deltaTime * 7);

    }

    #endregion

    #region zoom

    InputPhase DetectZoomPhase()
    {

        if (Camera.main == null || GOUtils.IsPointerOverUI() || !transform)
            return InputPhase.Stop;

        if (Input.touchCount < 2)
            return zoomPhase;

        if (AtLeastATouchBegan()) {
            lastZoomValue = 0;
            return InputPhase.Began;
        }
        if (TwoTouchesMoingSimoultaneously())
        {
            InputPhase inputPhase;
            inputPhase = InputPhase.Moved;
            return inputPhase;
        }
        else
        {
            lastZoomValue = 0;
            return InputPhase.Decelerating;
        }
    }

    void ZoomCamera () {

        //https://answers.unity.com/questions/970810/accurate-pinch-zoom.html

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        Vector2 prevTouchPosition0 = touch0.position - touch0.deltaPosition;
        Vector2 prevTouchPosition1 = touch1.position - touch1.deltaPosition;
        float touchDistance = (touch1.position - touch0.position).magnitude;
        float prevTouchDistance = (prevTouchPosition1 - prevTouchPosition0).magnitude;

        if (prevTouchDistance == 0)
            return;
  
        float touchChangeMultiplier = touchDistance / prevTouchDistance;

        Vector3 focalPoint = Vector3.zero;

        //Test focal point
        Vector3 position1 = Camera.main.ScreenToWorldPoint(new Vector3(touch0.position.x, touch0.position.y, transform.position.y));
        Vector3 position2 = Camera.main.ScreenToWorldPoint(new Vector3(touch1.position.x, touch1.position.y, transform.position.y));
        focalPoint = Vector3.Lerp(position1, position2, 0.5f);

        //GameObject spehere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //spehere.transform.localScale *= 10;
        //spehere.transform.position = focalPoint;

        Vector3 dir = transform.position - focalPoint;
        float newDistance = dir.magnitude / touchChangeMultiplier;

        transform.position = (newDistance * dir.normalized) + (transform.position + focalPoint);






    }

    //Decelerate camera speed
    void DecelerateZoom()
    {

        zoomPhase = InputPhase.Stop;
        return;

        if (Mathf.Abs(zoomSpeed) <= 1)
        {
            zoomPhase = InputPhase.Stop;
            return;
        }

        zoomSpeed = Mathf.Clamp(zoomSpeed, 0, maxDragSpeed);
        zoomSpeed = Mathf.Lerp(zoomSpeed, 0, Time.deltaTime);

        float delta = zoomSpeed * Time.deltaTime;

        Vector3 v = transform.position + new Vector3(0, delta, 0);
        v.y = Mathf.Clamp(v.y, zoomDistanceMin, zoomDistanceMax);

        transform.position = v;

    }

    #endregion

    #region Tilt

    public InputPhase DetectTiltPhase()
    {

        if (Camera.main == null || GOUtils.IsPointerOverUI() || !transform)
            return InputPhase.Stop;
                
        if (Input.touchCount < 1)
        {
            if (tiltPhase == InputPhase.Decelerating)
                return InputPhase.Decelerating;
            else
                return InputPhase.Stop;
        }

        if (TwoTouchesMoingSimoultaneously())
        {
            InputPhase inputPhase;
            inputPhase = lastTiltValue == 0? InputPhase.Began : InputPhase.Moved;
            return inputPhase;
        }
        else {
            lastTiltValue = 0;
            return InputPhase.Decelerating;
        } 

    }


    //Drag camera on 2D plane
    void TiltCamera()
    {
    
        float tiltValue = AngleBetweenTouches();

        if (lastTiltValue == 0) {
            lastTiltValue = tiltValue;
            return;
        }

        //Height based attenuation
        tiltDelta = tiltValue-lastTiltValue;

        //Speed
        tiltSpeed = tiltDelta / Time.deltaTime;

        //Move camera
        transform.eulerAngles += new Vector3(0, tiltDelta, 0);

        //Reset last position
        lastTiltValue = tiltValue;

    }

    //Decelerate camera speed
    void DecelerateTilt()
    {

        if (tiltSpeed <= 0.5)
        {
            tiltPhase = InputPhase.Stop;
            return;
        }

        tiltSpeed *= 0.2f;

        tiltSpeed = Mathf.Lerp(tiltSpeed, 0, Time.deltaTime);

        transform.eulerAngles += new Vector3(0, Mathf.Sign(tiltDelta) * tiltSpeed, 0);
    }

    void tiltAutomatically () {

        transform.eulerAngles += new Vector3(0, 1, 0);

    }

    #endregion

    #region Touch Utils

    public bool AtLeastATouchBegan()
    {
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began)
                return true;
        }
        return false;
    }

    public bool TwoTouchesMoingSimoultaneously() {

        return Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved;
    }

    public float DistanceBetweenTouches()
    {
        if (Input.touchCount < 2)
            return 0;

        return Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
    }

    public float AngleBetweenTouches()
    {
        if (Input.touchCount < 2)
            return 0;

        Vector3 a = Input.GetTouch(0).position;
        Vector3 b = Input.GetTouch(1).position;

        Vector3 diference = b - a;
        float sign = (b.y < a.y) ? -1.0f : 1.0f;
        return Vector3.Angle(Vector3.right, diference) * sign;
    }

    #endregion
}
