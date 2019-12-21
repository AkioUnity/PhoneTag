using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace GoMap {

	[System.Serializable]
	public class GORoadFeature : GOFeature {

		public bool isBridge;
		public bool isTunnel;
		public bool isLink;

		public Vector3 startingPoint;
		public Vector3 endingPoint;


		public GORoadFeature () {

		}

		public GORoadFeature (GORoadFeature f) {

			name = f.name;
			featureIndex = f.featureIndex;
			goFeatureType = f.goFeatureType;
			properties = f.properties;
			attributes = f.attributes;
			layer = f.layer;
			goTile = f.goTile;

			//After editing the feature in layer subclasses.

			//		public string kind;
			kind = f.kind;
			renderingOptions = f.renderingOptions;
			detail = f.detail;
			sort = f.sort;
			y = f.y;
			height = f.height;
			featureIndex = f.featureIndex;
			layerIndex = f.layerIndex;
			featureCount = f.featureCount;

			isBridge = f.isBridge;
			isTunnel = f.isTunnel;
			isLink = f.isLink;


		}

		public List<GORoadFeature> FindRoadsMatching(IList roads) {
		
			List<GORoadFeature> matching = new List<GORoadFeature>();
			foreach (GORoadFeature r in roads) {

				bool geoMatch = r.startingPoint.Equals (endingPoint) || r.endingPoint.Equals (startingPoint);
				bool reversedGeoMatch = r.startingPoint.Equals (startingPoint) || r.endingPoint.Equals (endingPoint);

				bool nameMatch = r.name == "" || name == "" || r.name == name ;
				bool kindMatch = r.kind == kind;

				if ((geoMatch || reversedGeoMatch) && nameMatch && kindMatch) {
					if (AngleWithRoad (r) > 90) {
						matching.Add (r);
					}
				}
			} 
				
			return matching;
		}

		public float AngleWithRoad (GORoadFeature r) {

			Vector3 dir1 = Vector3.zero; //this
			Vector3 dir2 = Vector3.zero; //other

			if (r.startingPoint.Equals (endingPoint)) {

				dir1 = convertedGeometry [convertedGeometry.Count - 2] - endingPoint;
				dir2 = r.convertedGeometry[1] -r.startingPoint;

			} else if ( r.endingPoint.Equals (startingPoint)){

				dir2 = r.convertedGeometry [r.convertedGeometry.Count - 2] - r.endingPoint;
				dir1 = convertedGeometry[1] - startingPoint;
			}
			else if ( r.startingPoint.Equals (startingPoint)){

				dir1 = convertedGeometry[1] - startingPoint;
				dir2 = r.convertedGeometry[1] - r.startingPoint;

			}
			else if ( r.endingPoint.Equals (endingPoint)){
				
				dir1 = convertedGeometry [convertedGeometry.Count - 2] - endingPoint;
				dir2 = r.convertedGeometry [r.convertedGeometry.Count - 2] - r.endingPoint;
			}

			float angle = Vector3.Angle (dir1, dir2);
			return angle;

		}

		public List<GORoadFeature> Merge(IList roads) {

			List<GORoadFeature> merged = new List<GORoadFeature>();

			for (int i = 0; i < roads.Count; i++) {
				GORoadFeature r = (GORoadFeature)roads [i];

				if (r.startingPoint.Equals (endingPoint)) {

					endingPoint = r.endingPoint;
					r.convertedGeometry.RemoveAt (0);
					convertedGeometry.AddRange (r.convertedGeometry);
					r.preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (r);
					preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (this);

					merged.Add (r);

				} else if ( r.endingPoint.Equals (startingPoint)){

					startingPoint = r.startingPoint;
					convertedGeometry.RemoveAt (0);
					r.convertedGeometry.AddRange (convertedGeometry);
					convertedGeometry = r.convertedGeometry;
					r.preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (r);
					preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (this);

					merged.Add (r);
				}
				else if ( r.startingPoint.Equals (startingPoint)){

					startingPoint = r.endingPoint;
					r.convertedGeometry.Reverse ();
					convertedGeometry.RemoveAt (0);
					r.convertedGeometry.AddRange (convertedGeometry);
					convertedGeometry = r.convertedGeometry;
					r.preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (r);
					preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (this);

					merged.Add (r);
				}
				else if ( r.endingPoint.Equals (endingPoint)){

					endingPoint = r.startingPoint;
					r.convertedGeometry.Reverse ();
					r.convertedGeometry.RemoveAt (0);
					convertedGeometry.AddRange (r.convertedGeometry);
					r.preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (r);
					preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (this);

					merged.Add (r);
				}

				if (name == "" && r.name != "") {
					name = r.name;
				}
			}
				

			return merged;
		}

		#region STATIC

		public static IList MergeRoads (IList roads) {

			List <GORoadFeature> merged = new List <GORoadFeature> ();

			foreach (GORoadFeature r in roads) {

				if (r.convertedGeometry == null || r.convertedGeometry.Count == 0)
					continue;

				r.startingPoint = r.convertedGeometry [0];
				r.endingPoint = r.convertedGeometry [r.convertedGeometry.Count - 1];

				List <GORoadFeature> m = r.FindRoadsMatching (merged);
				if (m.Count == 0) {
					merged.Add (r);
					continue;
				}

				List<GORoadFeature> toRemove = r.Merge (m);
				merged = merged.Except (toRemove).ToList();
				merged.Add (r);

			}

			return merged;

		}

//		public static IList MergeRoad (IList roads, GORoadFeature r) {
//
//
//			if (r.convertedGeometry == null || r.convertedGeometry.Count == 0)
//				return roads;
//
//
//			r.startingPoint = r.convertedGeometry [0];
//			r.endingPoint = r.convertedGeometry [r.convertedGeometry.Count - 1];
//
//			List <GORoadFeature> m = r.FindRoadsMatching (roads);
//			if (m.Count == 0) {
//				r.preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (r);
//				roads.Add (r);
//				return roads;
//			}
//
//			List<GORoadFeature> toRemove = r.Merge (m);
//
//
//			roadsCasted = roadsCasted.Except (toRemove).ToList();
//
//			r.preloadedMeshData = GOFeatureMeshBuilder.PreloadFeatureData (r);
//			roads.Add (r);
//
//			return roads;
//
//		}
//



		#endregion


	}
}
