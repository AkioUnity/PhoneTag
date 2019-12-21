using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mapbox.Utils;
using Mapbox.VectorTile;
using Mapbox.VectorTile.Geometry;
using GoShared;
using System;
using Mapbox.VectorTile.ExtensionMethods;


namespace GoMap {

	[ExecuteInEditMode]
	public class GOPbfProcedure : ThreadedJob {

		//In
		public GOLayer[] layers;
		public GOPBFTileAsync tile;
		public GOTileObj goTile;

		//Out
		public List<GOParsedLayer> list;
		public DateTime tT;
		public DateTime tF;
		public DateTime tP;
		public DateTime tL;

		protected override void OnFinished () {

			if (!Application.isPlaying) {
				#if UNITY_EDITOR
				tile.OnProcedureComplete (this);
//				tile.Update ();
				#endif
			}	
		}

		#region Main

		protected override void ThreadFunction()
		{

			if (goTile.useElevation) {
				goTile.elevatedTerrainMesh ();
			}
			tT = DateTime.Now;

			var decompressed = Mapbox.Utils.Compression.Decompress(goTile.getVectorData());
			VectorTile vt = new VectorTile(decompressed,false);

			//DEBUG TO GEOJSON
//			string debug = vt.ToGeoJson ((ulong)goTile.zoomLevel, (ulong)goTile.tileCoordinates.x, (ulong)goTile.tileCoordinates.y,0);
//			FileHandler.SaveText ("DEBUG_TILE", debug);

			//DEBUG LAYERS
//			foreach (string lyr in vt.LayerNames()) {
//				Debug.Log (lyr);
//			}


			list = new List<GOParsedLayer> ();

            //Sort layers so that the order is right for dropping environment
            Array.Sort(layers, (GOLayer a, GOLayer b) => GOLayer.LayerTypeToIndex(a.layerType).CompareTo(GOLayer.LayerTypeToIndex(b.layerType)));

			foreach (GOLayer layer in layers) {

				if (layer.disabled)
					continue;
				ParseGOLayerToList (list, vt, layer);
			}


			tF = DateTime.Now;

			if (tile.map.pois != null && (tile.map.pois.renderingOptions.Length >0  && !tile.map.pois.disabled))
				ParsePOILayerToList (list, vt, tile.map.pois);
			tP = DateTime.Now;

			if (tile.map.labels != null && !tile.map.labels.disabled)
				ParseLabelLayerToList (list, vt, tile.map.labels);
			tL = DateTime.Now;


		}

		#endregion

		#region Vector Features

		private void AddFatureToList(GOFeature f, IList list) {

			f.preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (f);
			if (f.goFeatureType == GOFeatureType.Point || f.goFeatureType == GOFeatureType.Label ||f.preloadedMeshData != null)
				list.Add (f);

		}

		private void ParseGOLayerToList (List<GOParsedLayer> list, VectorTile vt, GOLayer layer) {

			string[] lyrs = tile.GetLayersStrings(layer).Split(',');

			foreach (string l in lyrs) {

					VectorTileLayer lyr = vt.GetLayer(l);
					if (lyr != null) {

						int  featureCount = lyr.FeatureCount();

						if (featureCount == 0)
							continue;

						GOParsedLayer pl = new GOParsedLayer ();
						pl.name = lyr.Name;
						pl.goLayer = layer;
						pl.goFeatures = new List<GOFeature> ();

						int indexOfLayer = vt.LayerNames ().IndexOf (lyr.Name);

						for (int i = 0; i < featureCount; i++) {

							VectorTileFeature vtf = lyr.GetFeature(i);

							List<List<LatLng>> geomWgs = vtf.GeometryAsWgs84((ulong)goTile.zoomLevel, (ulong)goTile.tileCoordinates.x, (ulong)goTile.tileCoordinates.y,0);

							GOFeature gf;
							if (layer.layerType == GOLayer.GOLayerType.Roads) {
								gf = new GORoadFeature ();
							} else {
								gf = new GOFeature ();
							}

							gf.properties = vtf.GetProperties ();
							gf.attributes = GOFeature.PropertiesToAttributes (gf.properties);
							gf.goFeatureType = vtf.GOFeatureType (geomWgs);
							gf.layer = layer;
							gf.featureIndex = (Int64)i;
							gf.layerIndex = indexOfLayer;
							gf.featureCount = featureCount;
							gf = tile.EditFeatureData (gf);
							gf.goTile = goTile;
//							gf.setRenderingOptions ();
							gf.ConvertAttributes ();

							if (geomWgs.Count > 0) {

								switch (gf.goFeatureType) {

							case GOFeatureType.Line:
								gf.geometry = geomWgs [0];
								gf.ConvertGeometries ();
								AddFatureToList(gf,pl.goFeatures);
								break;
							case GOFeatureType.Polygon:
								gf.geometry = geomWgs[0];
								gf.ConvertGeometries ();
								AddFatureToList(gf,pl.goFeatures);
								break;
							case GOFeatureType.MultiLine:
								foreach (IList geometry in geomWgs) {

									float indexMulti = (((float)geomWgs.IndexOf((List<LatLng>)geometry) +1)*(i+1)/geomWgs.Count);
									GOFeature gfm;
									if (layer.layerType == GOLayer.GOLayerType.Roads) {
										gfm = new GORoadFeature ((GORoadFeature)gf);
									} else {
										gfm = new GOFeature (gf);
									}

//									gfm.index = indexMulti;
									gfm.geometry = geometry;
									gfm.ConvertGeometries();
									AddFatureToList(gfm,pl.goFeatures);
								}
								break;
							case GOFeatureType.MultiPolygon:
								foreach (IList geometry in geomWgs) {

									List<Vector3> convertedSubject = null;
									List<List<Vector3>> convertedClips = new List<List<Vector3>>();

									for (int j = 0; j<geomWgs.Count; j++) { //Clip ascending

										IList p = geomWgs [j];
										List <Vector3> convertedP = GOFeature.CoordsToVerts (p, layer.layerType == GOLayer.GOLayerType.Buildings);
										if (GOFeature.IsClockwise(convertedP)) {
											convertedSubject = convertedP;
										} 
										else {
											//Add clip
											convertedClips.Add (convertedP);
										}
										//Last one
										if (j == geomWgs.Count - 1 || (j<geomWgs.Count-1 && GOFeature.IsGeoPolygonClockwise (geomWgs [j + 1]) && convertedSubject != null)) {

											GOFeature gfm = new GOFeature (gf);
//											gfm.index = (i +1)*j;
											gfm.convertedGeometry = convertedSubject;
											gfm.clips = convertedClips;
											AddFatureToList(gfm,pl.goFeatures);

											convertedSubject = null;
											convertedClips = new List<List<Vector3>>();
										}
									}
								}
								break;
								}
							}
						}

						if (goTile.combineFeatures) {
							pl = GOCombineFeatures.Combine (pl);
						}
					
						list.Add (pl);
					}
				}
		}

		private void ParsePOILayerToList (List<GOParsedLayer> list, VectorTile vt, GOPOILayer layer) {

			string[] lyrs = tile.GetPoisStrings().Split(',');
			string kindKey = tile.GetPoisKindKey();

			foreach (string l in lyrs) {

				VectorTileLayer lyr = vt.GetLayer(l);
				if (lyr != null) {

					int  featureCount = lyr.FeatureCount();

					if (featureCount == 0)
						continue;

					GOParsedLayer pl = new GOParsedLayer ();
					pl.name = lyr.Name;
					pl.poiLayer = layer;
					pl.goFeatures = new List<GOFeature> ();

					for (int i = 0; i < featureCount; i++) {

						VectorTileFeature vtf = lyr.GetFeature(i);
						IDictionary properties = vtf.GetProperties ();

						GOPOIKind kind = GOEnumUtils.PoiKindToEnum((string)properties[kindKey]);
						GOPOIRendering rendering = layer.GetRenderingForPoiKind (kind);

						if (kind == GOPOIKind.UNDEFINED || rendering == null)
							continue;

						List<List<LatLng>> geomWgs = vtf.GeometryAsWgs84((ulong)goTile.zoomLevel, (ulong)goTile.tileCoordinates.x, (ulong)goTile.tileCoordinates.y,0);
						GOFeature gf = new GOFeature ();
						gf.poiKind = kind;
						gf.goTile = goTile;
						gf.properties = properties;
						gf.attributes = GOFeature.PropertiesToAttributes (gf.properties);
						gf.goFeatureType = vtf.GOFeatureType (geomWgs);

						if (gf.goFeatureType == GOFeatureType.Undefined) {
							continue;
						}

						gf.poiLayer = layer;
						gf.poiRendering = rendering;
						gf.featureIndex = (Int64)i + vt.LayerNames().IndexOf(lyr.Name);
						gf = tile.EditFeatureData (gf);
						gf.ConvertAttributes ();

						if (geomWgs.Count > 0 && gf.goFeatureType == GOFeatureType.Point) {

							gf.geometry = geomWgs [0];
							gf.ConvertPOIGeometries ();
							AddFatureToList(gf,pl.goFeatures);
						}
					}

					list.Add (pl);
				}
			}
		}

		private void ParseLabelLayerToList (List<GOParsedLayer> list, VectorTile vt, GOLabelsLayer layer) {

			string[] lyrs = tile.GetLabelsStrings().Split(',');
//			string kindKey = tile.GetPoisKindKey();

			foreach (string l in lyrs) {

				VectorTileLayer lyr = vt.GetLayer(l);
				if (lyr != null) {

					int  featureCount = lyr.FeatureCount();

					if (featureCount == 0)
						continue;

					GOParsedLayer pl = new GOParsedLayer ();
					pl.name = lyr.Name;
					pl.labelsLayer = layer;
					pl.goFeatures = new List<GOFeature> ();

					for (int i = 0; i < featureCount; i++) {

						VectorTileFeature vtf = lyr.GetFeature(i);
						IDictionary properties = vtf.GetProperties ();

						List<List<LatLng>> geomWgs = vtf.GeometryAsWgs84((ulong)goTile.zoomLevel, (ulong)goTile.tileCoordinates.x, (ulong)goTile.tileCoordinates.y,0);

						if (geomWgs.Count == 0 || geomWgs[0].Count <= 1)
							continue;

						GOFeature gf = new GOFeature ();
						gf.properties = properties;
						gf.goFeatureType = vtf.GOFeatureType (geomWgs);
						gf.labelsLayer = layer;
						gf.featureIndex = (Int64)i + vt.LayerNames().IndexOf(lyr.Name);
						gf.goTile = goTile;
						gf = tile.EditLabelData (gf);
						gf.goFeatureType = GOFeatureType.Label;

						gf.ConvertAttributes ();
						if (geomWgs.Count > 0) {

							gf.geometry = geomWgs[0];
							gf.ConvertGeometries ();
							gf.preloadedLabelData = GOSegment.FindTheLongestStreightSegment (gf.convertedGeometry, 0);
							AddFatureToList(gf,pl.goFeatures);
						}
					}
					list.Add (pl);
				}
			}
		}
			
		#endregion

	}


	public class GOParsedLayer {

		public IList goFeatures;
		public GOLayer.GOLayerType type;
		public string name;
		public GOLayer goLayer;
		public GOPOILayer poiLayer;
		public GOLabelsLayer labelsLayer;
	}

}


