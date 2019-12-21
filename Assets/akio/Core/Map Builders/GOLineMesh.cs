using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GoShared;

namespace GoMap {

	public class GOLineMesh {

		public bool isLoop = false;
		public bool smoothEdges = false;

		public float width;
		public Mesh mesh;

		private List<Vector3> geometry;
		private Vector3[] vertices = new Vector3[0];
		private int[] triangles = new int[0];
		private Vector2[] uvs = new Vector2[0];

		private MeshFilter filter;

		GOFeature goFeature;

		#region CONSTRUCTOR

		public GOLineMesh (List<Vector3> geometry_,  bool curved = false) {

			this.isLoop = geometry_ [0].Equals (geometry_[geometry_.Count-1]);

			if (curved && !isLoop)
				geometry = GOCurver.MakeBetterCurve (geometry_,10,isLoop);
			else geometry = geometry_;
		}

		public GOLineMesh (GOFeature goFeature_, bool curved = false) {
			
			goFeature = goFeature_;

			this.isLoop = goFeature.convertedGeometry [0].Equals (goFeature.convertedGeometry[goFeature.convertedGeometry.Count-1]);

			if ((goFeature.goTile.useElevation || curved) && goFeature.layer.layerType == GOLayer.GOLayerType.Roads) {
				geometry = GOCurver.MakeBetterCurve (goFeature.convertedGeometry,15,isLoop);
			} else {
				geometry = goFeature.convertedGeometry;
			}


		}

		#endregion

		#region LOADERS

		public void load (GameObject go) {

			filter = go.GetComponent<MeshFilter> ();
			if (filter == null) {
				filter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
			}

			MeshRenderer renderer = go.GetComponent<MeshRenderer> ();
			if (renderer == null) {
				renderer = (MeshRenderer)go.AddComponent(typeof(MeshRenderer));
			}

			UpdateVertices();

			filter.sharedMesh = CreateMesh();
		}

		public Mesh CreateMesh()
		{

			mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uvs;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}

		public GOMesh CreatePremesh () {

			UpdateVertices();

			GOMesh premesh = new GOMesh ();
			premesh.vertices = vertices;
			premesh.triangles = triangles;
			premesh.uv = uvs;

			if (!isLoop && goFeature != null) { 
				premesh = addEdge (premesh, geometry [geometry.Count - 1],true);
				premesh = addEdge (premesh, geometry [0],false);
			}


			return premesh;
		}

		#endregion

		#region BUILDERS

		private void UpdateVertices() {
			
			if (geometry.Count < 2) return; // minimum to make a line

			int count = geometry.Count - 1;

//			isLoop = geometry [0].Equals (geometry [count]);

			vertices = new Vector3[count*4];
			triangles = new int[(geometry.Count-1)*6];
			uvs = new Vector2[count*4];

			List <Vector3> dirs = new List<Vector3> ();
			List <Vector3> tans = new List<Vector3> ();
	 
			Vector3 tanVect = Vector3.down;

			for (int p = 0; p<geometry.Count; p++)
			{
				Vector3 dir;
				Vector3 tangent;

				if (p == 0) // First 
				{
					if (isLoop) {
						dir = (geometry[p+1] - geometry[p]).normalized; 
						Vector3 dirBefore = (geometry [p] - geometry [geometry.Count-2]).normalized;
						tangent =  Vector3.Cross( tanVect,(dirBefore + dir) * 0.5f ).normalized;
					} 
					else {
						dir = (geometry[p+1] - geometry[p]).normalized; 
						tangent = Vector3.Cross( tanVect, dir).normalized;
					}
				}

				else if (p != geometry.Count-1) // Middles
				{
					dir = (geometry[p+1] - geometry[p]).normalized; 
					Vector3 dirBefore = (geometry [p] - geometry [p-1]).normalized;
					tangent =  Vector3.Cross( tanVect,(dirBefore + dir) * 0.5f ).normalized;
				}

				else // Last
				{
					if (isLoop) {
						dir = (geometry[1] - geometry[p]).normalized; 
						Vector3 dirBefore = (geometry [p] - geometry [p-1]).normalized;
						tangent =  Vector3.Cross( tanVect,(dirBefore + dir) * 0.5f ).normalized;

					} else {
						dir = (geometry [p] - geometry [p-1]).normalized;
						tangent = Vector3.Cross( tanVect, dir).normalized;
					}

				}

				dirs.Add (dir);
				tans.Add (tangent);

			}


			float dmax = 10;
			float dcurr = 0;

            for (int i = 0; i < count; i++) //Loop the segments
            {

                int index = i * 4;

                if (goFeature != null)
                {
                    vertices[index + 0] = goFeature.goTile.altitudeToPoint(geometry[i] + (tans[i] * (width)));
                    vertices[index + 1] = goFeature.goTile.altitudeToPoint(geometry[i] - (tans[i] * (width)));
                    vertices[index + 2] = goFeature.goTile.altitudeToPoint(geometry[i + 1] + (tans[i + 1] * (width)));
                    vertices[index + 3] = goFeature.goTile.altitudeToPoint(geometry[i + 1] - (tans[i + 1] * (width)));
                }
                else
                {
                    vertices[index + 0] = geometry[i] + (tans[i] * (width));
                    vertices[index + 1] = geometry[i] - (tans[i] * (width));
                    vertices[index + 2] = geometry[i + 1] + (tans[i + 1] * (width));
                    vertices[index + 3] = geometry[i + 1] - (tans[i + 1] * (width));
                }
                //				}

                Vector2 offsetRight = new Vector2(1, Vector3.Distance(vertices[index + 1], vertices[index + 3])); // Green - Blue
                Vector2 offsetLeft = new Vector2(1, Vector3.Distance(vertices[index + 0], vertices[index + 2]));    // Red _ Yellow
                float d = Mathf.Max(offsetRight.y, offsetLeft.y);

                //if (d > dmax) {
                //	uvs [index + 0] = Vector2.zero;
                //	uvs [index + 1] = Vector2.right;
                //} else {
                //	uvs [index + 0] = new Vector2 (0, dcurr);
                //	uvs [index + 1] = new Vector2 (1, dcurr);
                //}

                //if (d + dcurr > dmax) {
                //	uvs [index + 2] = Vector2.up;
                //	uvs [index + 3] = Vector2.one;
                //	dcurr = 0;
                //} else {
                //	uvs [index + 2] = new Vector2 (0, offsetLeft.y + dcurr);//     Vector2.Scale (Vector2.up, offsetLeft); 
                //	uvs [index + 3] = new Vector2 (1, offsetRight.y + dcurr);//     Vector2.Scale (Vector2.one, offsetRight);
                //	dcurr += d;
                //}


                float start = dcurr / (width*2);
                float end = (dcurr + d) / (width * 2);

                uvs[index + 0] = new Vector2(0,start);
                uvs [index + 1] = new Vector2(1,start);
                uvs[index + 2] = new Vector2(0, end); 
                uvs [index + 3] = new Vector2(1, end);
                dcurr += d;
                

                //uvs[index + 0] = new Vector2(0,0);
                //uvs [index + 1] = new Vector2(1,0);
                //uvs[index + 2] = new Vector2(0, 1); // Vector2.Scale (Vector2.up, offsetLeft); //texture stretch?
                //uvs [index + 3] = new Vector2(1, 1); //Vector2.Scale(Vector2.one , offsetRight) ;


				triangles[(i*6)+0] = index+0;
				triangles[(i*6)+1] = index+2;
				triangles[(i*6)+2] = index+1;
				triangles[(i*6)+3] = index+2;
				triangles[(i*6)+4] = index+3;
				triangles[(i*6)+5] = index+1;

			}

			if (filter) {
				for (int i = 0; i < vertices.Length; i++) {
					vertices [i] = filter.transform.InverseTransformPoint (vertices [i]);
				}
			}

		}
			
		#endregion

		#region Utils



		private Vector3 lerpByDistance(Vector3 A, Vector3 B, float x)
		{
			Vector3 P = x * Vector3.Normalize(B - A) + A;
			return P;
		}

		#endregion

		#region edges

		public GOMesh addEdge (GOMesh mesh, Vector3 center, bool flip) {

			List<Vector3> vertices = mesh.vertices.ToList();
			List<int> roadTriangles = mesh.triangles.ToList();
			List<Vector2> uvs = mesh.uv.ToList ();

//			Vector3 center = lastSegment.center;

			Vector3 normal = Vector3.down;//lastSegment.getNormal();

			center = goFeature.goTile.altitudeToPoint (center);

			int edgeResolution = 10; // odd number only
			int vertsCount = mesh.vertices.Count();
			vertices.Add (center);

			int indexOfCenter = vertices.IndexOf (center);

			Vector3 tangent;
			if (!flip)
				tangent = (geometry[1] - geometry[0]).normalized;
			else tangent = (geometry [geometry.Count-1] - geometry [geometry.Count-2]).normalized;

			List<Vector3> points = FindPoints (center, tangent, normal, width, edgeResolution, flip);
			vertices.AddRange (points);

			for (int i = 1; i <= points.Count; i++) {

				roadTriangles.Add(i+vertsCount);
				roadTriangles.Add(i+vertsCount-1);
				roadTriangles.Add(indexOfCenter);
			}

			uvs.Add (new Vector2(0.5f,0.5f));
			for (int i = 0; i < edgeResolution; i++) {

				float n = i == edgeResolution-1?  i/(float)edgeResolution : i-1/(float)edgeResolution;

				float u =Mathf.Sin(180*n * Mathf.Deg2Rad)/edgeResolution;
				float v = Mathf.Cos(180*n * Mathf.Deg2Rad)/2;

				uvs.Add (new Vector2(u,v));
			}

			mesh.vertices = vertices.ToArray ();
			mesh.triangles = roadTriangles.ToArray ();
			mesh.uv = uvs.ToArray();

			return mesh;
		}

		List<Vector3> FindPoints(Vector3 center, Vector3 tangent, Vector3 normal, float radius, int numberOFPoints, bool flip, bool debug = false) {

			List<Vector3> points = new List<Vector3> ();
			for (int i = 0; i < numberOFPoints; i++) {

				int f = flip? 90: -90;

				float angle = (180 / (numberOFPoints-1)) * (i) - f;
				Vector3 rotatedTangent = Quaternion.AngleAxis((float)angle, normal) * tangent;
				Vector3 point = center + (radius * rotatedTangent.normalized);
				if (debug)
					Debug.DrawLine (center, point, Color.red);

				points.Add (goFeature.goTile.altitudeToPoint(point));
			}

			return points;
		}

		#endregion


	}
		
}