using System.Collections;
using System.Collections.Generic;
using GoShared;
using UnityEngine;
using UnityEngine.UI;

public class GOCompassUI : MonoBehaviour {

    public Image background;
    public Button compassHand;
    public RectTransform handTransform;
    public GOOrbit goOrbit;
    public bool useCompass = false;

    private Vector3 heading = Vector3.zero;

    private void Reset()
    {
        background = GetComponent<Image>();
        compassHand = GetComponentInChildren<Button>();
        handTransform = compassHand.GetComponent<RectTransform>();
        goOrbit = Camera.main.GetComponent<GOOrbit>();
    }


    // Update is called once per frame
    void Update () {

        Input.compass.enabled = useCompass;

        if (useCompass) {
            
            heading.z = Input.compass.trueHeading;
            handTransform.rotation = Quaternion.Euler(heading);

        } else if (goOrbit != null) {
            
            heading.z = goOrbit.currentAngle;
            handTransform.rotation = Quaternion.Euler(heading);
        }
	}

    public void toggleCompass () {

        useCompass = !useCompass;
        goOrbit.rotateWithHeading = useCompass;

    }
}
