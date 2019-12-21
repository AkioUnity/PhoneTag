using UnityEngine;
using System.Collections;

namespace GoShared {
	[ExecuteInEditMode]
	[RequireComponent(typeof(Canvas))]
	public class GOCanvasManager : MonoBehaviour 
	{

		public Canvas canvas { get; private set; }
		public RectTransform rectTransform { get; private set; }

		private void Awake()
		{
			canvas = GetComponent<Canvas>();
			rectTransform = GetComponent<RectTransform>();
		}

		// See result when builded to device.
		private void OnEnable()
		{
			canvas.scaleFactor = Screen.dpi / 160f;;
		}

		#if UNITY_EDITOR
		private Vector2 currentSizeDelta;
		private Vector3 currentScale;

		// Update Layout in editor & runtime
		internal void Update()
		{

			Vector2 pixel = new Vector2(Screen.width, Screen.height);
			float dpi = Screen.dpi;
			UpdateDevice(dpi,pixel, false, Vector3.one);
			UpdateRect();
		}

		internal void UpdateDevice (float dpi, Vector2 pixel, bool physicalSize, Vector3 baseScale)
		{

			Vector2 screenSize = pixel;
			Vector2 actualSize = (pixel / dpi) * Screen.dpi;

			float screenScale = actualSize.magnitude / screenSize.magnitude;

			float scaleWidth = 1f;
			float scaleHeight = 1f;
			float scale = 1f;

			float maxWidth = Screen.width;
			float maxHeight = Screen.height;

			canvas.scaleFactor = 1f;

			if(canvas.renderMode == RenderMode.ScreenSpaceCamera)
			{
				Camera camera = canvas.worldCamera;
				float cameraHeight = 0f;

				if(true == camera.orthographic)
				{
					// Ortho Camera
					cameraHeight = camera.orthographicSize * 2f;
				}
				else
				{
					// Perspective Camera
					float fov = camera.fieldOfView;
					float focalLength = 2f * Mathf.Tan( fov / 2f * Mathf.Deg2Rad );
					float perspectiveHeight = canvas.planeDistance * focalLength;

					cameraHeight = perspectiveHeight;
				}

				float cameraWidth = (maxWidth / maxHeight) * cameraHeight;

				float normalWidth = actualSize.x / maxWidth;
				float normalHeight = actualSize.y / maxHeight;

				float realWidth = normalWidth * cameraWidth;
				float realHeight = normalHeight * cameraHeight;

				if(realWidth > cameraWidth || false == physicalSize)
				{	
					realWidth = (cameraWidth / realWidth) * realWidth;
				}

				if(realHeight > cameraHeight || false == physicalSize)
				{
					realHeight = (cameraHeight / realHeight) * realHeight;
				}

				scaleWidth = realWidth / actualSize.x;
				scaleHeight = realHeight / actualSize.y;
			}
			else
			{
				if(actualSize.x > maxWidth || false == physicalSize)
				{	
					scaleWidth = maxWidth / actualSize.x;
				}

				if(actualSize.y > maxHeight || false == physicalSize)
				{
					scaleHeight = maxHeight / actualSize.y;
				}
			}

			// Uniform Scale
			scale = Mathf.Min(scaleWidth, scaleHeight) * screenScale;

			// Update Data
			currentSizeDelta = screenSize;
			currentScale = baseScale * scale;//Vector3.one  scale;
		}

		private void UpdateRect()
		{
			rectTransform.sizeDelta = currentSizeDelta;
			rectTransform.localScale = currentScale;
		}
		#endif
	}
}