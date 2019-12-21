using System.Collections;
using System.Collections.Generic;
using LocationManagerEnums;
using UnityEngine;

namespace GoShared
{

    public class BaseLocationManager : MonoBehaviour
    {
        [Header("Main Settings")]
        public int zoomLevel = 16;
        public float worldScale = 1;
        public float desiredAccuracy = 70;

        public Coordinates currentLocation = null;
        public Coordinates worldOrigin = null;

        public static bool IsOriginSet;

        [Header("Location Events")]
        public GOLocationEvent onOriginSet;
        public GOLocationEvent onLocationChanged;
        public GOMotionStateEvent OnMotionStateChanged;

        [HideInInspector] public List<Coordinates> lastLocations = new List<Coordinates>();
        [HideInInspector] public MotionState currentMotionState = MotionState.Idle;
        [HideInInspector] float currentSpeed = 0;


        #region Origin

        public IEnumerator WaitForOriginSet()
        {
            while (!IsOriginSet)
            {
                yield return null;
            }
        }

        public void SetOrigin(Coordinates coords)
        {
            Debug.Log("[Location Manager] set origin " + coords.toLatLongString());
            IsOriginSet = true;
            worldOrigin = coords.tileCenter(zoomLevel);
            Coordinates.setWorldOrigin(worldOrigin, worldScale);
            if (onOriginSet != null)
            {
                onOriginSet.Invoke(worldOrigin);
            }
        }
        #endregion


        #region MOTION STATE

        public void CheckMotionState(Coordinates lastLocation)
        {

            MotionState state = currentMotionState;

            if (lastLocations.Count > 0 && lastLocation.Equals(lastLocations[lastLocations.Count - 1]))
            {
                state = MotionState.Idle;
            }
            else
            {

                lastLocations.Add(lastLocation);
                int max = 10;
                if (lastLocations.Count == max + 1)
                {
                    lastLocations.RemoveAt(0);
                }

                //Speed is returned in m/s
                currentSpeed = GPSSpeedUtils.GetSpeedFromCoordinatesList(lastLocations);
                if (currentSpeed < 0.5f)
                {
                    state = MotionState.Idle;
                }
                else if (currentSpeed < 3f)
                {
                    state = MotionState.Walk;
                }
                else
                {
                    state = MotionState.Run;
                }
            }

            if (state != currentMotionState)
            {
                currentMotionState = state;
                if (OnMotionStateChanged != null)
                {
                    OnMotionStateChanged.Invoke(currentMotionState);
                }
            }

        }

        #endregion


    }
}