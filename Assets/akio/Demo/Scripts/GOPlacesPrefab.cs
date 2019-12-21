using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoMap {

	public class GOPlacesPrefab : MonoBehaviour {

		public IDictionary placeInfo; 
		public SpriteRenderer spriteRenderer;
		public GOPlaces goPlaces;

		Sprite texture;

		void Start () {

			spriteRenderer = GetComponentInChildren<SpriteRenderer> ();
			spriteRenderer.sprite = null;
		
			string url = (string)placeInfo["icon"];
			StartCoroutine (getTextureWithUrl (url));

		}


		private IEnumerator DownloadIcon (string url) {

			WWW www = new WWW(url);
			yield return www;

			texture = Sprite.Create (www.texture, new Rect (0, 0, 71, 71), new Vector2 (0.5f, 0.5f));
		}

		public IEnumerator getTextureWithUrl (string url) {

			if (goPlaces.iconsCache.Contains(url)) {
				texture = (Sprite)goPlaces.iconsCache [url];
				spriteRenderer.sprite = texture;
				yield break;
			}

			yield return StartCoroutine (DownloadIcon (url));
			spriteRenderer.sprite = texture;
			if (!goPlaces.iconsCache.Contains (url)) {
				goPlaces.iconsCache.Add (url, texture);
			}
			yield return null;
		}

	}
}