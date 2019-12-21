using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {
	
	[System.Serializable]
	public class GOLabelsLayer {

		public enum GOLabelsLanguage {

			International,
			English,
			Spanish,
			French,
			German,
			Russian,
			Chinese,
			Chinese_simplified,
			Portuguese,
			Arabic
		}

		public string name {
			get {
				return "Labels";
			}
			set {
				
			}
		}
        [Layer] public int unityLayer;

		public string tag;
		public GOStreetnamesSettings streetNames;

		[Header("This setting is applied when possible")]
		public GOLabelsLanguage preferredLanguage = GOLabelsLanguage.International;

		public bool startInactive;
		public bool disabled = true;

		public GOFeatureEvent OnLabelLoad; 

		public string json () {  //Mapzen

			return "";
		}

		public string lyr () { //Mapbox
			return "road_label";
		}

		public string lyr_osm () { //OSM
			return "transportation_name";
		}

		public string lyr_esri () { //Esri
			return "Road/label,Railroad/label,Road tunnel/label,Water area/label,Park or farming/label,Building/label";	
//			return "Road/label,Railroad/label,Road tunnel/label";	
		}
			
		public float defaultLayerY() {
			return 1f;
		}

		public string LanguageKey(GOMap.GOMapType mapType) {
		
			if (mapType == GOMap.GOMapType.Mapbox) {
			
				switch (preferredLanguage) {

				case GOLabelsLanguage.International: 
					return "name";
				case GOLabelsLanguage.English: 
					return "name_en";
				case GOLabelsLanguage.Spanish: 
					return "name_es";
				case GOLabelsLanguage.French: 
					return "name_fr";
				case GOLabelsLanguage.German: 
					return "name_de";
				case GOLabelsLanguage.Russian: 
					return "name_ru";
				case GOLabelsLanguage.Chinese: 
					return "name_zh";
				case GOLabelsLanguage.Chinese_simplified: 
					return "name_zh-Hans";
				case GOLabelsLanguage.Portuguese: 
					return "name_pt";
				case GOLabelsLanguage.Arabic: 
					return "name_ar";

				}
			}
			else if (mapType == GOMap.GOMapType.OSM) {

				switch (preferredLanguage) {

				case GOLabelsLanguage.International: 
					return "name";
				case GOLabelsLanguage.English: 
					return "name_en";
				case GOLabelsLanguage.German: 
					return "name_de";
				default:
					return "name_int";
				}
			}
			else if (mapType == GOMap.GOMapType.Esri) {

				switch (preferredLanguage) {

				case GOLabelsLanguage.International: 
					return "name_global";
				default:
					return "name_local";
				}
			}

			return "name";
		}

	}
}
