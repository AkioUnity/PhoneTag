using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using GoShared;
using Mapbox.VectorTile;
using UnityEngine.Profiling;

namespace GoMap
{
	[ExecuteInEditMode]
	public class GOMapzenProtoTile : GOPBFTileAsync
	{

		public override string GetLayersStrings (GOLayer layer)
		{
			return layer.json();
		}
		public override string GetPoisStrings ()
		{
			return map.pois.json();
		}
		public override string GetPoisKindKey ()
		{
			return "kind";
		}

		public override string GetLabelsStrings ()
		{
			return "";
		}

		public override GOFeature EditLabelData (GOFeature goFeature)
		{
			goFeature.name = (string) goFeature.properties ["name"];
			goFeature.kind = GOEnumUtils.MapzenToKind((string)goFeature.properties ["kind"]);
			if (goFeature.properties.Contains("kind_detail")) { //Mapzen
				goFeature.kind = GOEnumUtils.MapzenToKind((string)goFeature.properties ["kind_detail"]);
			}
				
	
			Int64 sort = 0;
			if (goFeature.properties.Contains ("sort_rank")) {
				sort = Convert.ToInt64(goFeature.properties ["sort_rank"]);
			} else if (goFeature.properties.Contains("sort_key")) {
				sort = Convert.ToInt64(goFeature.properties ["sort_key"]);
			}
			goFeature.y = sort / 1000.0f;
			goFeature.sort = sort;

			return goFeature;

		}

		public override GOFeature EditFeatureData (GOFeature feature) {

			feature.name = (string) feature.properties ["name"];
			if (feature.goFeatureType == GOFeatureType.Point)
				return feature;

			if (feature.GetType() == typeof(GORoadFeature)) {
				GORoadFeature grf = (GORoadFeature)feature;

				grf.kind = GOEnumUtils.MapzenToKind((string)grf.properties ["kind"]);
				if (grf.properties.Contains("kind_detail")) { //Mapzen
					grf.detail = (string)grf.properties ["kind_detail"];
				}

				grf.isBridge = grf.properties.Contains ("is_bridge") && Convert.ToBoolean(grf.properties ["is_bridge"]);
				grf.isTunnel = grf.properties.Contains ("is_tunnel") && Convert.ToBoolean(grf.properties ["is_tunnel"]);
				grf.isLink = grf.properties.Contains ("is_link") && Convert.ToBoolean(grf.properties ["is_link"]);

			} else {
				feature.kind = GOEnumUtils.MapzenToKind((string)feature.properties ["kind"]);
				if (feature.properties.Contains("kind_detail")) { //Mapzen
					feature.kind = GOEnumUtils.MapzenToKind((string)feature.properties ["kind_detail"]);
				}
			}

			//			goFeature.name = Convert.ToString( properties ["id"]);

			feature.setRenderingOptions ();

			Int64 sort = 0;
			if (feature.properties.Contains ("sort_rank")) {
				sort = Convert.ToInt64(feature.properties ["sort_rank"]);
			} else if (feature.properties.Contains("sort_key")) {
				sort = Convert.ToInt64(feature.properties ["sort_key"]);
			}
			feature.y = sort / 1000.0f;
			feature.sort = sort;

			feature.height = feature.renderingOptions.polygonHeight;

			if (feature.layer.useRealHeight && feature.properties.Contains("height")) {
				double h =  Convert.ToDouble(feature.properties["height"]);
				feature.height = (float)h;
			}

			if (feature.layer.useRealHeight && feature.properties.Contains("min_height")) {
				double hm = Convert.ToDouble(feature.properties["min_height"]);
				feature.y = (float)hm;
				if (feature.height >= hm) {
					feature.y = (float)hm;
					feature.height = (float)feature.height - (float)hm;
				}
			} 

            if (feature.layer.forceMinHeight && feature.height < feature.renderingOptions.polygonHeight && feature.y < 0.5f)
            {
                feature.height = feature.renderingOptions.polygonHeight;
            }
				
			return feature;

		}

		#region NETWORK

		public override string vectorUrl ()
		{
			var baseUrl = "https://tile.mapzen.com/mapzen/vector/v1/all/";
			var extension = ".mvt";

			//Download vector data
			Vector2 realPos = goTile.tileCoordinates;
            var tileurl = goTile.zoomLevel + "/" + realPos.x + "/" + realPos.y;

			var completeUrl = baseUrl + tileurl + extension;
//			var filename = "[MapzenProtoVector]" + gameObject.name;

            if (goTile.apiKey != null && goTile.apiKey  != "") {
                string u = completeUrl + "?api_key=" + goTile.apiKey;
				completeUrl = u;
			}

			return completeUrl;

		}

		public override string demUrl ()
		{
            var tileurl = goTile.zoomLevel + "/" + goTile.tileCoordinates.x + "/" + goTile.tileCoordinates.y;
			var baseUrl = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/";
			var extension = ".png";
			var completeUrl = baseUrl + tileurl + extension;
            if (goTile.apiKey  != null && goTile.apiKey  != "") {
                string u = completeUrl + "?api_key=" + goTile.apiKey;
				completeUrl = u;
			}
			return completeUrl;
		}

		public override string normalsUrl ()
		{
			//Normals data
            var tileurl = goTile.zoomLevel + "/" + goTile.tileCoordinates.x + "/" + goTile.tileCoordinates.y;
			var baseUrlNormals = "https://tile.mapzen.com/mapzen/terrain/v1/normal/";
			var extension = ".png";
			var normalsUrl = baseUrlNormals + tileurl + extension; 

            if (goTile.apiKey  != null && goTile.apiKey  != "") {
                string u = normalsUrl + "?api_key=" + goTile.apiKey;
				normalsUrl = u;
			}

			return normalsUrl;
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
            var sizeUrl = "/256x256.jpg?access_token="+goTile.apiKey;
			var completeurl = baseUrl + tileurl + sizeUrl; 

			return completeurl;

		}

		#endregion


	}
}
