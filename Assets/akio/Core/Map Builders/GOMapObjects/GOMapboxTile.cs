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
    public class GOMapboxTile : GOPBFTileAsync
    {


        public override string GetLayersStrings(GOLayer layer)
        {
            return layer.lyr();
        }
        public override string GetPoisStrings()
        {
            return map.pois.lyr();
        }

        public override string GetLabelsStrings()
        {
            return map.labels.lyr();
        }

        public override string GetPoisKindKey()
        {
            return "type";
        }

        public override GOFeature EditLabelData(GOFeature goFeature)
        {

            IDictionary properties = goFeature.properties;

            string labelKey = goFeature.labelsLayer.LanguageKey(goFeature.goTile.mapType);
            if (properties.Contains(labelKey) && !string.IsNullOrEmpty((string)properties[labelKey]))
            {
                goFeature.name = (string)goFeature.properties[labelKey];
            }
            else goFeature.name = (string)goFeature.properties["name"];

            goFeature.kind = GOEnumUtils.MapboxToKind((string)properties["class"]);
            goFeature.y = goFeature.getLayerDefaultY();


            return goFeature;
        }

        public override GOFeature EditFeatureData(GOFeature goFeature)
        {

            if (goFeature.goFeatureType == GOFeatureType.Point)
            {
                goFeature.name = (string)goFeature.properties["name"];
                return goFeature;
            }

            IDictionary properties = goFeature.properties;

            goFeature.kind = GOEnumUtils.MapboxToKind((string)properties["class"]);
            goFeature.name = (string)properties["class"];

            if (!properties.Contains("class") && properties.Contains("type"))
            {
                goFeature.kind = GOEnumUtils.MapboxToKind((string)properties["type"]);
                goFeature.name = (string)properties["type"];
            }

            if (goFeature.layer != null && goFeature.layer.layerType == GOLayer.GOLayerType.Roads)
            {

                ((GORoadFeature)goFeature).isBridge = properties.Contains("structure") && (string)properties["structure"] == "bridge";
                if (((GORoadFeature)goFeature).isBridge) {
                    goFeature.kind = GOFeatureKind.bridge;
                }
				((GORoadFeature)goFeature).isTunnel = properties.Contains ("structure") && (string)properties ["structure"] == "tunnel";
                if (((GORoadFeature)goFeature).isTunnel)
                {
                    goFeature.kind = GOFeatureKind.tunnel;
                }
				((GORoadFeature)goFeature).isLink = properties.Contains ("structure") && (string)properties ["structure"] == "link";
			} 



//			goFeature.y = (goFeature.index / 50.0f) + goFeature.getLayerDefaultY() /150.0f;
//			float fraction = goFeature.layer.layerType == GOLayer.GOLayerType.Buildings? 100f:10f;
			float fraction = 20f;
			goFeature.y = (1 + goFeature.layerIndex + goFeature.featureIndex/goFeature.featureCount)/fraction;

			goFeature.setRenderingOptions ();
			goFeature.height = goFeature.renderingOptions.polygonHeight;

			bool extrude = properties.Contains("extrude") && (string)properties["extrude"] == "true";

			if (goFeature.layer.useRealHeight && properties.Contains("height") && extrude) {
				double h =  Convert.ToDouble(properties["height"]);
				goFeature.height = (float)h;
			}

			if (goFeature.layer.useRealHeight && properties.Contains("min_height") && extrude) {
				double minHeight = Convert.ToDouble(properties["min_height"]);
				goFeature.y = (float)minHeight;
				goFeature.height = (float)goFeature.height - (float)minHeight;
			} 

            if (goFeature.layer.forceMinHeight && goFeature.height < goFeature.renderingOptions.polygonHeight && goFeature.y < 0.5f)
            {
                goFeature.height = goFeature.renderingOptions.polygonHeight;
            }

			return goFeature;

		}

		#region NETWORK

		public override string vectorUrl ()
		{
			var baseUrl = "https://api.mapbox.com:443/v4/mapbox.mapbox-streets-v7/";
			var extension = ".vector.pbf";

			//Download vector data
			Vector2 realPos = goTile.tileCoordinates;
            var tileurl = goTile.zoomLevel + "/" + realPos.x + "/" + realPos.y;

			var completeUrl = baseUrl + tileurl + extension; 
//			var filename = "[MapboxVector]" + gameObject.name;

            if (goTile.apiKey != null && goTile.apiKey!= "") {
                string u = completeUrl + "?access_token=" + goTile.apiKey;
				completeUrl = u;
			}

			return completeUrl;
		}
			
		public override string demUrl ()
		{

			Vector2 realPos = goTile.tileCoordinates;
            var tileurl = goTile.zoomLevel + "/" + realPos.x + "/" + realPos.y;
			var baseUrl = "https://api.mapbox.com/v4/mapbox.terrain-rgb/";
			var extension = ".pngraw";
			var completeUrl = baseUrl + tileurl + extension; 
            if (goTile.apiKey != null && goTile.apiKey != "") {
                string u = completeUrl + "?access_token=" + goTile.apiKey;
				completeUrl = u;
			}

			return completeUrl;
		}

		public override string normalsUrl ()
		{
			return null;
		}

//		public override string normalsUrl ()
//		{
//			//Normals data
//			var tileurl = map.zoomLevel + "/" + goTile.tileCoordinates.x + "/" + goTile.tileCoordinates.y;
//			var baseUrlNormals = "https://tile.mapzen.com/mapzen/terrain/v1/normal/";
//			var extension = ".png";
//			var normalsUrl = baseUrlNormals + tileurl + extension; 
//
//			if (map.mapzen_api_key != null && map.mapzen_api_key != "") {
//				string u = normalsUrl + "?api_key=" + map.mapzen_api_key;
//				normalsUrl = u;
//			}
//
//			return normalsUrl;
//		}

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

//			https://api.mapbox.com/v4/mapbox.satellite/7.6409912109375,45.9778785705566,15/256x256.jpg?access_token=pk.eyJ1IjoiYWxhbmdyYW50IiwiYSI6ImNpdHdtMXEwdTAwMXozbms5NzBoOGh4djcifQ.SONpcZWMGNpaFk9tCsupaQ

			return completeurl;

		}


		#endregion

	}
}
