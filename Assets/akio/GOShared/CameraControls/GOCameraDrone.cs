using UnityEngine;
using System.Collections;

namespace GoShared {

	public class GOCameraDrone : MonoBehaviour {

		#if (!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR
		[Header("This script is only for desktop builds")]

		public float mainSpeed = 100.0f; //regular speed
		public float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
		public float maxShift = 1000.0f; //Maximum speed when holdin gshift
		private float totalRun  = 1.0f;

		public bool moveParent = false;
		Transform objToMove;

		void  Start (){
			
			if (moveParent) {
				objToMove = transform.parent;
			} else {
				objToMove = transform;
			}
		}	

		void  Update (){
			
			//Keyboard commands
			Vector3 p= GetBaseInput();
			Vector3 pAltitude= GetBaseInputAltitude() ;

			if (Input.GetKey (KeyCode.LeftShift)){
				totalRun += Time.deltaTime;
				p  = p * totalRun * shiftAdd;
				p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
				p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
				p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
			}
			else{
				totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000);
				p = p * mainSpeed;
			}

			p = p * Time.deltaTime;

			if (Input.GetKey (KeyCode.LeftShift)){
				totalRun += Time.deltaTime;
				pAltitude  = pAltitude * totalRun * shiftAdd;
				pAltitude.x = Mathf.Clamp(pAltitude.x, -maxShift, maxShift);
				pAltitude.y = Mathf.Clamp(pAltitude.y, -maxShift, maxShift);
				pAltitude.z = Mathf.Clamp(pAltitude.z, -maxShift, maxShift);
			}
			else{
				totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000);
				pAltitude = pAltitude * mainSpeed;
			}

			pAltitude = pAltitude * Time.deltaTime;


			objToMove.Translate( p);
			objToMove.position += pAltitude;

		}

		private Vector3 GetBaseInput (){ //returns the basic values, if it's 0 than it's not active.
			Vector3 p_Velocity = new Vector3();
			if (Input.GetKey (KeyCode.W)){
				p_Velocity += new Vector3(0, 0 , 1);
			}
			if (Input.GetKey (KeyCode.S)){
				p_Velocity +=new Vector3(0, 0 , -1);
			}
			if (Input.GetKey (KeyCode.A)){
				p_Velocity +=new Vector3(-1, 0 , 0);
			}
			if (Input.GetKey (KeyCode.D)){
				p_Velocity +=new Vector3(1, 0 , 0);
			}

			return p_Velocity;
		}
			
		private Vector3 GetBaseInputAltitude (){ 
			Vector3 p_Velocity = new Vector3();
			if (Input.GetKey (KeyCode.Q)){
				p_Velocity -= Vector3.up;
			}

			if (Input.GetKey (KeyCode.E)){
				p_Velocity += Vector3.up;
			}

			return p_Velocity;
		}

		#endif
	}

}