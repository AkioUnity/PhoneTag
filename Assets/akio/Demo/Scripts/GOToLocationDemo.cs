using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using GoMap;
using System.Collections.Generic;

namespace GoShared {

	public class GOToLocationDemo : MonoBehaviour {

		public InputField inputField;
		public Button button;
		public GOMap goMap;
		public GameObject addressMenu;

		GameObject addressTemplate;

		public void Start () {
		
			addressTemplate = addressMenu.transform.Find ("Address template").gameObject;

			inputField.onEndEdit.AddListener(delegate(string text) {
				GoToAddress();
			});
		}


		public void GoToAddress() {

			if (inputField.text.Any (char.IsLetter)) { //Text contains letters
				SearchAddress();
			} else if (inputField.text.Contains(",")){

				string s = inputField.text;
				Coordinates coords = new Coordinates (inputField.text);

                LocationManager locationManager = (LocationManager)goMap.locationManager;
				locationManager.SetLocation (coords);
				Debug.Log ("NewCoords: " + coords.latitude +" "+coords.longitude);
			}
		}

		public void SearchAddress () {
		
			addressMenu.SetActive (false);
			string completeUrl;

			if (goMap.mapType == GOMap.GOMapType.Mapzen_Legacy) {

				string baseUrl = "https://search.mapzen.com/v1/search?";
				string apiKey = goMap.mapzen_legacy_api_key;
				string text = inputField.text;
				completeUrl = baseUrl + "&text=" + WWW.EscapeURL (text) + "&api_key=" + apiKey;
			} else {
				string baseUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/";
				string apiKey = goMap.mapbox_accessToken;
				string text = inputField.text;
				completeUrl = baseUrl + WWW.EscapeURL (text) +".json" + "?access_token=" + apiKey;

                if (goMap.locationManager.currentLocation != null) {
					completeUrl += "&proximity=" + goMap.locationManager.currentLocation.longitude + "%2C" + goMap.locationManager.currentLocation.latitude;
                } else if (goMap.locationManager.worldOrigin != null){
                    completeUrl += "&proximity=" + goMap.locationManager.worldOrigin.longitude + "%2C" + goMap.locationManager.worldOrigin.latitude;
				}
			}
			Debug.Log (completeUrl);

			IEnumerator request = GOUrlRequest.jsonRequest (this, completeUrl, false, null, (Dictionary<string,object> response, string error) => {

				if (string.IsNullOrEmpty(error)){
					IList features = (IList)response["features"];
					LoadChoices(features);
				}

			});

			StartCoroutine (request);
		}

		public void LoadChoices(IList features) {

			while (addressMenu.transform.childCount > 1) {
				foreach (Transform child in addressMenu.transform) {
					if (!child.gameObject.Equals (addressTemplate)) {
						DestroyImmediate (child.gameObject);
					}
				}
			}


			for (int i = 0; i<Math.Min(features.Count,5); i++) {

				IDictionary feature = (IDictionary) features [i];

				IDictionary geometry = (IDictionary)feature["geometry"];
				IList coordinates = (IList)geometry["coordinates"];
				GOLocation location = new GOLocation ();
				IDictionary properties = (IDictionary)feature ["properties"];
				Coordinates coords = new Coordinates (Convert.ToDouble (coordinates [1]), Convert.ToDouble (coordinates [0]), 0);

				if (goMap.mapType == GOMap.GOMapType.Mapzen_Legacy) {
					location.addressString = (string)properties ["label"];
				} else {
					location.addressString = (string)feature["matching_place_name"] ?? (string)feature["place_name"];
                }
				location.coordinates = coords;
				location.properties = properties;

				GameObject cell = Instantiate (addressTemplate);
				cell.transform.SetParent(addressMenu.transform);
				cell.transform.GetComponentInChildren<Text> ().text = location.addressString;
				cell.name = location.addressString;
				cell.SetActive (true);

				Button btn = cell.GetComponent<Button> ();
				btn.onClick.AddListener(() => { LoadLocation(location); }); 
			
			}


			addressMenu.SetActive (true);


		}

		public void LoadLocation (GOLocation location) {

			inputField.text = location.addressString;
			addressMenu.SetActive (false);
            LocationManager locationManager = (LocationManager)goMap.locationManager;
            locationManager.SetLocation(location.coordinates);
		}

	}

	[System.Serializable]
	public class GOLocation {

		public Coordinates coordinates;
		public IDictionary properties;
		public string addressString;

	}
}
