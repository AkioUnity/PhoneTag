using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using GoMap;

namespace GoShared {
    
    public enum GOUVMappingStyle
    {

        TopAndSidesRepeated,
        TopRepeatedSidesStretched,
        TopRepeatedSidesFit,
        TopFitSidesFit,
        TopCenteredSidesFit,
        TopFitSidesRatio,
        TopFitSidesSliced
    }

	public class GOMesh {

		public string name;
		
		public Vector3[] vertices;
		public int[] triangles;
		public Vector2[] uv;
		public Vector3[] normals;
		public Color32[] colors;

		public GOMesh secondaryMesh;
        public GOUVMappingStyle uvMappingStyle = GOUVMappingStyle.TopAndSidesRepeated;

        public Vector3 center;
        public Vector3 direction;
        public float angle;

        public float Y = 0f;

		//Mesh Extrusion info
		public float sliceHeight;
		public float heightOriginal;
		public bool separateTop = false;
		public bool separateBottom = false;
		public int [] topTriangles;
		public int [] bottomTriangles;

		public GOMesh () {}
		public GOMesh (GOMesh premesh) {
		
			vertices = premesh.vertices;
			triangles = premesh.triangles;
			uv = premesh.uv;
		}

		public Mesh ToMesh (bool recalculateNormals_ = true) {

			// Create the mesh
			Mesh msh = new Mesh();
			msh.vertices = vertices;
			msh.triangles = triangles;
			msh.uv = uv;

			if (recalculateNormals_)
				msh.RecalculateNormals();
//			msh.RecalculateBounds();
//			msh.name = name;

			return msh;

		}

		public Mesh ToSubmeshes () {

			if (topTriangles == null)
				return ToMesh ();

			// Create the mesh
			Mesh msh = new Mesh();
			msh.vertices = vertices;
			msh.uv = uv;
//			msh.name = name;

			msh.subMeshCount = 2;

			msh.SetTriangles(triangles,0);
			if (separateTop)
				msh.SetTriangles(topTriangles,1);
			else if (separateBottom)
				msh.SetTriangles(topTriangles,1);

			msh.RecalculateNormals();
//			msh.RecalculateBounds();

			return msh;

		}

		public Mesh ToRoadMesh () {

			// Create the mesh
			Mesh msh = new Mesh();
			msh.vertices = vertices;
			msh.uv = uv;

			msh.subMeshCount = 2;
			msh.SetTriangles(triangles,0);
			msh.SetTriangles(topTriangles,1);

			msh.RecalculateNormals();

			return msh;

		}


        public void SetShapeInfo (List<Vector3> shape) {

            //List<GOSegment>segments = new List<GOSegment>();
            //for (int i = 1; i < shape.Count(); i++) {

            //    GOSegment seg = new GOSegment();
            //    seg.pointA = shape[i-1];
            //    seg.pointB = shape[i];
            //    seg.distance = Vector3.Distance(seg.pointA, seg.pointB);
            //    segments.Add(seg);
            //}
            //segments.Sort((p1, p2) => p1.distance.CompareTo(p2.distance));
            //GOSegment longest = segments[segments.Count() - 1];
            //direction = longest.pointB-longest.pointA;

            center = shape.Aggregate((acc, cur) => acc + cur) / shape.Count;
            direction = shape[1] - shape[0];
            angle = Vector3.Angle(direction, Vector3.right);

            //Debug.DrawLine(longest.pointA+ Vector3.up * 30, longest.pointB + Vector3.up * 30, Color.cyan, 1000);
            //Debug.DrawLine(shape[1]+ Vector3.up * 30, shape[0] + Vector3.up * 30, Color.blue, 1000);
            //Debug.DrawLine(center, center + Vector3.up * 1000, Color.red, 1000);
            //Debug.DrawLine(shape[0], shape[0] + Vector3.up * 1000, Color.yellow, 1000);
        }

        public void ApplyUV (List<Vector3> shape) {

            float xMax = 0, yMax = 0, xMin = 0, yMin = 0, min = 0;
            if (uvMappingStyle == GOUVMappingStyle.TopFitSidesFit ||
                uvMappingStyle == GOUVMappingStyle.TopCenteredSidesFit ||
                uvMappingStyle == GOUVMappingStyle.TopFitSidesRatio ||
                uvMappingStyle == GOUVMappingStyle.TopFitSidesSliced)
            {
                xMax = vertices.Max(v => v.x);
                yMax = vertices.Max(v => v.z);
                xMin = vertices.Min(v => v.x);
                yMin = vertices.Min(v => v.z);
                min = Mathf.Min(xMax, yMax);
                SetShapeInfo(shape);
            }

            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                switch (uvMappingStyle)
                {
                    case GOUVMappingStyle.TopAndSidesRepeated:
                    case GOUVMappingStyle.TopRepeatedSidesStretched:
                    case GOUVMappingStyle.TopRepeatedSidesFit:
                        uvs[i] = new Vector2(vertices[i].x, vertices[i].z) / 100;
                        break;
                    case GOUVMappingStyle.TopFitSidesRatio:
                    case GOUVMappingStyle.TopFitSidesFit:
                    case GOUVMappingStyle.TopFitSidesSliced:
                        Vector3 p = vertices[i];
                        p = GOUtils.RotatePointAroundPivot(p, center, -Vector3.up* angle);
                        uvs[i] = new Vector2((p.x - xMin) / (xMax - xMin), -((p.z - yMin) / (yMax - yMin)));
                        break;
                    case GOUVMappingStyle.TopCenteredSidesFit:

                        Vector3 po = vertices[i];
                        po = GOUtils.RotatePointAroundPivot(po, center, -Vector3.up * angle);
                        float u = (po.x - xMin) / (xMax - xMin);
                        float v = -((po.z - yMin) / (yMax - yMin));
                        uvs[i] = new Vector2(u, v) - new Vector2(((xMax / min) - 1) / 2, ((yMax / min) - 1) / 2);//- new Vector2(0.2f,0);
                        break;
                    default:
                        break;
                }
            }
            uv = uvs;

        }

	}



}