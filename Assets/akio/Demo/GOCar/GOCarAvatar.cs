using UnityEngine;
using System.Collections;
using GoMap;

using GoShared;
using System;
using UnityEngine.Events;


public class GOCarAvatar : MonoBehaviour {

	public GOMap goMap;
	public GameObject avatarFigure;
	public bool autoDrive = false;

	public Camera mainCamera;
	public Camera carCamera;
	public bool carCameraEnabled = false;

	// Use this for initialization
	void Start () {

		goMap.locationManager.onOriginSet.AddListener((Coordinates) => {OnOriginSet(Coordinates);});
		if (goMap.useElevation)
			goMap.OnTileLoad.AddListener((GOTile) => {OnTileLoad(GOTile);});
	}

	public void OnTileLoad (GOTile tile) {

		Vector3 currentLocation = goMap.locationManager.currentLocation.convertCoordinateToVector ();

		if (tile.goTile.vectorIsInTile(currentLocation)) {
//
			Debug.Log ("FIX Start");
//
			currentLocation = GOMap.AltitudeToPoint (currentLocation);
			transform.position = currentLocation;
		} 
	}

	void OnOriginSet (Coordinates currentLocation) { //Here we probably don't have the altitude

		//Position
		Vector3 currentPosition = currentLocation.convertCoordinateToVector ();
		currentPosition.y = transform.position.y;

		transform.position = currentPosition;

	}

	void LateUpdate () {

		if (Input.GetKeyDown(KeyCode.Tab))
			carCameraEnabled = !carCameraEnabled;

		carCamera.enabled = carCameraEnabled;
		mainCamera.enabled  = !carCameraEnabled;

		if (carCameraEnabled) {
			CarMotion ();
		} else {
			OrbitMotion ();
		}

	}

	#region MoveAvatar

	void CarMotion () {

		Vector3 dir = Vector3.ProjectOnPlane (carCamera.transform.forward, Vector3.down);

		if (Input.GetKey (KeyCode.D))
			dir = Quaternion.AngleAxis(10, Vector3.up) * dir;
		else if (Input.GetKey (KeyCode.A))
			dir = Quaternion.AngleAxis(-10, Vector3.up) * dir;

		Debug.DrawLine (carCamera.transform.position, carCamera.transform.position + dir*100, Color.green,1);

		Vector3 lastPosition = transform.position;

		bool thrust = Input.GetKey (KeyCode.W) || autoDrive;
		bool reverseThrust = Input.GetKey(KeyCode.S);

		int reverse = 1;
		if (reverseThrust) {
			reverse = -1;
			thrust = true;
		}
			
		float speed = 0.8f;

		if (thrust && !GOUtils.IsPointerOverUI ()) {
			transform.Translate (Time.deltaTime * (speed * 60 * avatarFigure.transform.forward * reverse));
			if (goMap.useElevation) {
				transform.localPosition = GOMap.AltitudeToPoint (transform.localPosition);

				int f = lastPosition.y > transform.localPosition.y ? -1 : 1;
				dir.y = Math.Abs(lastPosition.y - transform.localPosition.y) * f * reverse;

			}
		}

		rotateAvatar (dir);

	}

	void OrbitMotion () {


		Vector3 dir = Vector3.forward;
		dir =  Camera.main.transform.forward;
		dir = Vector3.ProjectOnPlane (dir, Vector3.down);

		Vector3 lastPosition = transform.position;

		Vector3 v1 = Vector3.forward;
		bool drag = false;
		if (Application.isMobilePlatform) {
			drag = Input.touchCount >= 1;
			if (drag)
				v1 = Input.GetTouch (0).position;
		} else {
			drag = Input.GetMouseButton (0);
			if (drag)
				v1 = Input.mousePosition;
		}


		Vector3 v2 = Camera.main.WorldToScreenPoint (avatarFigure.transform.position);
		float d = Vector2.Distance (v1, v2)/Screen.height;

		if (autoDrive) {
			d = 1; 
			drag = true;
		}

		if (d < 0.5f)
			d = 0.5f;

		int reverse = 1;
		if (v1.y > v2.y && Mathf.Abs (v2.x - v1.x) < 80) {
			reverse = -1;
			d = -d;
		}


		if (drag && !GOUtils.IsPointerOverUI ()) {
			transform.Translate (Time.deltaTime * (d * 60 * avatarFigure.transform.forward));
			if (goMap.useElevation) {
				transform.localPosition = GOMap.AltitudeToPoint (transform.localPosition);

				int f = lastPosition.y > transform.localPosition.y ? -1 : 1;
				dir.y = Math.Abs(lastPosition.y - transform.localPosition.y) * f * reverse;

			}
		}

		rotateAvatar (dir);

	}

	#endregion

	void rotateAvatar(Vector3 targetDir) {

		if (carCameraEnabled)
			targetDir.y /= 5;

		if (targetDir != Vector3.zero) {
			avatarFigure.transform.rotation = Quaternion.Slerp(
				avatarFigure.transform.rotation,
				Quaternion.LookRotation(targetDir),
				Time.deltaTime * 5
			);
		}
	}


}
