using GoShared;
using UnityEngine.Serialization;
using UnityEngine;


namespace GoMap {

	[System.Serializable]
	public class GOLayer
	{

		public string name {
			get {
				return layerType.ToString ();
			}
			set {
				this.name = value;
			}
		}

		public GOLayerType layerType;
		public enum GOLayerType  {
			Buildings,
			Landuse,
			Water,
			Earth,
			Roads,
			Pois
		}

		public bool isPolygon;
        [ConditionalHide("isPolygon")] public GOUVMappingStyle uvMappingStyle = GOUVMappingStyle.TopAndSidesRepeated;
        [ConditionalHide("layerType", "Buildings")] public bool useRealHeight = false;
        [ConditionalHide("layerType", "Buildings")] public bool forceMinHeight = false;
        [ConditionalHide("isPolygon")]  public bool slicedExtrusion = true;
        public GORenderingOptions defaultRendering;
		public GORenderingOptions [] renderingOptions;

		public GOFeatureKind[] useOnly;
		public GOFeatureKind[] avoid;
		[ConditionalHide("layerType", "Roads")] public bool useTunnels = true;
		[ConditionalHide("layerType", "Roads")]public bool useBridges = true;
		public bool useColliders = false;
        public int colliderHeight;
		[Layer] public int unityLayer = 0;
		public UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.Off;

		public bool startInactive;
		public bool disabled = false;

		public GOFeatureEvent OnFeatureLoad; 


		public string json () {  //Mapzen

			return layerType.ToString ().ToLower ();
		}

		public string lyr () { //Mapbox
			switch (layerType) {
			case GOLayerType.Buildings:
				return "building";
			case GOLayerType.Landuse:
				return "landuse";
			case GOLayerType.Water:
				return "water";
			case GOLayerType.Earth:
				return "earth";
			case GOLayerType.Roads:
				return "road";
			case GOLayerType.Pois:
				return "poi_label";
			default:
				return "";
			}		
		}

		public string lyr_osm () { //OSM
			switch (layerType) {
			case GOLayerType.Buildings:
				return "building";
			case GOLayerType.Landuse:
				return "landcover";
			case GOLayerType.Water:
				return "water";
			case GOLayerType.Earth:
				return "landcover";
			case GOLayerType.Roads:
				return "transportation";
			case GOLayerType.Pois:
				return "poi";
			default:
				return "";
			}		
		}

		public string lyr_esri () { //Esri
			switch (layerType) {
			case GOLayerType.Buildings:
				return "Building";
			case GOLayerType.Landuse:
				return "Park or farming,Education,Cemetery,Medical,Landmark";
			case GOLayerType.Water:
				return "Water area,Marine area";
			case GOLayerType.Earth:
				return "Land";
			case GOLayerType.Roads:
				return "Road,Road tunnel,Railroad,Transportation";
			default:
				return "";
			}		
		}

		public float defaultLayerY() {
			return defaultLayerY (layerType);
		}

		public static float defaultLayerY(GOLayerType t) {
			switch (t) {
			case GOLayerType.Buildings:
				return 0;
			case GOLayerType.Landuse:
				return 0.3f;
			case GOLayerType.Water:
				return 0.2f;
			case GOLayerType.Earth:
				return 0.1f;
			case GOLayerType.Roads:
				return 0.4f;
			default:
				return 0;
			}		
		}

        public static int LayerTypeToIndex(GOLayerType type) {
            switch (type) {
                case GOLayerType.Water :
                    return 0;
                case GOLayerType.Earth:
                    return 1;
                case GOLayerType.Landuse:
                    return 2;
                case GOLayerType.Buildings:
                    return 3;
                case GOLayerType.Roads:
                    return 4;
                default :return 100;
            }
        }

	}


}