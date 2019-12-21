using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Profiling;
using LocationManagerEnums;

namespace GoShared {

    public class LocationManager : BaseLocationManager {


        [Header("Location Settings")]
		public bool useLocationServices;
		public DemoLocation demoLocation;

        [HideInInspector] public float updateDistance = 0.1f;

        [Header("Test GPS updates Settings")]
        public MotionPreset simulateMotion = MotionPreset.Run;
		float demo_WASDspeed = 20;
		public bool useWsadInEditor = true;

        [Header("Avatar Settings")]
        public MotionMode motionMode = MotionMode.GPS;
		public GameObject avatar;

        [Header("Banner Settings")]
		public bool useBannerInsideEditor;
		public GameObject banner;
		public Text bannerText;

		public static bool UseLocationServices;
		public static LocationServiceStatus status;

        private float updateEvery = 1 / 1000f;

        private Camera cam;

		// Use this for initialization
		void Start () {

            if ((Application.isEditor || !Application.isMobilePlatform) && motionMode != MotionMode.UnityRemote) {
				useLocationServices = false;
			}

            if (Camera.main != null) {
                cam = Camera.main;
            }

            switch (motionMode)
			{
			case MotionMode.Avatar:
				LoadDemoLocation ();
				updateEvery = 1;
				StartCoroutine(LateStart(0.01f));
				break;
			case MotionMode.GPS:

				//if (useLocationServices) {
				//	Input.location.Start (desiredAccuracy, updateDistance);
				//} else { //Demo origin
				//	LoadDemoLocation ();
				//}
				//UseLocationServices = useLocationServices;
				//updateEvery = 0.1f;
				//StartCoroutine(LateStart(0.01f));
                    StartCoroutine(StartGPS(0));
				break;
                case MotionMode.UnityRemote:

                    StartCoroutine(StartGPS(4));
                break;
			default:
				break;
			}
		}

        IEnumerator StartGPS (float waitTime) {

            yield return new WaitForSeconds(waitTime);

            if (useLocationServices)
            {
                Input.location.Start(desiredAccuracy, updateDistance);
            }
            else
            { //Demo origin
                LoadDemoLocation();
            }
            UseLocationServices = useLocationServices;
            updateEvery = 0.1f;
            StartCoroutine(LateStart(0.01f));

            yield return null;
        }
			
		IEnumerator LateStart(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			if (!useLocationServices && demoLocation != DemoLocation.NoGPSTest && demoLocation != DemoLocation.SearchMode) {
				adjust (); //This adjusts the current location just after the initialization
			}
		}

		float tempTime;
		public void Update () {

			Profiler.BeginSample("[LocationManager] Update");
			tempTime += Time.deltaTime;
			if (tempTime > updateEvery ) {
				tempTime -= updateEvery;
				switch (motionMode)
				{
				case MotionMode.Avatar:
					AvatarPositionCheck ();
					break;
				case MotionMode.GPS:
                case MotionMode.UnityRemote:
					GPSLocationCheck ();
					break;
				default:
					break;
				}
			}
			Profiler.EndSample ();

		}

        void adjust()
        {

            Vector3 current = currentLocation.convertCoordinateToVector();
            Vector3 v = current;
            currentLocation = Coordinates.convertVectorToCoordinates(v);
            //          v = current + new Vector3(0, 0 , 0.1f)*worldScale;
            currentLocation = Coordinates.convertVectorToCoordinates(v);

            switch (motionMode)
            {
                case MotionMode.Avatar:
                    if (onOriginSet != null)
                    {
                        onOriginSet.Invoke(currentLocation);
                    }
                    break;
                case MotionMode.GPS:
                    if (onLocationChanged != null)
                    {
                        onLocationChanged.Invoke(currentLocation);
                    }
                    break;
                default:
                    break;
            }
        }


		#region Location Updates

		void GPSLocationCheck () {

			status = Input.location.status;

			if (!useLocationServices) {
				if (Application.isEditor && useBannerInsideEditor)
					showBannerWithText (true, "GPS is disabled");
			}
			else if (status == LocationServiceStatus.Failed) {
				showBannerWithText (true, "GPS signal not found");
			}
			else if (status == LocationServiceStatus.Stopped) {
				showBannerWithText (true, "GPS signal not found");
			}
			else if (status == LocationServiceStatus.Initializing) {
				showBannerWithText (true, "Waiting for GPS signal");
			} 
			else if (status == LocationServiceStatus.Running) {

				if (Input.location.lastData.horizontalAccuracy > desiredAccuracy) {
					showBannerWithText (true, "GPS signal is weak");
				} else {
					showBannerWithText (false, "GPS signal ok!");

					if (!IsOriginSet) {
						SetOrigin (new Coordinates (Input.location.lastData));
					}
					LocationInfo info = Input.location.lastData;
					if (info.latitude != currentLocation.latitude || info.longitude != currentLocation.longitude) {

                        if (worldOrigin.TooFarFromOrigin(info)) {
                            Debug.Log("[LocationManager] too far from origin, reset.");
                            SetLocation(new Coordinates(Input.location.lastData));
                            return;
                        }

						currentLocation.updateLocation (Input.location.lastData);
						if (onLocationChanged != null) {
							onLocationChanged.Invoke (currentLocation);
						}
					}
					CheckMotionState (new Coordinates(Input.location.lastData));
				}
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer)
				changeLocationWASD ();
			if (!useLocationServices && (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer) && demoLocation != DemoLocation.NoGPSTest && demoLocation != DemoLocation.SearchMode && !GOUtils.IsPointerOverUI() && motionMode != MotionMode.Avatar) {
				changeLocationWASD ();
			}

		}

		void AvatarPositionCheck () {

            if (avatar != null && worldOrigin != null && !worldOrigin.isZeroCoordinates()) {
				currentLocation = Coordinates.convertVectorToCoordinates (avatar.transform.position);
				if (onLocationChanged != null) {
					onLocationChanged.Invoke (currentLocation);
				}
			}

		}

		#endregion;

        #region Search Mode

        public void SetLocation(Coordinates newLocation)
        {

            SetOrigin(newLocation);
            currentLocation = newLocation;
            adjust();
        }

        public void SetOriginAndLocation(Coordinates origin, Coordinates newLocation)
        {

            SetOrigin(origin);
            currentLocation = newLocation;
            adjust();
        }

        #endregion

        #region UI

        ////UI
        void showBannerWithText(bool show, string text) {

			if (banner == null || bannerText == null) {
				return;
			}

			bannerText.text = text;

			RectTransform bannerRect = banner.GetComponent<RectTransform> ();
			bool alreadyOpen = bannerRect.anchoredPosition.y != bannerRect.sizeDelta.y;

			if (show != alreadyOpen) {
				StartCoroutine (Slide (show, 1));
			}

		}

		private IEnumerator Slide(bool show, float time) {

//			Debug.Log ("Toggle banner");

			Vector2 newPosition;
			RectTransform bannerRect = banner.GetComponent<RectTransform> ();

			if (show) {//Open
				newPosition = new Vector2 (bannerRect.anchoredPosition.x, 0);
			} else { //Close
				newPosition = new Vector2 (bannerRect.anchoredPosition.x, bannerRect.sizeDelta.y);
			} 

			float elapsedTime = 0;
			while (elapsedTime < time)
			{
				bannerRect.anchoredPosition = Vector2.Lerp(bannerRect.anchoredPosition, newPosition, (elapsedTime / time));
				elapsedTime += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
				
		}

//		public void OnGUI () {
//
//			GUIStyle guiStyle = new GUIStyle(); 
//			guiStyle.fontSize = 30; 
//			GUILayout.Label(currentSpeed + " "+currentMotionState.ToString(), guiStyle);
//
//		}

		#endregion


		#region GPS MOTION TEST

		void changeLocationWASD (){

			if (!useWsadInEditor)
				return;

			switch (simulateMotion)
			{
			case MotionPreset.Car:
				demo_WASDspeed = 4;
				break;
			case MotionPreset.Bike:
				demo_WASDspeed = 2;
				break;
			case MotionPreset.Run:
				demo_WASDspeed = 0.8f;
				break;
			default:
				break;
			}


			Vector3 current = currentLocation.convertCoordinateToVector ();
			Vector3 v = current;

            if (cam == null)
            {

                if (Input.GetKey(KeyCode.W))
                {
                    v = current + new Vector3(0, 0, demo_WASDspeed);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    v = current + new Vector3(0, 0, -demo_WASDspeed);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    v = current + new Vector3(-demo_WASDspeed, 0, 0);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    v = current + new Vector3(demo_WASDspeed, 0, 0);
                }

            } else {

                if (Input.GetKey(KeyCode.W))
                {
                    v = current + cam.transform.forward.ToProjectedVector3xz().normalized * demo_WASDspeed;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    v = current + cam.transform.forward.ToProjectedVector3xz().normalized * -demo_WASDspeed;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    v = current + cam.transform.right.ToProjectedVector3xz().normalized * -demo_WASDspeed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    v = current + cam.transform.right.ToProjectedVector3xz().normalized * demo_WASDspeed;
                }
            }

            if (!v.Equals(current)) {
				currentLocation = Coordinates.convertVectorToCoordinates (v);
				if (onLocationChanged != null) {
					onLocationChanged.Invoke (currentLocation);
				}
			}
			CheckMotionState (currentLocation);

		}

		#endregion



		#region DEMO LOCATIONS

		public void LoadDemoLocation () {

            Coordinates location = LocationEnums.GetCoordinates(demoLocation);
            if (location != null) {
                worldOrigin = currentLocation = location;
                SetOrigin(worldOrigin);
            } else if (demoLocation == DemoLocation.Custom) {
                currentLocation = worldOrigin;
                SetOrigin(worldOrigin);
            } 
            else {
                currentLocation = worldOrigin = null;
            }
		}

		#endregion

	}


}