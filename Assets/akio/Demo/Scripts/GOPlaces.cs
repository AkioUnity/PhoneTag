using UnityEngine;
using System.Collections;

//This class uses Google Places webservice API. 
//It's made for demo purpose only, and needs your personal Google Developer API Key. 
//(No credit card is required, visit https://developers.google.com/places/web-service/intro)

using GoShared;
using System.Linq;
using MiniJSON;
using System.Collections.Generic;


namespace GoMap
{

	public class GOPlaces : MonoBehaviour {

		public GOMap goMap;
		public string googleAPIkey;
		public string type;
		public GameObject prefab;
		public bool addGOPlaceComponent = false;


		string nearbySearchUrl = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?";
		[HideInInspector] public IDictionary iconsCache = new Dictionary <string,Sprite>();

		// Use this for initialization
		void Awake () {

			if (googleAPIkey.Length == 0) {
				Debug.LogWarning ("GOPlaces - GOOGLE API KEY IS REQUIRED, GET iT HERE: https://developers.google.com/places/web-service/intro");
				return;
			}

			//register to the GOMap event OnTileLoad
			goMap.OnTileLoad.AddListener ((GOTile) => {
				OnLoadTile (GOTile);
			});

		}

		void OnLoadTile (GOTile tile) {
			StartCoroutine (NearbySearch(tile));
		}

		IEnumerator NearbySearch (GOTile tile) {
		
			//Center of the map tile
			Coordinates tileCenter = tile.goTile.tileCenter;

			//radius of the request, equals the tile diagonal /2
			float radius = tile.goTile.diagonalLenght / 2;

			//The complete nearby search url, api key is added at the end
			string url = nearbySearchUrl + "location="+tile.goTile.tileCenter.latitude+","+tile.goTile.tileCenter.longitude+"&radius="+radius+"&type="+type+"&key="+googleAPIkey;

			//Perform the request
			var www = new WWW(url);
			yield return www;

			//Check for errors
			if (string.IsNullOrEmpty (www.error)) {

				string response = www.text;
				//Deserialize the json response
				IDictionary deserializedResponse = (IDictionary)Json.Deserialize (response);

				Debug.Log(string.Format("[GO Places] Tile center: {0} - Request Url {1} - response {2}",tileCenter.toLatLongString(),url,response));

				//That's our list of Places
				IList results = (IList)deserializedResponse ["results"];

				//Create a container for the places and set it as a tile child. In this way when the tile is destroyed it will take also the places with it.
				GameObject placesContainer = new GameObject ("Places");
				placesContainer.transform.SetParent (tile.transform);

				foreach (IDictionary result in results) {

					string placeID = (string)result["place_id"];
					string name = (string)result["name"];

					IDictionary location = (IDictionary)((IDictionary)result ["geometry"])["location"];
					double lat = (double)location ["lat"];
					double lng = (double)location ["lng"];

					//Create a new coordinate object, with the desired lat lon
					Coordinates coordinates = new Coordinates (lat, lng,0);

					if (!TileFilter (tile, coordinates))
						continue;

					//Instantiate your game object
					GameObject place = GameObject.Instantiate (prefab);
					place.SetActive (true);
					//Convert coordinates to position
					Vector3 position = coordinates.convertCoordinateToVector(place.transform.position.y);

					if (goMap.useElevation)
						position = GOMap.AltitudeToPoint (position);

					//Set the position to object
					place.transform.localPosition = position;
					//the parent
					place.transform.SetParent (placesContainer.transform);
					//and the name
					place.name = (name != null && name.Length>0)? name:placeID;

					if (addGOPlaceComponent) {
						GOPlacesPrefab component = place.AddComponent<GOPlacesPrefab> ();
						component.placeInfo = result;
						component.goPlaces = this;
					}
				}
			}
		}

		bool TileFilter (GOTile tile, Coordinates coordinates) {
		
			Vector2 tileCoordinates = coordinates.tileCoordinates (goMap.zoomLevel);

			if (tile.goTile.tileCoordinates.Equals (tileCoordinates))
				return true;

//			Debug.LogWarning ("Coordinates outside the tile");
			return false;
		
		}
	}
}
