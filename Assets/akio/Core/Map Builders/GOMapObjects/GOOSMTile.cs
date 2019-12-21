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
	public class GOOSMTile : GOPBFTileAsync
	{

		public override string GetLayersStrings (GOLayer layer)
		{
			return layer.lyr_osm ();
		}

		public override string GetPoisStrings ()
		{
			return map.pois.lyr_osm();
		}
		public override string GetPoisKindKey ()
		{
			return "class";
		}
		public override string GetLabelsStrings ()
		{
			return map.labels.lyr_osm();
		}

		public override GOFeature EditLabelData (GOFeature goFeature)
		{

			IDictionary properties = goFeature.properties;

			string labelKey = goFeature.labelsLayer.LanguageKey (goFeature.goTile.mapType);
			if (properties.Contains (labelKey) && !string.IsNullOrEmpty ((string)properties [labelKey])) {
				goFeature.name = (string)goFeature.properties [labelKey];
			} else goFeature.name = (string)goFeature.properties ["name"];

			goFeature.kind = GOEnumUtils.MapboxToKind((string)properties["class"]);

			goFeature.y = goFeature.getLayerDefaultY();

			return goFeature;

		}

		public override GOFeature EditFeatureData (GOFeature goFeature) {

			IDictionary properties = goFeature.properties;

			if (goFeature.goFeatureType == GOFeatureType.Point){
				goFeature.name = (string)goFeature.properties ["name"];
				return goFeature;
			}

			if (goFeature.layer != null && goFeature.layer.layerType == GOLayer.GOLayerType.Roads) {
				((GORoadFeature)goFeature).isBridge = properties.Contains ("brunnel") && (string)properties ["brunnel"] == "bridge";
				((GORoadFeature)goFeature).isTunnel = properties.Contains ("brunnel") && (string)properties ["brunnel"] == "tunnel";
				((GORoadFeature)goFeature).isLink = properties.Contains ("brunnel") && (string)properties ["brunnel"] == "link";
			} 

			goFeature.kind = GOEnumUtils.MapboxToKind((string)properties["class"]);

//			goFeature.y = goFeature.featureIndex/1000 + goFeature.layer.defaultLayerY();
//			if (goFeature.kind == GOFeatureKind.lake) //terrible fix for vector maps without a sort value.
//				goFeature.y = GOLayer.defaultLayerY (GOLayer.GOLayerType.Landuse)+0.1f;

			float fraction = 20f;
			goFeature.y = (1 + goFeature.layerIndex + goFeature.featureIndex/goFeature.featureCount)/fraction;
			if (goFeature.kind == GOFeatureKind.lake) //terrible fix for vector maps without a sort value.
				goFeature.y += 0.5f;
			
			goFeature.setRenderingOptions ();
			goFeature.height = goFeature.renderingOptions.polygonHeight;

			if (goFeature.layer.useRealHeight && properties.Contains("render_height")) {
				double h =  Convert.ToDouble(properties["render_height"]);
				goFeature.height = (float)h;         
			}

			if (goFeature.layer.useRealHeight && properties.Contains("render_min_height")) {
				double hm = Convert.ToDouble(properties["render_min_height"]);
				goFeature.y = (float)hm;
				if (goFeature.height >= hm) {
					goFeature.y = (float)hm;
					goFeature.height = (float)goFeature.height - (float)hm;
				}
			} 


            if (goFeature.layer.forceMinHeight && goFeature.height < goFeature.renderingOptions.polygonHeight && goFeature.y < 0.5f) {
                goFeature.height = goFeature.renderingOptions.polygonHeight;
            }

			return goFeature;
		}

		#region NETWORK

		public override string vectorUrl ()
		{
			//var baseUrl = "https://free-0.tilehosting.com/data/v3/";
            var baseUrl = "https://maps.tilehosting.com/data/v3/";
			var extension = ".pbf.pict";

			//Download vector data
			Vector2 realPos = goTile.tileCoordinates;
            var tileurl = goTile.zoomLevel + "/" + realPos.x + "/" + realPos.y;

			var completeUrl = baseUrl + tileurl + extension; 
//			var filename = "[OSMVector]" + gameObject.name;

            if (goTile.apiKey != null && goTile.apiKey != "") {
                string u = completeUrl + "?key=" + goTile.apiKey;
				completeUrl = u;
			}

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
            var tileurl = goTile.tileCenter.longitude + "," + goTile.tileCenter.latitude + "," +goTile.zoomLevel;
			if (tileCoords != null) {
                Coordinates coord = new Coordinates ((Vector2)tileCoords,goTile.zoomLevel+1);
                tileurl = coord.longitude + "," + coord.latitude + "," +(goTile.zoomLevel+1);
			}

			var baseUrl = "https://api.mapbox.com/v4/mapbox.satellite/";
			var sizeUrl = "/256x256.jpg?access_token="+map.mapbox_accessToken;
			var completeurl = baseUrl + tileurl + sizeUrl; 

			return completeurl;

		}

		#endregion

	}
}
