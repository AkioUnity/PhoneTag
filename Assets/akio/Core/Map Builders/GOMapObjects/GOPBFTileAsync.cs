using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using MiniJSON;

using GoShared;
using Mapbox.Utils;
using Mapbox.VectorTile;
using Mapbox.VectorTile.ExtensionMethods;
using Mapbox.VectorTile.Geometry;
using UnityEngine.Profiling;

namespace GoMap
{
	[ExecuteInEditMode]
	public abstract class GOPBFTileAsync : GOTile
	{

		public VectorTile vt;

		//THis method is called on a background thread
		public abstract GOFeature EditFeatureData (GOFeature goFeature);
		//THis method is called on a background thread
		public abstract GOFeature EditLabelData (GOFeature goFeature);
		//THis method is called on a background thread
		public abstract string GetLayersStrings (GOLayer layer);
		//THis method is called on a background thread
		public abstract string GetPoisStrings ();
		//THis method is called on a background thread
		public abstract string GetLabelsStrings ();
		//THis method is called on a background thread
		public abstract string GetPoisKindKey ();


		public override IEnumerator BuildTile(IDictionary mapData, GOLayer layer, bool delayedLoad)
		{ 
			yield return null;
		}

		public override IEnumerator ParseTileData(GOLayer[] layers, bool delayedLoad, List <string> layerNames)
		{

			byte[] mapData = goTile.getVectorData ();

			if (mapData == null) {
				Debug.LogWarning ("Map Data is null! ASYNC");
				FileHandler.Remove (gameObject.name);
				yield break;
			}
			DateTime t0 = DateTime.Now;
			float t0s = Time.time;

			GOPbfProcedure procedure = new GOPbfProcedure();
			procedure.goTile = goTile;
			procedure.layers = layers;
			procedure.tile = this;
			procedure.onMainThread = map.runOnMainThread;
			procedure.Start();

			if (Application.isPlaying) { //Runtime build
				
				yield return StartCoroutine (procedure.WaitFor ());

				float t1s = Time.time;
				goTile.status = GOTileObj.GOTileStatus.Loaded;

				if (goTile.useElevation) {
					MeshFilter filter = GetComponent<MeshFilter> ();
					filter.sharedMesh = procedure.goTile.goMesh.ToMesh (recalculateNormals_: false);

					MeshCollider collider = GetComponent<MeshCollider> ();
					collider.sharedMesh = filter.sharedMesh;
				}
					
				foreach (GOParsedLayer p in procedure.list) {
					yield return StartCoroutine (BuildLayer (p, delayedLoad));
				}


				goTile.status = GOTileObj.GOTileStatus.Done;
                //Resources.UnloadUnusedAssets();

//				Debug.Log (string.Format("Procedure Time: {0}, Total time: {1}", (t1s-t0s), (Time.time - t0s)));
//				Debug.Log (string.Format("Terrain time: {0}, Features Time: {1}, POI Time: {2}, Total time: {3}",procedure.tT.Subtract(t0).Milliseconds, procedure.tF.Subtract(procedure.tT).Milliseconds, procedure.tP.Subtract(procedure.tF).Milliseconds, DateTime.Now.Subtract(t0).Milliseconds));


			} 
			else { //In editor build

				while(!procedure.Update())
				{
					
				}
			}

			if (Application.isPlaying) {
                Destroy(goTile.featurePrototype);
                Destroy(goTile.streetnamePrototype);
			}

			if (map.OnTileLoad != null) {
				map.OnTileLoad.Invoke(this);
			}

            base.OnTileLoaded();

			yield return null;
		}

		public IEnumerator BuildLayer(GOParsedLayer parsedLayer, bool delayedLoad)
		{
			
			GOLayer goLayer = parsedLayer.goLayer;

			Profiler.BeginSample("[GoMap] [BuildLayer] game object");
			GameObject parent = null;
			parent = new GameObject ();
			parent.name = parsedLayer.name;
			parent.transform.parent = this.transform;
			if (parsedLayer.goLayer != null)
				parent.SetActive (!goLayer.startInactive);
			else {
				parent.SetActive (!map.pois.startInactive);
			}
			
			Profiler.EndSample ();

			int  featureCount = parsedLayer.goFeatures.Count;

			if (featureCount == 0)
				yield break;

			IList iList = new List<GOFeature> ();
			for (int i = 0; i < featureCount; i++) {

				GOFeature goFeature = (GOFeature)parsedLayer.goFeatures [i];

				if (goFeature.goFeatureType == GOFeatureType.Undefined || goFeature.goFeatureType == GOFeatureType.MultiPoint) {
					continue;
				}

				if (goFeature.goFeatureType == GOFeatureType.Point || goFeature.goFeatureType == GOFeatureType.Label){ //POIS 
					goFeature.parent = parent;
					iList.Add (goFeature);
					continue;
				}
					
				if (goLayer.useOnly.Length > 0 && !goLayer.useOnly.Contains (goFeature.kind)) {
					continue;
				}
				if (goLayer.avoid.Length > 0 && goLayer.avoid.Contains (goFeature.kind)) {
					continue;
				}

				if (goLayer.layerType == GOLayer.GOLayerType.Roads) {

					if (goFeature.goFeatureType != GOFeatureType.Line && goFeature.goFeatureType != GOFeatureType.MultiLine)
						continue;

					GORoadFeature grf = (GORoadFeature)goFeature;
					if ((grf.isBridge && !goLayer.useBridges) || (grf.isTunnel && !goLayer.useTunnels) || (grf.isLink && !goLayer.useBridges)) {
						continue;
					}
				}

				goFeature.parent = parent;

				iList.Add (goFeature);

			}

//			Profiler.BeginSample("[GoMap] [BuildLayer] merge roads");
//			if (goLayer.layerType == GOLayer.GOLayerType.Roads) {
//				iList = GORoadFeature.MergeRoads (iList);
//			}
//			Profiler.EndSample ();

			int n = 100;
			for (int i = 0; i < iList.Count; i+=n) {

				for (int k = 0; k<n; k++) {
					if (i + k >= iList.Count) {
//						yield return null;
						break;
					}

					GOFeature r = (GOFeature)iList [i + k];
					IEnumerator routine = r.BuildFeature (this, delayedLoad);

					if (routine != null) {
						if (Application.isPlaying)
							StartCoroutine (routine);
						else
							GORoutine.start (routine, this);
					}

				}
//				yield return null;
			}
//				
//			yield return null;
		}
			
		#if UNITY_EDITOR
		//This is called only in the editor mode.
		public void OnProcedureComplete (GOPbfProcedure procedure) {

			if (!Application.isPlaying) {

				if (goTile.useElevation) {
					MeshFilter filter = GetComponent<MeshFilter> ();
					filter.mesh = procedure.goTile.goMesh.ToMesh ();
				}

				foreach (GOParsedLayer p in procedure.list) {
					GORoutine.start (BuildLayer (p, false), this);
				}
			}
		}

		#endif
	}
}
