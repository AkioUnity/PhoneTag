using UnityEngine;

	namespace GoShared {
	public class GOCameraClipPlane : MonoBehaviour {

		public bool showDebug = false;
		public GOClipPlane clipPlane;

		void Start () {
			clipPlane = new GOClipPlane (Camera.main);
		}

		// Update is called once per frame
		void Update () {

			clipPlane.UpdateNearClipPlane ();

			if (showDebug) {
				clipPlane.ShowDebugLines ();
			}

		}
	}
}
