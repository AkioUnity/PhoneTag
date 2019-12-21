using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using UnityEngine.Profiling;
using GoShared;


namespace GoMap
{

    public class GOFeatureMeshBuilder
    {
		GOFeature feature;
		public Mesh mesh;
		public Mesh mesh2D;
		public Vector3 center;
		static System.Random random = new System.Random ();
		public MeshRenderer meshRenderer;

		MeshJob job;
		public GameObject gameObject;

		public GOFeatureMeshBuilder (GOFeature f) {

			feature = f;
			if (feature.goFeatureType == GOFeatureType.Polygon || feature.goFeatureType == GOFeatureType.MultiPolygon)
				center = feature.convertedGeometry.Aggregate((acc, cur) => acc + cur) / feature.convertedGeometry.Count;

		}

		#region Builders

		public GameObject BuildLine(GOLayer layer, GORenderingOptions renderingOptions , GOMap map, GameObject parent)
        {
			if (feature.convertedGeometry.Count == 2 && feature.convertedGeometry[0].Equals(feature.convertedGeometry[1])) {
				return null;
			}

			GameObject line = GameObject.Instantiate (feature.goTile.featurePrototype, parent.transform);

			if (renderingOptions.tag.Length > 0) {
				line.tag = renderingOptions.tag;
			}

			if (renderingOptions.material)
				renderingOptions.material.renderQueue = -(int)feature.sort;
			if (renderingOptions.outlineMaterial)
				renderingOptions.outlineMaterial.renderQueue = -(int)feature.sort;
			
			GOLineMesh lineMesh = new GOLineMesh (feature.convertedGeometry);
			lineMesh.width = renderingOptions.lineWidth;
			lineMesh.load (line);
			mesh = lineMesh.mesh;
			line.GetComponent<Renderer>().material = renderingOptions.material;

			Vector3 position = line.transform.position;
			position.y = feature.y;

			line.transform.position = position;

			if (renderingOptions.outlineMaterial != null) {
				GameObject outline = CreateRoadOutline (line,renderingOptions.outlineMaterial, renderingOptions.lineWidth + layer.defaultRendering.outlineWidth);
				if (layer.useColliders) {
					MeshCollider mc = outline.GetComponent<MeshCollider> ();
					mc.enabled = true;
					mc.sharedMesh = outline.GetComponent<MeshFilter> ().sharedMesh;
				}

				outline.layer = line.layer;
				outline.tag = line.tag;
				
			} else if (layer.useColliders) {
				line.GetComponent<MeshCollider> ().enabled = true;
			}

			return line;
        }

		public GameObject BuildPolygon(GOLayer layer, float height)
		{

			if (feature.convertedGeometry.Count == 2 && feature.convertedGeometry[0].Equals(feature.convertedGeometry[1])) {
				return null;
			}
			List<Vector3> clean = feature.convertedGeometry.Distinct().ToList();

			if (clean == null || clean.Count <= 2)
				return null;

			GameObject polygon = new GameObject();

			Profiler.BeginSample("[GoMap] Start poly2mesh");
			Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
			poly.outside = feature.convertedGeometry;
			if (feature.clips != null ) {
				foreach (IList clipVerts in feature.clips) {
					poly.holes.Add(GOFeature.CoordsToVerts(clipVerts,true));
				}
			}
			Profiler.EndSample ();

			MeshFilter filter = polygon.AddComponent<MeshFilter>();
			meshRenderer = polygon.AddComponent<MeshRenderer>();

			Profiler.BeginSample("[GoMap] Create polygon mesh");
			try {
				mesh = Poly2Mesh.CreateMesh (poly);
			} catch {
				
			}
			Profiler.EndSample ();


			if (mesh) {

				Profiler.BeginSample("[GoMap] Set polygon UV");
				Vector2[] uvs = new Vector2[mesh.vertices.Length];
				Vector3[] vertices = mesh.vertices;
				for (int i=0; i < uvs.Length; i++) {
					uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
				}
				mesh.uv = uvs;
				Profiler.EndSample ();

				Profiler.BeginSample("[GoMap] instantiate mesh 2D");
				mesh2D = Mesh.Instantiate(mesh);
				Profiler.EndSample ();

				Profiler.BeginSample("[GoMap] polygon extrusion");
				if (height > 0) {
					mesh = SimpleExtruder.SliceExtrude (mesh, polygon, height, 4f,4f,10f);
//					mesh = SimpleExtruder.Extrude (mesh, polygon, height);
				}
				Profiler.EndSample();

			}


			filter.sharedMesh = mesh;

			if (layer.useColliders && mesh != null && feature.convertedGeometry.Count() > 2)
				polygon.AddComponent<MeshCollider>().sharedMesh = mesh;


			return polygon;

		}

		#endregion

		#region LINE UTILS

		GameObject CreateRoadOutline (GameObject line, Material material, float width) {

			GameObject outline = new GameObject ("outline");
			outline.transform.parent = line.transform;

			material.renderQueue = -((int)feature.sort-1);

			GOLineMesh lineMesh = new GOLineMesh (feature.convertedGeometry);
			lineMesh.width = width;
			lineMesh.load (outline);

			Vector3 position = outline.transform.position;
			position.y = -0.039f;
			outline.transform.localPosition = position;

			outline.GetComponent<Renderer>().material = material;

			return outline;
		}


		#endregion

		#region POLYGON UTILS

		public static string VectorListToString (List<Vector3> list) {

			list =  new HashSet<Vector3>(list).ToList();
			string s = "";
			foreach (Vector3 v in list) {
				s += v.ToString() + " ";
			}
			return s;

		}

		#endregion

		#region PreloadedData 

		public static GOMesh PreloadFeatureData (GOFeature feature) {

			try {
				switch (feature.goFeatureType) {
				case GOFeatureType.Polygon:
				case GOFeatureType.MultiPolygon:
					if (!feature.layer.isPolygon)
						return PreloadLine (feature);
					else if (feature.goTile.useElevation && feature.layer.layerType != GOLayer.GOLayerType.Buildings)
						return Preload3DPolygon (feature);
					else return PreloadPolygon (feature);
				case GOFeatureType.Line:
				case GOFeatureType.MultiLine:
					return PreloadLine (feature);
					default:
					return null;
				}
			} catch (Exception ex) {
				Debug.LogWarning ("[GOMAP] error catched in feature: "+feature.name+", "+feature.kind + ", "+feature.convertedGeometry + ", " +ex);
				return null;
			}
		}

		public static GOMesh PreloadPolygon (GOFeature feature) {

			if (feature.convertedGeometry == null)
				return null;
			
			if (feature.convertedGeometry.Count == 2 && feature.convertedGeometry[0].Equals(feature.convertedGeometry[1])) {
				return null;
			}

			List<Vector3> clean = feature.convertedGeometry.Distinct().ToList();

			if (clean == null || clean.Count <= 2)
				return null;


			Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
			poly.outside = feature.convertedGeometry;
			if (feature.clips != null ) {
				foreach (List<Vector3> clipVerts in feature.clips) {
					poly.holes.Add(clipVerts);
				}
			}

			GOMesh goMesh = null;
            goMesh = Poly2Mesh.CreateMeshInBackground (poly);

            if (goMesh != null) {

                goMesh.uvMappingStyle = feature.layer.uvMappingStyle;
                goMesh.ApplyUV(feature.convertedGeometry);
                goMesh.Y = feature.y;

				if (feature.goTile.useElevation)
					feature.ComputeHighestAltitude ();

				if (feature.height > 0) {

					feature.height *= feature.goTile.worldScale;
                    goMesh.secondaryMesh = new GOMesh (goMesh);

					float h = feature.height;

					if (feature.goTile.useElevation)
						h += GOFeature.BuildingElevationOffset;

					h += Noise ();
                    goMesh.separateTop = feature.renderingOptions.hasRoof;

                    if (feature.layer.slicedExtrusion)
                        goMesh = SimpleExtruder.SliceExtrudePremesh (goMesh,h, 4f,4f,10f*feature.goTile.worldScale);
                    else
                        goMesh = SimpleExtruder.ExtrudePremesh(goMesh, h);


                }

                if (feature.height < feature.layer.colliderHeight)
                {
                    float h = feature.layer.colliderHeight;
                    h *= feature.goTile.worldScale;
                    if (feature.goTile.useElevation)
                        h += GOFeature.BuildingElevationOffset;

                    goMesh.secondaryMesh = new GOMesh(goMesh);
                    goMesh.secondaryMesh = SimpleExtruder.SliceExtrudePremesh(goMesh.secondaryMesh, h, 4f, 4f, 10f * feature.goTile.worldScale);
                }
			}

            return goMesh;
		}

		public static GOMesh PreloadLine (GOFeature feature) {


			if (feature.convertedGeometry.Count == 2 && feature.convertedGeometry[0].Equals(feature.convertedGeometry[1])) {
				return null;
			}

			GOMesh preMesh = new GOMesh ();

			GOLineMesh lineMesh = new GOLineMesh (feature,true);
			lineMesh.width = feature.renderingOptions.lineWidth * feature.goTile.worldScale;
			preMesh = lineMesh.CreatePremesh();
			feature.isLoop = lineMesh.isLoop;
//			GORoad road = new GORoad (feature, 25, 1, 0);
//			road.computeRoad ();
//			preMesh = road.goMesh;

			if (feature.goTile.useElevation && feature.height == 0)
				feature.height += GOFeature.RoadsHeightForElevation;

			if (feature.height > 0) {
				float h = feature.height * feature.goTile.worldScale;
				if (feature.goTile.useElevation)
					h += GOFeature.BuildingElevationOffset;
				preMesh = SimpleExtruder.ExtrudePremesh (preMesh, h + Noise(),false);
			}

			if (feature.renderingOptions.outlineWidth > 0 && !feature.goTile.useElevation) {
				lineMesh.width = (feature.renderingOptions.lineWidth + feature.layer.defaultRendering.outlineWidth) * feature.goTile.worldScale;
				preMesh.secondaryMesh = lineMesh.CreatePremesh();

				if (feature.height > 0) {
					float h = feature.height * feature.goTile.worldScale;
					if (feature.goTile.useElevation)
						h += GOFeature.BuildingElevationOffset;
					preMesh.secondaryMesh = SimpleExtruder.ExtrudePremesh (preMesh.secondaryMesh, h + Noise(),false);
				}

			}

			return preMesh;
		}

		public static GOMesh Preload3DPolygon (GOFeature feature) {

			if (feature.convertedGeometry == null)
				return null;

			if (feature.convertedGeometry.Count == 2 && feature.convertedGeometry[0].Equals(feature.convertedGeometry[1])) {
				return null;
			}

			List<Vector3> clean = feature.convertedGeometry.Distinct().ToList();

			if (clean == null || clean.Count <= 2)
				return null;

			GOFeature3DMeshBuilderNew b = new GOFeature3DMeshBuilderNew ();
			GOMesh preMesh = b.ProjectFeature(feature, feature.goTile.goMesh, 0);
		
			return preMesh;
		}

		public static float Noise() {
			double r = random.NextDouble ();
			return ((float)r/10f);
		}

			
		#endregion

		#region New Builders

		public GameObject BuildLineFromPreloaded(GOFeature feature, GOMap map, GameObject parent)
		{

			if (feature.preloadedMeshData == null)
				return null;

			GameObject line = GameObject.Instantiate (feature.goTile.featurePrototype,parent.transform);

			GORenderingOptions renderingOptions = feature.renderingOptions;

			if (renderingOptions.tag.Length > 0) {
				line.tag = renderingOptions.tag;
			}

			if (renderingOptions.material)
				renderingOptions.material.renderQueue = -(int)feature.sort;
			if (renderingOptions.outlineMaterial)
				renderingOptions.outlineMaterial.renderQueue = -(int)feature.sort;


			MeshFilter filter = line.GetComponent<MeshFilter> ();
			MeshRenderer renderer = line.GetComponent<MeshRenderer> ();
			renderer.shadowCastingMode = feature.layer.castShadows;

			filter.sharedMesh = feature.preloadedMeshData.ToMesh();
			renderer.material = renderingOptions.material;

//			filter.sharedMesh = feature.preloadedMeshData.ToRoadMesh();
//			renderer.materials = new Material[] {renderingOptions.material,renderingOptions.outlineMaterial};
				
			Vector3 position = line.transform.position;
			position.y = feature.y;
			position.y += Noise ();
			position.y *= feature.goTile.worldScale;

			if (feature.goTile.useElevation) {
				position.y -= GOFeature.BuildingElevationOffset;
			}

			line.transform.position = position;

			if (renderingOptions.outlineMaterial != null && feature.preloadedMeshData != null && feature.preloadedMeshData.secondaryMesh != null) {
				GameObject outline = RoadOutlineFromPreloaded (line, feature, renderingOptions.outlineMaterial);
				if (feature.layer.useColliders) {
					MeshCollider mc = outline.GetComponent<MeshCollider> ();
					mc.enabled = true;
					mc.sharedMesh = outline.GetComponent<MeshFilter> ().sharedMesh;
				}

				outline.layer = line.layer;
				outline.tag = line.tag;

			} else if (feature.layer.useColliders) {
				MeshCollider collider = line.GetComponent<MeshCollider> ();
				collider.enabled = true;
				collider.sharedMesh = filter.sharedMesh;
			}

			return line;
		}

		public GameObject BuildPolygonFromPreloaded(GOFeature feature, GameObject parent)
		{

			if (feature.preloadedMeshData == null)
				return null;

			GameObject polygon = GameObject.Instantiate(feature.goTile.featurePrototype,parent.transform);

			meshRenderer = polygon.GetComponent<MeshRenderer>();

			Mesh mesh = null;

			Profiler.BeginSample ("Load preloaded mesh data");
            if (feature.renderingOptions.hasRoof){ //&& feature.roofMat != null) {
				mesh = feature.preloadedMeshData.ToSubmeshes ();
			} else mesh = feature.preloadedMeshData.ToMesh ();
			Profiler.EndSample ();

//			if (feature.layer.layerType == GOLayer.GOLayerType.Buildings && feature.height > 0 && feature.layer.useRealHeight)
//				SimpleExtruder.FixUV (mesh, feature.preloadedMeshData.sliceHeight,10f);
			
			polygon.GetComponent<MeshFilter>().sharedMesh = mesh;

			if (feature.layer.useColliders && mesh != null && feature.convertedGeometry.Count () > 2) {

				MeshCollider collider = polygon.GetComponent<MeshCollider> ();
				collider.enabled = true;
				collider.sharedMesh = mesh;

                if (feature.height < feature.layer.colliderHeight)
                {

                    //GameObject colliderGameObject = new GameObject("Collider");
                    //colliderGameObject.transform.parent = polygon.transform;
                    //MeshCollider collider2 = colliderGameObject.AddComponent<MeshCollider>();
                    //collider2.enabled = true;
                    //collider2.sharedMesh = feature.preloadedMeshData.secondaryMesh.ToMesh();
                    //MeshCollider.Destroy(collider);

                    collider.sharedMesh = feature.preloadedMeshData.secondaryMesh.ToMesh();
                }
			}
				

			return polygon;

		}

		GameObject RoadOutlineFromPreloaded (GameObject line, GOFeature feature, Material material) {

			GameObject outline = GameObject.Instantiate (feature.goTile.featurePrototype, line.transform);

			material.renderQueue = -((int)feature.sort-1);

			MeshFilter filter = outline.GetComponent<MeshFilter>();
			MeshRenderer renderer = outline.GetComponent<MeshRenderer> ();
			renderer.shadowCastingMode = feature.layer.castShadows;
			renderer.material = material;
			filter.sharedMesh = feature.preloadedMeshData.secondaryMesh.ToMesh();

			Vector3 position = outline.transform.position;
			position.y = -0.1f * feature.goTile.worldScale;
//			position.y = -0.039f;

			outline.transform.localPosition = position;

			return outline;
		}

		public GameObject CreateRoofFromPreloaded (GOFeature feature, GOMesh premesh, GameObject parent){
			GameObject roof = GameObject.Instantiate(feature.goTile.featurePrototype,parent.transform);
			MeshFilter filter = roof.GetComponent<MeshFilter>();
			filter.mesh = premesh.ToMesh();
			return roof;
		}


		#endregion


    }




}
