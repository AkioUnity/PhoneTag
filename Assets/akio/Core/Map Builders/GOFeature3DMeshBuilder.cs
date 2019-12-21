using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoShared;
using UnityEngine.Profiling;

namespace GoMap {

	public class GOFeature3DMeshBuilder {

		private static List<Vector3> bufVertices = new List<Vector3>();
		private static List<Vector3> bufNormals = new List<Vector3>();
		private static List<Vector2> bufTexCoords = new List<Vector2>();
		private static List<int> bufIndices = new List<int>();


		public static void ProjectFeature(GOFeature feature, GOMesh terrainMesh, float maxAngle) {

			Profiler.BeginSample("Project feature");

			Vector3[] vertices = terrainMesh.vertices;
			int[] triangles = terrainMesh.triangles;

			GOTempPoly poly;

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
////
//				if( Vector3.Angle(-Vector3.forward, normal) >= maxAngle ) 
//					continue;
//
				poly = new GOTempPoly( v1 , v2, v3 );

				poly = GOTempPoly.WrapPolygon(poly, feature.convertedGeometry.ToArray(), terrainMesh);
				if(poly == null)
					continue;
				AddPolygon( poly, normal );
			}

			Profiler.EndSample();
		}

		private static void AddPolygon(GOTempPoly poly, Vector3 normal) {

			Profiler.BeginSample("Add polygon");
			int ind1 = AddVertex( poly.vertices[0], normal );
			for(int i=1; i<poly.vertices.Count-1; i++) {
				int ind2 = AddVertex( poly.vertices[i], normal );
				int ind3 = AddVertex( poly.vertices[i+1], normal );

				bufIndices.Add( ind1 );
				bufIndices.Add( ind2 );
				bufIndices.Add( ind3 );
			}
			Profiler.EndSample ();
		}

		private static int AddVertex(Vector3 vertex, Vector3 normal) {
			int index = FindVertex(vertex);
			if(index == -1) {
				bufVertices.Add( vertex );
				bufNormals.Add( normal );
				index = bufVertices.Count-1;
			} else {
				Vector3 t = bufNormals[ index ] + normal;
				bufNormals[ index ] = t.normalized;
			}
			return (int) index;
		}

		private static int FindVertex(Vector3 vertex) {
			for(int i=0; i<bufVertices.Count; i++) {
				if( Vector3.Distance(bufVertices[i], vertex) < 0.01f ) {
					return i;
				}
			}
			return -1;
		}


		public static void Push(float distance) {
			for(int i=0; i<bufVertices.Count; i++) {
				Vector3 normal = bufNormals[i];
				bufVertices[i] += normal * distance;
			}
		}


		public static GOMesh PreloadMesh() {

			if(bufIndices.Count == 0) {
				return null;
			}
			GOMesh mesh = new GOMesh();
			mesh.vertices = bufVertices.ToArray();
			mesh.normals = bufNormals.ToArray();
			mesh.uv = bufTexCoords.ToArray();
			mesh.triangles = bufIndices.ToArray();

			bufVertices.Clear();
			bufNormals.Clear();
			bufTexCoords.Clear();
			bufIndices.Clear();

			return mesh;
		}

	}


	public class GOTempPoly {

		public List<Vector3> vertices = new List<Vector3>(9);

		public GOTempPoly(params Vector3[] vts) {
			vertices.AddRange( vts );
		}


		public static GOTempPoly WrapPolygon (GOTempPoly polygon, Vector3[] convertedGeometry, GOMesh terrainMesh) {

			bool[] positive = new bool[polygon.vertices.Count];
			int positiveCount = 0;

			for(int i = 0; i < polygon.vertices.Count; i++) {
				positive [i] = ContainsPoint2D (convertedGeometry, polygon.vertices[i]);
				if(positive[i]) 
					positiveCount++;
			}

			if (positiveCount == 0) {
				return null; // Fully outside the shape
			} 

			return polygon; // Return all polygon that are partially inside the shape

		}

		public static bool ContainsPoint2D (Vector3[] polyPoints, Vector3 p) { 

			Profiler.BeginSample("Contains point");
			var j = polyPoints.Length-1; 
			var inside = false; 
			for (int i = 0; i < polyPoints.Length; j = i++) { 

				if (((polyPoints [i].z <= p.z && p.z < polyPoints [j].z) || (polyPoints [j].z <= p.z && p.z < polyPoints [i].z)) &&
					(p.x < (polyPoints [j].x - polyPoints [i].x) * (p.z - polyPoints [i].z) / (polyPoints [j].z - polyPoints [i].z) + polyPoints [i].x)) {
					inside = !inside;
				}

			} 
			Profiler.EndSample ();
			return inside; 
		}
	}

}