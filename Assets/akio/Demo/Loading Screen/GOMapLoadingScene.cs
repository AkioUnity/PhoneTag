using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoShared;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GoMap
{
    public class GOMapLoadingScene : MonoBehaviour
    {
        [Header("UI")]
        public Slider slider;
        public Text sliderLabel;
        public string nextSceneName;

        [Header("Settings")]
        public BaseLocationManager locationManager;
        [Range(0, 8)] public int tileBuffer = 2;

        [Header("Cache")]
        public bool useCache = true;
        [InspectorButton("ClearCache")] public bool clearCache;
        [InspectorButton("OpenCacheFolder")] public bool openCache;

        [Header("Map APIs")]
        [Regex(@"^(?!\s*$).+", "Please insert your MapBox Access Token")]
        public string mapbox_accessToken = "";
        [Regex(@"^(?!\s*$).+", "Please insert your OSM API key")] public string osm_api_key = "";
        [Regex(@"^(?!\s*$).+", "Please insert your Mapzen API key")] public string mapzen_legacy_api_key = "";

        public GOMapType mapType = GOMapType.Mapbox;
        public enum GOMapType
        {
            Mapbox,
            OSM,
            Esri,
            Mapzen_Legacy,
        }

        public bool useElevation = false;
        public bool useSatelliteBackground = false;
        public bool satellite4X = false;
        public GONavMeshSettings navmeshSettings;

        #region Start

        // Use this for initialization
        void Start()
        {
            slider.value = 0;
            sliderLabel.text = "Waiting for GPS position...";

            //Register to the location manager event. This will be fired once a valid GPS position is acquired. 
            locationManager.onOriginSet.AddListener((Coordinates) => { OnOriginSet(Coordinates); });
        }

        void OnOriginSet(Coordinates worldOrigin)
        {
            StartCoroutine(downloadMapData(worldOrigin));
        }
        #endregion

        #region Download Map Data

        IEnumerator downloadMapData(Coordinates worldOrigin)
        {

            //Get the list of tile coordinates to download. 
            List<Vector2> adiacentTiles = worldOrigin.adiacentNTiles(locationManager.zoomLevel, tileBuffer);

            //Update the max slider value
            slider.maxValue = adiacentTiles.Count;
            sliderLabel.text = "Downloading map data...";

            //Create a GOTile obejct for each tile coordinates pair
            foreach (Vector2 tileCoords in adiacentTiles)
            {

                GOTile tile = createSmartTileObject(tileCoords, locationManager.zoomLevel);
                tile.PrepareGoTile();

                //Download tile data
                yield return StartCoroutine(tile.goTile.downloadData(this));

                slider.value++;
                Debug.Log(string.Format("[GOMap Loading Scene] Tiles Downloaded {0}/{1}", slider.value, slider.maxValue));

                Object.Destroy(tile.gameObject);
            }

            Debug.Log(string.Format("[GOMap Loading Scene] All tiles Downloaded, load scene: {0}", nextSceneName));

            //Load new scene, the data is now stored in the GOMap cache folder and will be automatically reused by the next scene.
            //That means no API waiting time in the game scene.
            if (!string.IsNullOrEmpty(nextSceneName)) {
                SceneManager.LoadScene(nextSceneName);
            }

            yield return null;
        }

        GOTile createSmartTileObject(Vector2 tileCoords, int Zoom)
        {

            GOTileObj goTile = new GOTileObj(tileCoords, locationManager.zoomLevel, (GoMap.GOMap.GOMapType)mapType, useElevation, 1, new Vector2(50, 50), useCache, false, locationManager.worldScale, useSatelliteBackground, satellite4X, false, navmeshSettings);
            GameObject tileObj = new GameObject(goTile.name);

            tileObj.transform.parent = gameObject.transform;
            GOTile tile;

            if (mapType == GOMapType.Mapbox)
            {
                tile = tileObj.AddComponent<GOMapboxTile>();
                goTile.apiKey = mapbox_accessToken;
            }
            else if (mapType == GOMapType.Mapzen_Legacy)
            {
                tile = tileObj.AddComponent<GOMapzenProtoTile>();
                goTile.apiKey = mapzen_legacy_api_key;
            }
            else if (mapType == GOMapType.OSM)
            {
                tile = tileObj.AddComponent<GOOSMTile>();
                goTile.apiKey = osm_api_key;
            }
            else if (mapType == GOMapType.Esri)
            {
                tile = tileObj.AddComponent<GOEsriTIle>();
            }
            else
            {
                tile = tileObj.AddComponent<GOTile>();
            }

            goTile.position = goTile.tileCenter.convertCoordinateToVector();
            tile.gameObject.transform.position = goTile.position;

            tile.goTile = goTile;

            return tile;
        }

        #endregion

        #region CacheControl

        public void ClearCache()
        {
            FileHandler.ClearEverythingInFolder(FileHandler.GoCachePath());
        }


        public void OpenCacheFolder()
        {
            #if UNITY_EDITOR
            EditorUtility.RevealInFinder(FileHandler.GoCachePath());
            #endif
        }
        #endregion
    }
}