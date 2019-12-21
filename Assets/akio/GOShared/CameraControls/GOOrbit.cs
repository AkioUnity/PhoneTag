using UnityEngine;
using System.Collections;

namespace GoShared {

	[AddComponentMenu("Camera-Control/GOOrbit")]
	public class GOOrbit : MonoBehaviour {

		public Transform target;
		public float distance = 55.0f;
		public float orbitSpeed = 1.0f;
//		public float pinchSpeed = 3.0f;

		public float yMinLimit = 20f;
		public float yMaxLimit = 60f;

		public float distanceMin = 20f;
		public float distanceMax = 200f;

		public float offset;

		public bool orbitParent;
		Transform objToRotate;

		public AnimationCurve zoomCurve;

		private Rigidbody _rigidbody;

		float prevPinchDist = 0f;
		float prevAngle = 0f;

        [HideInInspector] public float currentAngle;

		public bool autoOrbit = false;

		GOClipPlane clipPlane;

        public bool rotateWithHeading = false;

		// Use this for initialization
		void Start () 
		{

			clipPlane = new GOClipPlane (Camera.main);

			if (orbitParent) {
				objToRotate = transform.parent;
			} else {
				objToRotate = transform;
			}

			_rigidbody = objToRotate.gameObject.GetComponent<Rigidbody>();

			//// Make the rigid body not change rotation
			//if (_rigidbody != null)
			//{
			//	_rigidbody.freezeRotation = true;
			//}

			updateOrbit (true);

		}

		void LateUpdate () 
		{

			//		bool condition = (Application.isMobilePlatform && Input.touchCount > 0) || (!Application.isMobilePlatform && (Input.GetMouseButton(0)|| Input.GetAxis("Mouse ScrollWheel") != 0));
            bool condition = (Application.isMobilePlatform && Input.touchCount > 0) || !Application.isMobilePlatform || rotateWithHeading;

			if (target && condition && !GOUtils.IsPointerOverUI()) {
				updateOrbit (false);
			} 
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360F)
				angle += 360F;
			if (angle > 360F)
				angle -= 360F;
			return Mathf.Clamp(angle, min, max);
		}

		private float distanceToAngle ()
		{
			float distanceFactor = (distance / distanceMax);
			float angle = 90 * distanceFactor;

			return angle;
		}

		void updateOrbit (bool firstLaunch) {

			if (Camera.main == null)
				return;

			bool drag = false;

			Vector3 v1 = Vector3.forward;
			if (Application.isMobilePlatform) {
				drag = Input.touchCount == 1 && Input.GetTouch (0).phase == TouchPhase.Moved;
				if (drag)
					v1 = Input.GetTouch (0).position;
			} else {
				drag = Input.GetMouseButton (0);
				if (drag)
					v1 = Input.mousePosition;
			}

            if (drag || firstLaunch || autoOrbit || rotateWithHeading) {

				Vector3 v2 = Camera.main.WorldToScreenPoint (target.position);

				float angle = (Mathf.Atan2 (v1.y - v2.y, v1.x - v2.x) * 180.0f / Mathf.PI) + 180.0f;
				if (firstLaunch)
					angle = 0f;

				if (prevAngle == 361) {
					prevAngle = angle;
				}

				if (autoOrbit) {
				
					prevAngle = angle;
					currentAngle += orbitSpeed;

                } else if (rotateWithHeading) {
                    
                    Input.compass.enabled = true;
                    currentAngle = Input.compass.trueHeading;

                } else if (angle != prevAngle) {
					
					float delta = angle - prevAngle;
					if (delta > 180.0f) {
						delta -= 360;
					} else if (delta < -180.0f) {
						delta += 360;
					}
					prevAngle = angle;
					currentAngle += delta * orbitSpeed;
				}
			} else {
				prevAngle = 361;
			}


			float deltaD = 0;
			if (Application.isMobilePlatform) {
				if (Input.touchCount >= 2) {
					Vector2 touch0, touch1;
					float d;
					touch0 = Input.GetTouch (0).position;
					touch1 = Input.GetTouch (1).position;
					d = Mathf.Abs (Vector2.Distance (touch0, touch1));

					deltaD = Mathf.Clamp (prevPinchDist - d, -1, 1) * (distanceMax - distanceMin) / 25;  //pinchSpeed;
					prevPinchDist = d;

					distance = Mathf.Clamp (distance + deltaD, distanceMin, distanceMax);

				}
			} else {

				deltaD = Input.GetAxis ("Mouse ScrollWheel") * (distanceMax - distanceMin) / 25;
				float newD = distance - deltaD;
				distance = Mathf.Clamp (newD, distanceMin, distanceMax);

			}


			if (clipPlane != null && clipPlane.IsAboutToClip (false)) {
			
				distance += deltaD + 2;
			}


			float height = EvaluateCurrentHeight (distance);


			Quaternion rotation = Quaternion.Euler(height, currentAngle, 0);
			Vector3 negDistance = new Vector3(0.0f, 0.0f, - distance);
			Vector3 position = rotation * negDistance + target.position;

             objToRotate.rotation = rotation * Quaternion.Euler(-offset, 0, 0);
             objToRotate.position = position;

		}

		float EvaluateCurrentHeight (float currentDistance) {
		
			float convValue = (distance- distanceMin) / (distanceMax - distanceMin);
			float factor = zoomCurve.Evaluate (convValue);

			float height = factor *(yMaxLimit-yMinLimit) + yMinLimit;

			return height;

		}



	}

}