using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace GoShared {

	public class GOClipPlane {

		Camera camera;

		public Vector3 lowerRight;
		public Vector3 lowerLeft;
		public Vector3 upperRight;
		public Vector3 upperLeft;

		public float height;
		public float width;

		public GOClipPlane (Camera c) {
			camera = c;
		}

		public void UpdateNearClipPlane () {

			UpdateClipPlane (camera.nearClipPlane);
		}

		public void UpdateFarClipPlane () {

			UpdateClipPlane (camera.farClipPlane);

		}

		private void UpdateClipPlane (float distance) {

			float halfFov = camera.fieldOfView / 2 * Mathf.Deg2Rad;

			height = Mathf.Tan (halfFov) * distance;
			width = height * camera.aspect;

			Vector3 center = camera.transform.position + camera.transform.forward * distance;

			//Lower right
			lowerRight = center + (camera.transform.right * width) - camera.transform.up* height;
			//Lower left
			lowerLeft = center - (camera.transform.right * width) - camera.transform.up* height;
			//Upper rigth
			upperRight = center + (camera.transform.right * width) + camera.transform.up* height;
			//Upper left
			upperLeft = center - (camera.transform.right * width) + camera.transform.up* height;

		}

		public void ShowDebugLines () {

			float t = 0.5f;

			Debug.DrawLine (lowerRight, lowerLeft, Color.red,t);
			Debug.DrawLine (upperRight, upperLeft, Color.red,t);
			Debug.DrawLine (upperRight, lowerRight, Color.red,t);
			Debug.DrawLine (upperLeft, lowerLeft, Color.red,t);

		}


		#region Collision

		public bool IsAboutToClip (bool debug = false) {
		
			Profiler.BeginSample ("[GoMap] Clip Pane Check");
			UpdateClipPlane (camera.nearClipPlane + 10);

			bool h = false;

			if (intersectionBetweenPoints (upperRight, lowerLeft, debug) || intersectionBetweenPoints (lowerRight, upperLeft,debug) || intersectionBetweenPoints(lowerLeft,lowerRight,debug)) {
				h = true;
			}
			Profiler.EndSample ();
			return h;
		}

		private bool intersectionBetweenPoints (Vector3 a, Vector3 b, bool debug) {
		
			if(Physics.Linecast(a,b))
			{
				if (debug) {
					Debug.DrawLine (a, b, Color.red, 0.5f);
				}
				return true;
			} 

			if (debug) {
				Debug.DrawLine (a, b, Color.green, 0.5f);
			}

			return false;
		}

		#endregion


		#region Static


		public static GOClipPlane MainCameraNearClipPlane() {

			Camera c = Camera.main;

			GOClipPlane clipPlane = new GOClipPlane (c);

			clipPlane.UpdateNearClipPlane ();

			return clipPlane;
		}

		public static GOClipPlane MainCameraFarClipPlane() {

			Camera c = Camera.main;

			GOClipPlane clipPlane = new GOClipPlane (c);

			clipPlane.UpdateNearClipPlane ();

			return clipPlane;
		}


		#endregion
	}

}