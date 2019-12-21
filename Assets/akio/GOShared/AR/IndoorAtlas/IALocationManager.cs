using System.Collections;
using System.Collections.Generic;
using IndoorAtlas;
using UnityEngine;
using LocationManagerEnums;
using System;
using UnityEngine.Events;

namespace GoShared
{
    public class IALocationManager : BaseLocationManager
    {
        [Header("Use this class with the Indoor Atlas SDK")]
        [Space (30)]

        [Header("Editor settings")]
        public DemoLocation inEditorLocation;

        [Header("Indoor Atlas Events")]
        public IAStatusEvent IAstatusEvent;
        public IALocationEvent IAlocationEvent;
        public IARegionEvent IAenterRegionEvent;
        public IARegionEvent IAexitRegionEvent;
        public IAOrientationEvent IAorientationEvent;
        public IAHeadingEvent IAheadingEvent;

        IEnumerator Start()
        {

            if (Application.isEditor)
            {
                yield return new WaitForSeconds(1);
                Coordinates location = LocationEnums.GetCoordinates(inEditorLocation);
                SetOrigin(location);
            }

            yield return null;
        }


        #region Indoor Atlas Listeners

        new void onLocationChanged(string data)
        {
            Location location = JsonUtility.FromJson<Location>(data);
            Debug.Log("[IndoorAtlasLocationManager] onLocationChanged " + location.latitude + ", " + location.longitude);

            currentLocation = new Coordinates(location.latitude, location.longitude, location.altitude);
            currentLocation.timestampLastUpdate = location.timestamp;

            if (!IsOriginSet)
            {
                SetOrigin(currentLocation);
            }
            else
            {
                SetNewLocation(currentLocation);
            }
            CheckMotionState(currentLocation);

            //Indoor atlas events
            if (IAlocationEvent != null) {  IAlocationEvent.Invoke(location); }
        }

        void onStatusChanged(string data)
        {
            Status serviceStatus = JsonUtility.FromJson<Status>(data);
            Debug.Log("[IndoorAtlasLocationManager] onStatusChanged " + serviceStatus.status);

            //Indoor atlas events
            if (IAstatusEvent != null) { IAstatusEvent.Invoke(serviceStatus); }
        }

        void onHeadingChanged(string data)
        {
            Heading heading = JsonUtility.FromJson<Heading>(data);
            //Indoor atlas events
            if (IAheadingEvent != null) { IAheadingEvent.Invoke(heading); }  
        }

        void onOrientationChange(string data)
        {
            Quaternion orientation = JsonUtility.FromJson<Orientation>(data).getQuaternion();
            //Quaternion rot = Quaternion.Inverse(new Quaternion(orientation.x, orientation.y, -orientation.z, orientation.w));
            //Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)) * rot;

            //Indoor atlas events
            if (IAorientationEvent != null) { IAorientationEvent.Invoke(orientation);}
        }

        void onEnterRegion(string data)
        {
            Region region = JsonUtility.FromJson<Region>(data);
            Debug.Log("[IndoorAtlasLocationManager] onEnterRegion " + region.name + ", " + region.type + ", " + region.id + " at " + region.timestamp);
            //Indoor atlas events
            if (IAenterRegionEvent != null) { IAenterRegionEvent.Invoke(region); }
        }

        void onExitRegion(string data)
        {
            Region region = JsonUtility.FromJson<Region>(data);
            Debug.Log("[IndoorAtlasLocationManager] onExitRegion " + region.name + ", " + region.type + ", " + region.id + " at " + region.timestamp);
            //Indoor atlas events
            if (IAexitRegionEvent != null) { IAexitRegionEvent.Invoke(region); }
        }

        #endregion

        #region Events

        void SetOrigin(Coordinates coords)
        {
            Debug.Log("[IndoorAtlasLocationManager] set origin " + coords.toLatLongString());
            IsOriginSet = true;
            Coordinates.setWorldOrigin(coords, worldScale);
            if (base.onOriginSet != null)
            {
                base.onOriginSet.Invoke(coords);
            }
        }

        void SetNewLocation(Coordinates coords)
        {
            if (base.onLocationChanged != null)
            {
                base.onLocationChanged.Invoke(coords);
            }
        }

        #endregion
    }

    [Serializable] public class IARegionEvent : UnityEvent<Region> {}
    [Serializable] public class IAHeadingEvent : UnityEvent<Heading> { }
    [Serializable] public class IAOrientationEvent : UnityEvent<Quaternion> { }
    [Serializable] public class IAStatusEvent : UnityEvent<Status> { }
    [Serializable] public class IALocationEvent : UnityEvent<Location> { }

}
