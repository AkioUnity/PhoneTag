using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {

	public class GOGridMaker {

		public static GOMesh CreateGrid (float size, int resolution) {

			resolution++;

			GOMesh goMesh = new GOMesh ();
			float factor = (size / (resolution - 1));

			List<Vector3> vertices = new List<Vector3> ();
			List<int> triangles = new List<int> ();
			List<Vector2> uv = new List<Vector2> ();

			for (int i = 0; i < resolution; i++) {
			
				for (int j = 0; j < resolution; j++) {

					Vector3 vert = new Vector3 (factor * j, 0, factor * i) + new Vector3 (-size/2, 0 , -size/2);
					vertices.Add (vert);

					uv.Add (new Vector2 (vert.x,vert.z)/ (size/4)) ;

					if (i == 0 || j == 0) {
						continue;
					}

					List<int> tris = new List<int> {

						resolution * (i-1) + (j-1), resolution*i + (j-1), resolution * i + j, 
						resolution*(i-1) +j, resolution * (i-1) + (j-1), resolution * i + j, 
					};
					triangles.AddRange (tris);
				}	
			}

			goMesh.vertices = vertices.ToArray ();
			goMesh.triangles = triangles.ToArray ();
			goMesh.uv = uv.ToArray ();

			return goMesh;
		}
	}

}
