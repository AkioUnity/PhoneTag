using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.VectorTile;
using Mapbox.VectorTile.Geometry;


namespace GoShared{

	public static class MapboxLayerExtensions  {

		public static void AddSortRankToFeatures(
			this VectorTileLayer tile
		) {
			for (int i = 0; i < tile.FeatureCount (); i++) {
				VectorTileFeature f = tile.GetFeature (i);
			}
		}

		public static string GetFeatureType(this VectorTileFeature feature, List<List<LatLng>> geomWgs84) {
			string type = null;

			if (geomWgs84.Count > 1) {
				switch (feature.GeometryType) {
				case GeomType.POINT:
					type = "MultiPoint";
					break;
				case GeomType.LINESTRING:
					type = "MultiLineString";
					break;
				case GeomType.POLYGON:
					type = "MultiPolygon";
					break;
				default:
					break;
				}
			} else if (geomWgs84.Count == 1) { //singlepart
				switch (feature.GeometryType) {
				case GeomType.POINT:
					type = "Point";
					break;
				case GeomType.LINESTRING:
					type = "LineString";
					break;
				case GeomType.POLYGON:
					type = "Polygon";
					break;
				default:
					break;
				}
			} else {//no geometry

			}
			return type;		
		}
			
		public static GOFeatureType GOFeatureType(this VectorTileFeature feature, List<List<LatLng>> geomWgs84) {

			GOFeatureType type = GoShared.GOFeatureType.Undefined;

			if (geomWgs84.Count > 1) {
				switch (feature.GeometryType) {
				case GeomType.POINT:
					type = GoShared.GOFeatureType.MultiPoint;
					break;
				case GeomType.LINESTRING:
					type =GoShared. GOFeatureType.MultiLine;
					break;
				case GeomType.POLYGON:
					type = GoShared.GOFeatureType.MultiPolygon;
					break;
				default:
					break;
				}
			} else if (geomWgs84.Count == 1) { //singlepart
				switch (feature.GeometryType) {
				case GeomType.POINT:
					type = GoShared.GOFeatureType.Point;
					break;
				case GeomType.LINESTRING:
					type = GoShared.GOFeatureType.Line;
					break;
				case GeomType.POLYGON:
					type = GoShared.GOFeatureType.Polygon;
					break;
				default:
					break;
				}
			} else {//no geometry

			}
			return type;		
		}
	}
}


