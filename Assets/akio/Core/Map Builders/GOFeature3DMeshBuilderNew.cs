using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoShared;
using UnityEngine.Profiling;

namespace GoMap {

	public class GOFeature3DMeshBuilderNew {

		private List<Vector3> bufVertices = new List<Vector3>();
		private List<Vector3> bufNormals = new List<Vector3>();
		private List<Vector2> bufUVs = new List<Vector2>();
		private List<int> bufIndices = new List<int>();

		private List<GOTempPolyNew> polys = new List<GOTempPolyNew>();

		private Vector2 xRange;
		private Vector2 zRange; 

		public GOMesh ProjectFeature(GOFeature feature, GOMesh terrainMesh, float distance) {

			Vector3[] vertices = terrainMesh.vertices;
			int[] triangles = terrainMesh.triangles;

			GOTempPolyNew poly;

			ComputeFeatureRanges (feature);

			for(int i=0; i<triangles.Length; i+=3) {

				int i1 = triangles[i];
				int i2 = triangles[i+1];
				int i3 = triangles[i+2];

				Vector3 v1 = feature.goTile.position + vertices [i1];
				Vector3 v2 = feature.goTile.position + vertices [i2];
				Vector3 v3 = feature.goTile.position + vertices [i3]; 

				Vector3 side1 = v2 - v1;
				Vector3 side2 = v3 - v1;
				Vector3 normal = Vector3.Cross(side1, side2).normalized;

				poly = new GOTempPolyNew( v1 , v2, v3 );
				poly.normal = normal;
				poly.indices.AddRange (new int[] {i1, i2, i3});
				poly.xRange = xRange;
				poly.zRange = zRange;

//				Profiler.BeginSample ("Wrap Polygon");
				poly = poly.WrapPolygon(feature.convertedGeometry.ToArray(), terrainMesh);
//				Profiler.EndSample ();

				if(poly == null)
					continue;

				polys.Add (poly);

			}

			return MergeTempPolys (distance);

		}

		private GOMesh MergeTempPolys (float distance) {
		
			foreach (GOTempPolyNew temp in polys) {
				AddPolygon (temp, temp.normal);
			}

			if (distance != 0f) {
				Push (distance);
			}

			if(bufIndices.Count == 0) {
				return null;
			}
			GOMesh mesh = new GOMesh();
			mesh.vertices = bufVertices.ToArray();
			mesh.normals = bufNormals.ToArray();
			mesh.uv = bufUVs.ToArray();
			mesh.triangles = bufIndices.ToArray();

			return mesh;
		}
			
		private void AddPolygon(GOTempPolyNew poly, Vector3 normal) {

			int ind1 = AddVertex( poly.vertices[0], normal, poly.indices[0]);
			int ind2 = AddVertex( poly.vertices[1], normal, poly.indices[1]);
			int ind3 = AddVertex( poly.vertices[2], normal, poly.indices[2]);

			bufIndices.Add( ind1 );
			bufIndices.Add( ind2 );
			bufIndices.Add( ind3 );
		}

		private int AddVertex(Vector3 vertex, Vector3 normal, int pastIndex) {

			int index = FindVertex(vertex,pastIndex);
		
			if(index == -1) {
				bufVertices.Add( vertex );
				bufNormals.Add( normal );
				bufUVs.Add (new Vector2 (vertex.x,vertex.z)*0.01f);
				index = bufVertices.Count-1;
			} else {
				Vector3 t = bufNormals[ index ] + normal;
				bufNormals[ index ] = t.normalized;
			}

			return (int) index;
		}


		private Dictionary <int,int> cache = new Dictionary<int,int>();
		private int FindVertex(Vector3 vertex, int pastIndex) {

			if (cache.ContainsKey (pastIndex)) {
				return cache [pastIndex];
			} else {
				cache.Add (pastIndex, bufVertices.Count);
			}

			return -1;
		}


		public void Push(float distance) {
			for(int i=0; i<bufVertices.Count; i++) {
				Vector3 normal = bufNormals[i];
				bufVertices[i] += normal * distance;
			}
		}

		private void ComputeFeatureRanges (GOFeature feature) {

			xRange.x = xRange.y = feature.convertedGeometry [0].x;
			zRange.x = zRange.y = feature.convertedGeometry [0].z;

			foreach (Vector3 v in feature.convertedGeometry) {

				if (v.x > xRange.y)
					xRange.y = v.x;
				if (v.x < xRange.x)
					xRange.x = v.x;

				if (v.z > zRange.y)
					zRange.y = v.z;
				if (v.z < zRange.x)
					zRange.x = v.z;
			}
		}
	}


	public class GOTempPolyNew {

		public List<Vector3> vertices = new List<Vector3>(3);
		public List<int> indices = new List<int>(3);

		public Vector3 normal;

		public Vector2 xRange = Vector2.zero;
		public Vector2 zRange = Vector2.zero; 

		public GOTempPolyNew(params Vector3[] vts) {
			vertices.AddRange( vts );
		}


		public GOTempPolyNew WrapPolygon (Vector3[] convertedGeometry, GOMesh terrainMesh) {

			bool[] positive = new bool[vertices.Count];
			int positiveCount = 0;

			for (int i = 0; i < vertices.Count; i++) {
				positive [i] = ContainsPoint2D (convertedGeometry, vertices[i]);
				if (positive [i])
					positiveCount++;
			}

			if (positiveCount == 0) {
				return null; // Fully outside the shape
			} 

			return this; // Return all polygon that are partially inside the shape

		}

		public bool ContainsPoint2D (Vector3[] polyPoints, Vector3 p) { 

			if (p.x > xRange.y || p.x < xRange.x || p.z > zRange.y || p.z < zRange.x)
				return false;

//			Profiler.BeginSample("Contains point");
			var j = polyPoints.Length-1; 
			var inside = false; 
			for (int i = 0; i < polyPoints.Length; j = i++) { 

				if (
					((polyPoints [i].z <= p.z && p.z < polyPoints [j].z) || (polyPoints [j].z <= p.z && p.z < polyPoints [i].z))
					&&
					(p.x < (polyPoints [j].x - polyPoints [i].x) * (p.z - polyPoints [i].z) / (polyPoints [j].z - polyPoints [i].z) + polyPoints [i].x)
				) 
				
				{
					inside = !inside;
				}
			} 
//			Profiler.EndSample ();
			return inside; 
		}
	}

}