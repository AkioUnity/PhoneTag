using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace GoShared {

	public class GOCameraLook : MonoBehaviour {

		private Quaternion ResetCamera;

		public float speed = 2.0f;
		public bool rotateParent = false;
		Transform objToRotate;

		void Start () {

			if (rotateParent) {
				objToRotate = transform.parent;
			} else {
				objToRotate = transform;
			}

			ResetCamera = objToRotate.rotation;	 

		}

		void Update() {
			
			objToRotate.Rotate(handleInput ());
			objToRotate.localEulerAngles = new Vector3 (objToRotate.localEulerAngles.x,objToRotate.localEulerAngles.y,0);

			#if UNITY_EDITOR
			resetDefault ();
			#endif

		}

		void LateUpdate () {

			handleDrag ();
		}

		private void resetDefault() {
			
			if (Input.GetMouseButton (1)) {
				objToRotate.rotation=ResetCamera;
			}
		}

		private Vector3 handleInput() {

			Vector3 p_Velocity = new Vector3();

			if (!Application.isMobilePlatform) {

				if (Input.GetKey (KeyCode.UpArrow)){
					p_Velocity +=  Vector3.left * speed;
				}
				if (Input.GetKey (KeyCode.DownArrow)){
					p_Velocity += Vector3.right * speed;
				}
				if (Input.GetKey (KeyCode.LeftArrow)){
					p_Velocity += Vector3.down * speed;
				}
				if (Input.GetKey (KeyCode.RightArrow)){
					p_Velocity += Vector3.up *speed;
				}
			}

			return p_Velocity;

		
		}

		private void handleDrag () { 

			float lastAngleX = transform.eulerAngles.x; //Vertical
			float lastAngleY = transform.eulerAngles.y; //Horizontal


			if (Application.isMobilePlatform) {

				if (Input.touchCount == 1 && Input.GetTouch (0).phase == TouchPhase.Moved) { 

					Vector2 touchDeltaPosition = Input.GetTouch (0).deltaPosition;
					transform.Rotate(new Vector3 (-touchDeltaPosition.y * speed * Time.deltaTime,touchDeltaPosition.x * speed * Time.deltaTime,0));

				}

			}
			else {
				if (Input.GetMouseButton(0)) {

					lastAngleX += Input.GetAxis ("Mouse Y") * speed;
					lastAngleY += -Input.GetAxis ("Mouse X") * speed;
					objToRotate.eulerAngles = new Vector3 (lastAngleX,lastAngleY,transform.eulerAngles.z);
				}
			}
		}
	}
}
