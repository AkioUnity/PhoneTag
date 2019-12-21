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
    public class GOMapLoadingScreen : MonoBehaviour
    {
        public GOMap goMap;

        [Header("UI")]
        public RectTransform panel;
        public Slider slider;
        public Text sliderLabel;

        #region Start

        // Use this for initialization
        void Start()
        {
            slider.value = 0;
            sliderLabel.text = "Waiting for GPS position...";

            //Register to the location manager event. This will be fired once a valid GPS position is acquired. 
            goMap.locationManager.onOriginSet.AddListener((Coordinates) => { OnOriginSet(Coordinates); });

            //Register to the GO Map OnTileLoad Event. This will be fired everytime a tile is loaded 
            goMap.OnTileLoad.AddListener((GOTile) => { OnTileLoad(GOTile); });

        }

        #endregion

        #region Location Manager Event

        void OnOriginSet(Coordinates worldOrigin)
        {
            int buffer = goMap.tileBuffer;

            if (buffer > 1)
                buffer = 1;
            
            //Set the slider max value equal the number of GO Map tile buffer count.
            slider.maxValue = Coordinates.tileBufferCount(buffer);
            sliderLabel.text = "Loading the map...";
        }
        #endregion

        #region GO Map Event

        void OnTileLoad (GOTile goTile) {

            slider.value++;   
            Debug.Log(string.Format("[GOMap Loading Screen] Tiles Loaded {0}/{1}", slider.value, slider.maxValue));

            if (slider.value.Equals(slider.maxValue)) {

                StartCoroutine(slideOutLoadingScreen());
                Object.Destroy(gameObject, 1);

            }
        }

        IEnumerator slideOutLoadingScreen () {

            float time = 0.5f;
            float elapsedTime = 0;
            Vector2 finalPosition = new Vector2(panel.anchoredPosition.x,-Screen.height-100);


            while (elapsedTime <= time)
            {
                panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, finalPosition, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }

        #endregion

    }
}