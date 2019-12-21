using System.Collections;
using System.Collections.Generic;
using GoShared;
using UnityEngine;
using UnityEngine.UI;

public class GOSlippyCompassUI : MonoBehaviour {

    public Image background;
    public Button compassHand;
    public RectTransform handTransform;
    public Camera slippyCamera;

    private Vector3 heading = Vector3.zero;

    private void Reset()
    {
        background = GetComponent<Image>();
        compassHand = GetComponentInChildren<Button>();
        handTransform = compassHand.GetComponent<RectTransform>();
        slippyCamera = Camera.main;
    }


    // Update is called once per frame
    void Update () {
    
        heading.z = 360 - slippyCamera.transform.eulerAngles.y;
        handTransform.rotation = Quaternion.Euler(heading);
	}

}
