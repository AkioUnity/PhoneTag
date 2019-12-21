using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {
	
	[System.Serializable]
	public class GOPOILayer {

		public string name {
			get {
				return "Pois";
			}
			set {
				
			}
		}
        [Layer] public int unityLayer;
		public GOPOIRendering [] renderingOptions;
		public bool startInactive;
		public bool disabled = false;

		public string json () {  //Mapzen

			return "pois";
		}

		public string lyr () { //Mapbox
			return "poi_label";
		}

		public string lyr_osm () { //OSM
			return "poi";
		}

		public string lyr_esri () { //Esri
			return "";	
		}

		public GOPOIRendering GetRenderingForPoiKind (GOPOIKind kind) {

			foreach (GOPOIRendering r in renderingOptions) {
				if (r.kind == kind)
					return  r;
			}
			return null;
		}
	}

	[System.Serializable]
	public class GOPOIRendering {

		public GOPOIKind kind;
		public GameObject prefab;
		public string tag;
		public GOFeatureEvent OnPoiLoad; 

	}
}
