using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {

	public class GOCoordinatesRaycast : MonoBehaviour {

		public GameObject projectorPrefab;
		private GameObject currentProjector;

		public bool debugLog = false;
		public bool atomic = true;

		//In the update we detect taps of mouse or touch to trigger a raycast on the ground
		void Update() {

			bool drag = false;
			if (Application.isMobilePlatform) {
				drag = Input.touchCount == 1 && Input.GetTouch (0).phase == TouchPhase.Began;
			} else 
				drag = Input.GetMouseButton (0);

			if (drag) {

				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast (ray, out hit,Mathf.Infinity,GOMap.GetActiveMasks())) {

					//From the raycast data it's easy to get the vector3 of the hit point 
					Vector3 worldVector = hit.point;
					//And it's just as easy to get the gps coordinate of the hit point.
					Coordinates gpsCoordinates = Coordinates.convertVectorToCoordinates (hit.point);

					if (debugLog) {
						//There's a little debug string
						Debug.Log (string.Format ("[GOMap] Tap world vector: {0}, GPS Coordinates: {1}", worldVector, gpsCoordinates.toLatLongString ()));
					}

					if (currentProjector != null && atomic)
						GameObject.Destroy(currentProjector);

					//Add a simple projector to the tapped point
					currentProjector = GameObject.Instantiate(projectorPrefab);
					worldVector.y += 5.5f;
					currentProjector.transform.position = worldVector;
				}
			}
		}




	}
}
