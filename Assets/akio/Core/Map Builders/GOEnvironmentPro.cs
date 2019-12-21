using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {

	public class GOEnvironmentPro : MonoBehaviour {

		public GOMap goMap;
		public GOEnvironmentKind[] floatingEnvironment;
		public GOEnvironmentKind[] featureEnvironment;
		//public GOEnvironmentKind[] buildings;

		// Use this for initialization
		void Awake () {
			
			goMap.OnTileLoad.AddListener((GOTile) => {OnTileLoad(GOTile);});
			foreach (GOLayer layer in goMap.layers) {
				layer.OnFeatureLoad.AddListener((GOFeature,GameObject) => {OnFeatureLoad(GOFeature,GameObject);});
			}
		}

		public void OnTileLoad (GOTile tile) {

			foreach (GOEnvironmentKind kind in floatingEnvironment) {

				if (Application.isPlaying)
					StartCoroutine (SpawnPrefabsInTile (tile, tile.gameObject, kind));
				else
					GORoutine.start (SpawnPrefabsInTile (tile, tile.gameObject, kind), this);
			}
		}

		public void OnFeatureLoad (GOFeature feature, GameObject featureObject) {


             if (goMap.useElevation == true){
				foreach (GOEnvironmentKind kind in featureEnvironment) {

					bool kindCondition = kind.kind == feature.kind;
					bool layerCondition = kind.layer == feature.layer.layerType && kind.kind == GOFeatureKind.baseKind;

					if (kindCondition || layerCondition) {
						if (Application.isPlaying)
							StartCoroutine (SpawnPrefabsIn3DMesh (feature, featureObject, kind));
						else
							GORoutine.start (SpawnPrefabsIn3DMesh (feature, featureObject, kind), this);
					}
				}
			}
			else {
				foreach (GOEnvironmentKind kind in featureEnvironment) {

                    bool kindCondition = kind.kind == feature.kind || kind.kind == GOFeatureKind.baseKind;
					bool layerCondition = kind.layer == feature.layer.layerType;

   					if (kindCondition && layerCondition) {
           
                        Debug.Log(kind.layer + " " + feature.layer.layerType);

						if (Application.isPlaying)
							StartCoroutine (SpawnPrefabsInMesh (feature, featureObject, kind));
						else
							GORoutine.start (SpawnPrefabsInMesh (feature, featureObject, kind), this);
					}
				}
			}
		}


		public IEnumerator SpawnPrefabsInTile (GOTile tile, GameObject parent, GOEnvironmentKind kind) {

			if (tile == null)
				yield break;

			float rate = Mathf.FloorToInt(tile.goTile.diagonalLenght * (kind.density/100));


			for (int i = 0 ; i<=rate; i++) {
				
				float randomX = UnityEngine.Random.Range (tile.goTile.getXRange().x, tile.goTile.getXRange().y);
				float randomZ = UnityEngine.Random.Range (tile.goTile.getZRange().x, tile.goTile.getZRange().y);
				float randomY = UnityEngine.Random.Range (200, 320);

				int n = UnityEngine.Random.Range (0, kind.prefabs.Length);
				Vector3 pos = new Vector3 (randomX,randomY,randomZ);

				pos.y += kind.prefabs [n].transform.position.y;

				pos.y += GOMap.AltitudeForPoint (pos);

				var rotation = kind.prefabs [n].transform.eulerAngles;
				var randomRotation =  new Vector3( 0 , UnityEngine.Random.Range(0, 360) , 0);

				GameObject obj =  (GameObject)GameObject.Instantiate (kind.prefabs[n], pos, Quaternion.Euler(rotation+randomRotation));
				obj.transform.parent = parent.transform;
				obj.transform.position = pos;
				if (Application.isPlaying)
					yield return null;
			}

			yield return null;
		}

		public IEnumerator SpawnPrefabsIn3DMesh (GOFeature feature, GameObject parent, GOEnvironmentKind kind) {

			if (feature.preloadedMeshData == null)
				yield break;

			int rate = 100 / kind.density;

			foreach (Vector3 vertex in feature.preloadedMeshData.vertices) {

				try {
					int spawn = UnityEngine.Random.Range (0, rate);
					if (spawn != 0)
						continue;

					int n = UnityEngine.Random.Range (0, kind.prefabs.Length);
					Vector3 pos = vertex;

					if(GOMap.IsPointAboveWater(pos))
						continue;

					pos.y += kind.prefabs [n].transform.position.y;


					var rotation = kind.prefabs [n].transform.eulerAngles;
					var randomRotation =  new Vector3( 0 , UnityEngine.Random.Range(0, 360) , 0);

					GameObject obj =  (GameObject)GameObject.Instantiate (kind.prefabs[n], pos, Quaternion.Euler(rotation+randomRotation));
					obj.transform.parent = parent.transform;

				} catch {
				}

				if (Application.isPlaying)
					yield return null;
			}

			yield return null;
		}

		public IEnumerator SpawnPrefabsInMesh (GOFeature feature, GameObject parent, GOEnvironmentKind kind) {

			if (feature.preloadedMeshData == null)
				yield break;
			
			float area = Area (feature.convertedGeometry);
			float rate = kind.density / 1000.0f;
			int k = Mathf.FloorToInt(area * rate / 100);

			//Debug.Log (area + " " + rate + " " + k);

			for (int i = 0; i < k; i++) {

				try {
//					int spawn = UnityEngine.Random.Range (0, rate);
//					if (spawn != 0)
//						continue;

					int n = UnityEngine.Random.Range (0, kind.prefabs.Length);
					Vector3 pos = randomPointInShape(feature.convertedGeometry);

					if(GOMap.IsPointAboveWater(pos))
						continue;

					pos.y += kind.prefabs [n].transform.position.y;


					var rotation = kind.prefabs [n].transform.eulerAngles;
					var randomRotation =  new Vector3( 0 , UnityEngine.Random.Range(0, 360) , 0);

					GameObject obj =  (GameObject)GameObject.Instantiate (kind.prefabs[n], pos, Quaternion.Euler(rotation+randomRotation));
					obj.transform.parent = parent.transform;

				} catch {
				}

				if (Application.isPlaying)
					yield return null;
			}

			yield return null;
		}



//		public IEnumerator SpawnBuildings (GOFeature feature, GameObject parent, GOEnvironmentKind kind) {

//			if (feature.preloadedMeshData == null)
//				yield break;


//			int rate = 100 / (kind.density+1);

////			foreach (Vector3 vertex in feature.preloadedMeshData.vertices) {
////
////				try {
//					int spawn = UnityEngine.Random.Range (0, rate);
//					if (spawn != 0)
//						yield break;

//					int n = UnityEngine.Random.Range (0, kind.prefabs.Length);

//					Vector3 pos = feature.featureCenter;

//					pos.y += kind.prefabs [n].transform.position.y;

//					var rotation = kind.prefabs [n].transform.eulerAngles;
//					var randomRotation =  new Vector3( 0 , UnityEngine.Random.Range(0, 360) , 0);

//					GameObject obj =  (GameObject)GameObject.Instantiate (kind.prefabs[n], pos, Quaternion.Euler(rotation+randomRotation));
//					obj.transform.parent = parent.transform;

////				} catch {
////				}

//				if (Application.isPlaying)
//					yield return null;
////			}

		//	yield return null;
		//}


		#region Utils

		public Vector3 randomPointInShape(List<Vector3> shape) {

			int random = UnityEngine.Random.Range (0, shape.Count);
			int random2 = UnityEngine.Random.Range (0, shape.Count);

			Vector3 vert = (Vector3)shape [random];
			Vector3 vert2 = (Vector3)shape [random2];

			return Vector3.Lerp (vert, vert2, Random.value);

		}

		public float Area (List<Vector3> shape) {
			Vector3 result = Vector3.zero;
			for(int p = shape.Count-1, q = 0; q < shape.Count; p = q++) {
				result += Vector3.Cross(shape[q], shape[p]);
			}
			result *= 0.5f;
			return result.magnitude;
		}

		#endregion

	}

	[ExecuteInEditMode]
	[System.Serializable]
	public class GOEnvironmentKind {

		public GoMap.GOLayer.GOLayerType layer;
		public GOFeatureKind kind;
		public GameObject[] prefabs;
		[Range(0, 100)] public int density;

	}
}