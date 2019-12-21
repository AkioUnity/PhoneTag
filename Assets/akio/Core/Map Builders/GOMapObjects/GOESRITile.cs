using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using GoShared;
using Mapbox.VectorTile;


namespace GoMap
{
	[ExecuteInEditMode]
	public class GOEsriTIle : GOPBFTileAsync
	{

		public override string GetLayersStrings (GOLayer layer)
		{
			return layer.lyr_esri();
		}

		public override string GetPoisStrings ()
		{
			return "";
		}

		public override string GetLabelsStrings ()
		{
			return map.labels.lyr_esri();
		}

		public override string GetPoisKindKey ()
		{
			return "";
		}

		public override GOFeature EditLabelData (GOFeature goFeature) {
			
			string labelKey = goFeature.labelsLayer.LanguageKey (goFeature.goTile.mapType);
			if (goFeature.properties.Contains (labelKey) && !string.IsNullOrEmpty ((string)goFeature.properties [labelKey])) {
				goFeature.name = (string)goFeature.properties [labelKey];
			} else goFeature.name = (string)goFeature.properties ["name"];

			goFeature.kind = GOEnumUtils.MapboxToKind(goFeature.labelsLayer.name);
			goFeature.y = goFeature.getLayerDefaultY()+1;

			return goFeature;
		}

		public override GOFeature EditFeatureData (GOFeature goFeature) {

			IDictionary properties = goFeature.properties;

			if (goFeature.goFeatureType == GOFeatureType.Point) {
				goFeature.name = (string)goFeature.properties ["name"];
				return goFeature;
			} else {
				goFeature.name = goFeature.layer.name;
			}

			goFeature.kind = GOEnumUtils.MapboxToKind(goFeature.layer.name);
			goFeature.setRenderingOptions ();

			goFeature.y = goFeature.layer.defaultLayerY();
			if (properties.Contains ("_symbol"))
				goFeature.y = Convert.ToInt64 (properties ["_symbol"]) / 15.0f;

//			float fraction = 20f;
//			goFeature.y = (1 + goFeature.layerIndex + goFeature.featureIndex/goFeature.featureCount)/fraction;

			goFeature.height = goFeature.layer.defaultRendering.polygonHeight;

			return goFeature;

		}

		#region NETWORK

		public override string vectorUrl ()
		{
            //var baseUrl = "https://basemaps.arcgis.com/v1/arcgis/rest/services/World_Basemap/VectorTileServer/tile/";
            var baseUrl = "https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/World_Basemap_v2/VectorTileServer/tile/";

			var extension = ".pbf";

			//Download vector data
			Vector2 realPos = goTile.tileCoordinates;
			var tileurl = map.zoomLevel + "/" + realPos.y + "/" + realPos.x; //of course Esri uses inverted tile x,y. =/
			var completeUrl = baseUrl + tileurl + extension; 
//			var filename = "[ESRIVector]" + gameObject.name;

			return completeUrl;
		}
			
		public override string demUrl ()
		{
			return null;
		}

		public override string normalsUrl ()
		{
			return null;
		}

		public override string satelliteUrl (Vector2? tileCoords = null)
		{
			//Satellite data
			var baseUrl = "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/";
			var tileurl = map.zoomLevel + "/" + goTile.tileCoordinates.y + "/" + goTile.tileCoordinates.x;
			if (tileCoords != null)
				tileurl = map.zoomLevel+1 + "/" + ((Vector2)tileCoords).y + "/" + ((Vector2)tileCoords).x;

			var completeurl = baseUrl + tileurl; 
			return completeurl;
		
		}

		#endregion

	}
}
