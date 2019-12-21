using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoShared {

	public class GOPreloadedMap : MonoBehaviour {

		public LocationManager locationManager;
		public Coordinates centerCoordinates;

		public void Start ()
		{
			locationManager.onOriginSet.AddListener ((Coordinates) => {
				RepositionMap (Coordinates);
			});
		}

		void RepositionMap (Coordinates currentLocation) {//This is called when the origin is set

			transform.localPosition = centerCoordinates.tileCenter(locationManager.zoomLevel).convertCoordinateToVector();
		}

	}
}
