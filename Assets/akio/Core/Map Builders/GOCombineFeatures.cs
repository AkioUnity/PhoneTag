using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;
using System.Linq;

namespace GoMap {

	public class GOCombineFeatures {

		public static GOParsedLayer Combine (GOParsedLayer pl) {

			if (pl.goFeatures.Count == 0)
				return pl;
			if (pl.goLayer.layerType == GOLayer.GOLayerType.Roads)
				return pl;

			GOFeature feature = (GOFeature)pl.goFeatures [0];

			for (int i = 1; i<pl.goFeatures.Count; i++) {

				List<Vector3> verts = feature.preloadedMeshData.vertices.ToList();
				List<int> triangles = feature.preloadedMeshData.triangles.ToList ();
				List<Vector2> uvs = feature.preloadedMeshData.uv.ToList();

				GOFeature f = (GOFeature)pl.goFeatures [i];
				verts.AddRange (f.preloadedMeshData.vertices);

				int vertscount = feature.preloadedMeshData.vertices.Count();
				triangles.AddRange(f.preloadedMeshData.triangles.Select (x => x + vertscount) .ToList ());  

				uvs.AddRange (f.preloadedMeshData.uv);

				feature.preloadedMeshData.vertices = verts.ToArray ();
				feature.preloadedMeshData.triangles = triangles.ToArray ();
				feature.preloadedMeshData.uv = uvs.ToArray ();

			}

			pl.goFeatures.Clear ();
			pl.goFeatures.Add (feature);

			return pl;
		}
	}
}