using System.Collections;
using System.Collections.Generic;
using System;
using GoShared;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;

namespace GoMap {

	[Serializable]
	public class GOTileObj {

		public string name;
		public Coordinates tileCenter;
		public Vector2 tileCoordinates;
		public int zoomLevel;
		public float diagonalLenght;
		public Vector2 tileSize;
		public Vector3 tileOrigin;
		public float worldScale = 1;
		public List<Vector3> vertices;

		public GOStreetnamesSettings streetNames;

		public GoMap.GOMap.GOMapType mapType;
		public GOElevationAPI elevationType;
        
		public enum GOTileStatus  {

			Created,
			Downloaded,
			Loaded,
			Done,
			Error
		}
		public GOTileStatus status = GOTileStatus.Created;
		public List<GOTileData> tileData = new List<GOTileData>();

        [HideInInspector] public string apiKey;
		[HideInInspector] public bool useCache;
		[HideInInspector] public bool addGoFeatureComponents;
		[HideInInspector] public bool useElevation;
		[HideInInspector] public bool useSatelliteBackground;
		[HideInInspector] public bool useStreetnames;
		[HideInInspector] public bool satellite4X;
		[HideInInspector] public bool combineFeatures;
        [HideInInspector] public GONavMeshSettings navmeshSettings;
		 public float elevationMultiplier = 1.5f;
		[HideInInspector] public Vector2 resolution = new Vector2 (50, 50);
        
        [HideInInspector] public GOTileObj topTile;
		[HideInInspector] public GOTileObj leftTile;
		[HideInInspector] public GOTileObj bottomTile;
		[HideInInspector] public GOTileObj rightTile;

		[HideInInspector] public GOMesh goMesh = null;
		public GODEMTexture2D demData = null;
		[HideInInspector] public Vector3 position;
		[HideInInspector] public Quaternion rotation;

		[HideInInspector] public GameObject featurePrototype;
		[HideInInspector] public GameObject streetnamePrototype;

		#region Constructors

		public GOTileObj (Vector2 tileCoordinates, int zoomLevel, GoMap.GOMap.GOMapType mapType, bool useElevation, float elevationMultiplier, Vector2 resolution,
                          bool useCache, bool addGofeatureComponents, float worldScale, bool useSatelliteBackground, bool satellite4X, bool combineFeatures, GONavMeshSettings navmeshSettings) {

			this.tileCoordinates = tileCoordinates;
			this.zoomLevel = zoomLevel;
			this.useElevation = useElevation;
            this.elevationMultiplier = elevationMultiplier *= worldScale;
			this.resolution = resolution;
			this.useCache = useCache;
			this.mapType = mapType;
			this.elevationType = MapTypeToElevationType (mapType);
			this.addGoFeatureComponents = addGofeatureComponents;
			this.worldScale = worldScale;
			this.useSatelliteBackground = useSatelliteBackground;
			this.satellite4X = satellite4X;
			this.combineFeatures = combineFeatures;
            this.navmeshSettings = navmeshSettings;

			tileCenter = new Coordinates (tileCoordinates, zoomLevel);
			tileOrigin = tileCenter.tileOrigin (zoomLevel).convertCoordinateToVector ();

			diagonalLenght = tileCenter.diagonalLenght(zoomLevel);

			name = tileCoordinates.x + "-" + tileCoordinates.y + "-" + zoomLevel;

			Vector3[] vs = tileCenter.tileVertices (zoomLevel).ToArray();
			vertices = vs.ToList ();

			float tileHeight = Vector3.Distance (vertices [0], vertices [1]);
			float tileWidth = Vector3.Distance (vertices [1], vertices [2]);
			this.tileSize = new Vector2 (tileWidth, tileHeight);

		}

		public static GOElevationAPI MapTypeToElevationType (GoMap.GOMap.GOMapType mapType) {

			switch (mapType) {
			case GOMap.GOMapType.Mapzen_Legacy:
				return GOElevationAPI.Mapzen;
			case GOMap.GOMapType.Mapbox:
				return GOElevationAPI.Mapbox;
			}

			return GOElevationAPI.Mapzen;
		}

		#endregion

		#region static

		public static string TileNamePrototype (Vector2 tileCoordinates, int zoomLevel) {
			return tileCoordinates.x + "-" + tileCoordinates.y + "-" + zoomLevel;
		}

		#endregion

		#region Terrain

		public Mesh groundMesh () {
			updateVertices ();
			GOMesh goMesh = flatTerrainMesh ();

			if (goMesh == null)
				return null;

			return goMesh.ToMesh ();
		}

		public GOMesh flatTerrainMesh () {

			GOMesh mesh = new GOMesh ();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = new int[] {

				0, 1, 2,
				0, 2, 3
			};
			mesh.uv = new Vector2[] {
				new Vector2 (0,0),
				new Vector2 (0,1),
				new Vector2 (1,1),
				new Vector2 (1,0)
			};

			return mesh;
		}

		public GOMesh elevatedTerrainMesh () {

			GODEMTexture2D terrainTex = getTerrainData ();
			GODEMTexture2D normalTex = getNormalsData ();
		
			GOMesh terrainMesh = new GOMesh ();
			terrainMesh.name = "Elevated terrain - "+name;

			Vector3[] vertices;
			Vector3[] normals;
			Color[] colors;

			Vector2 offset = Vector2.zero;

			vertices = new Vector3[(((int)resolution.x +1) * ((int)resolution.y+1))];

			colors = new Color[vertices.Length];
			normals = new Vector3[vertices.Length];
			Vector2[] uv = new Vector2[vertices.Length];

			float stepSizeWidth = tileSize.x / resolution.x;
			float stepSizeHeight = tileSize.y / resolution.y;

			for (int v = 0, z = 0; z < (int)resolution.y + 1; z++) {
				for (int x = 0; x < (int)resolution.x +1; x++) {

					Color32 c32 = terrainTex.calculateColor (new Vector2 (x, z), offset , resolution);

					float height = terrainTex.ConvertColorToAltitude (c32);
					height = height * elevationMultiplier;

					vertices[v] = new Vector3 (x * stepSizeWidth + offset.x  , height, 	z * stepSizeHeight + offset.y  );
					vertices [v] -= new Vector3 (tileSize.x/2, 0, tileSize.y / 2);

					colors [v] = c32;

					if (normalTex != null) {

						Color32 c32Normal = normalTex.calculateColor (new Vector2 (x, z), offset, resolution);

						normals [v] = new Vector3 (c32Normal.r, c32Normal.g, c32Normal.b);
					} else {
						normals [v] = Vector3.up;
					}


					uv[v] = new Vector2(x/resolution.x, z/resolution.y);

					v++;

				}
			}



			for (int v = 0, z = 0; z < (int)resolution.y + 1; z++) {
				for (int x = 0; x < (int)resolution.x + 1; x++) {

					if (topTile != null && topTile.goMesh != null && z == (int)resolution.y) {
						int pos = x;
						vertices [v] = TrasformVectorReferences (this,topTile,topTile.goMesh.vertices[pos]);

                    } 
					if  (leftTile != null && leftTile.goMesh != null && x == 0) {
						int pos = (z+1) * (int)resolution.x + z ;	
						vertices [v] = TrasformVectorReferences (this,leftTile,leftTile.goMesh.vertices[pos]); 
					}
					if  (rightTile != null && rightTile.goMesh != null && x == (int)resolution.x) {
						int pos = z * ((int)resolution.x + 1) ;
						vertices [v] = TrasformVectorReferences (this,rightTile,rightTile.goMesh.vertices[pos]); 
					} 
					if (bottomTile != null && bottomTile.goMesh != null && z == 0) {
						int pos = bottomTile.goMesh.vertices.Count () - ((int)resolution.x + 1) + x;
						vertices [v] = TrasformVectorReferences (this,bottomTile,bottomTile.goMesh.vertices[pos]); 
					}

					v++;
				}
			}

            int[] triangles = new int[((int)resolution.x)*((int)resolution.y) *2 *3];
			float[] heightCheck = new float [6];
			for (int v = 0,t=0, z = 0; z < (int)resolution.y; z++,t++) {
				for (int x = 0; x < (int)resolution.x; x++,v+=6,t++) {

					triangles [v] = t;
					triangles [v + 1] = t + (int)resolution.x +1;
					triangles [v + 2] = t + 1;
					triangles [v + 3] = t + 1;
					triangles [v + 4] = t + (int)resolution.x +1;
					triangles [v + 5] = t + (int)resolution.x +2;

					//Checks spike map errors
					heightCheck[0] = vertices [triangles [v]].y;
					heightCheck[1] = vertices [triangles [v+1]].y;
					heightCheck[2] = vertices [triangles [v+2]].y;
					heightCheck[3] = vertices [triangles [v+3]].y;
					heightCheck[4] = vertices [triangles [v+4]].y;
					heightCheck[5] = vertices [triangles [v+5]].y;

					float mean = 0.0f;
					float meanAbs = 0.0f;
					float standardDeviation = 0.0f;

					foreach(float height in heightCheck){
						mean += height/ 6.0f;
						meanAbs += Mathf.Abs(height)/ 6.0f;
						standardDeviation += (height * height); 
					}
					standardDeviation = standardDeviation / 6.0f;

					float variance = standardDeviation;
					standardDeviation= Mathf.Sqrt (variance);

					float newMean = 0.0f;
					int meanCount = 0;
					for (int i = 0; i < heightCheck.Count (); i++) {

						if (variance > 100 && Mathf.Abs (heightCheck [i]) > standardDeviation + meanAbs) {
							//	CreateDebugSphere ( vertices[triangles[v+i]] + Vector3.up,"spikes STD" + standardDeviation + "  " + variance,10,null);
						} else {
							meanCount++;
							newMean += heightCheck [i];
						}

					}

					newMean = newMean / meanCount;

					for (int i = 0; i < heightCheck.Count (); i++) {

						if (variance > 200 && Mathf.Abs (heightCheck [i]) > standardDeviation + meanAbs) {
							vertices [triangles [v + i]] = new Vector3 (vertices [triangles [v + i]].x, newMean, vertices [triangles [v + i]].z);
						}
					}
				}
			}


			terrainMesh.vertices = vertices;
			terrainMesh.normals = normals;
			terrainMesh.uv = uv;
			terrainMesh.triangles = triangles;

			goMesh = terrainMesh;

			return goMesh;

		}
	
		static Vector3 TrasformVectorReferences (GOTileObj parent, GOTileObj local, Vector3 point){

//			point = local.position + local.rotation * Vector3.Scale (Vector3.one, point);
//			point = Vector3.Scale(Vector3.one , Quaternion.Inverse (parent.rotation) * (point - parent.position));

			//Simplified version
			point = local.position + point - parent.position;
			return point;
		}

		public void updateVertices() {

			List<Vector3> verts = new List<Vector3> ();
			foreach (Vector3 v in vertices) {
				verts.Add(v - position);
			}
			vertices = verts;
		}

		public float altitudeForPoint (Vector3 point) {
		
			if (!useElevation || demData == null)
				return 0;

			return demData.getAltitude (point, tileOrigin);;
		}

		public Vector3 altitudeToPoint (Vector3 point) {

			if (!useElevation || demData == null)
				return point;

			point = trimVectorToTile (point);

			return new Vector3 (point.x,demData.getAltitude (point, tileOrigin),point.z) ;
		}

//		public Vector3 rayAltitudeToPoint (Vector3 point) {
//
//			Profiler.BeginSample ("Trim vector");
//			Vector3 trimmed = trimVectorToTile (point);
//			Profiler.EndSample();
//			return new Vector3 (trimmed.x,raycastAltitude (trimmed),trimmed.z) ;
//
//		}

		public Vector3 rayAltitudeToPoint (Vector3 point) {

			Profiler.BeginSample ("Trim vector");
			point = trimVectorToTile (point);
			Profiler.EndSample();
			Profiler.BeginSample ("Raycast");
			point.y = raycastAltitude (point);
			Profiler.EndSample ();
			return point;
			//			return new Vector3 (trimmed.x,raycastAltitude (trimmed),trimmed.z) ;

		}
	
		public float raycastAltitude (Vector3 point) {

			RaycastHit hit;
			Ray ray = new Ray (point+ new Vector3(0,10000,0), Vector3.down);
			if (Physics.Raycast (ray, out hit,10000)) {
				return hit.point.y;
			}
			return point.y;
		
		}

		public bool vectorIsInTile (Vector3 v) {

			Vector3 vp = v - position;

			if (vp.x > vertices [2].x || vp.x < vertices [1].x)
				return false;					
			if (vp.z > vertices [1].z || vp.z < vertices [0].z)
				return false;	

			return true;
		}

		public Vector3 trimVectorToTile (Vector3 v) {


			Vector3 vp = v - position;

			vp.x = vp.x > vertices [2].x ? vertices [2].x-0.1f : vp.x;
			vp.x = vp.x < vertices [1].x ? vertices [1].x+0.1f : vp.x;

			vp.z = vp.z > vertices [1].z ? vertices [1].z-0.1f : vp.z;
			vp.z = vp.z < vertices [0].z ? vertices [0].z+0.1f : vp.z;

			vp += position;

			return vp;

		}

		public Vector3 coordinatesToVector (Coordinates coordinates) {

			Vector3 point = coordinates.convertCoordinateToVector ();
			try {
				point.y = altitudeForPoint (point);
			} catch (Exception ex){
				Debug.Log (ex);
			}
			return point;
		}

		#endregion

		#region Load Data

		public bool DownloadComplete () {
		
			foreach (GOTileData d in tileData) {
				if (d.data == null && status != GOTileStatus.Error)
					return false;
			}
			return true;
		}
			

		public void downloadData (MonoBehaviour host, Action completion) {
		
			if (DownloadComplete ()) {
				completion ();
				return;
			}

			foreach (GOTileData data in tileData) {
			
				data.Download (host, (byte[] bytes, string text, string error) => {

					if (String.IsNullOrEmpty (error)) {
						data.data = bytes;
						data.prepareData();

						if (DownloadComplete()) {
							status = GOTileStatus.Downloaded;
							completion();
						}
					} 
					else {
						status = GOTileStatus.Error;
						completion();
					}
				});
			}
		} 

		public IEnumerator downloadData (MonoBehaviour host) {

			if (DownloadComplete ()) {
				yield break;
			}

			foreach (GOTileData data in tileData) {

				data.Download (host, (byte[] bytes, string text, string error) => {

					if (String.IsNullOrEmpty (error)) {
						data.data = bytes;
						data.prepareData();

						if (DownloadComplete()) {
							status = GOTileStatus.Downloaded;
						}
					} 
					else {
						status = GOTileStatus.Error;
					}
				});
			}

			yield return new WaitUntil(() => DownloadComplete());
		} 

		public byte [] getVectorData () {

			foreach (GOTileData data in tileData) {
				if (data.type == GOTileData.GODataType.VectorPBF)
					return data.data;
			}
			return null;
		}

		public IDictionary getJsonData () {

			foreach (GOTileData data in tileData) {
				if (data.type == GOTileData.GODataType.VectorJson)
					return data.jsonData;
			}
			return null;
		}

		public GODEMTexture2D getTerrainData () {

			if (demData != null)
				return demData;

			foreach (GOTileData data in tileData) {
				if (data.type == GOTileData.GODataType.DEM)
					demData = data.textureData;
					return data.textureData;
			}
			return null;
		}

		public GODEMTexture2D getNormalsData () {

			foreach (GOTileData data in tileData) {
				if (data.type == GOTileData.GODataType.Normals)
					return data.textureData;
			}
			return null;
		}

		public GODEMTexture2D getSatelliteData () {


			foreach (GOTileData data in tileData) {
				if (data.type == GOTileData.GODataType.Satellite)
					return data.textureData;
			}
			return null;
		}

		public Texture2D getSatelliteTexture () {
			if (satellite4X) {

				List<GOTileData> dl = new List<GOTileData> ();
				foreach (GOTileData data in tileData) {
					if (data.type == GOTileData.GODataType.Satellite4X)
						dl.Add (data);
				}
				return GOTileData.MergeSatellite2X (dl);

			} else {
				return getSatelliteData ().ToTexture2D ();
			}

		}

		#endregion

		#region Utils

		public void SetupAdiacentTiles (List<GOTile> tiles) {

			Vector2 tileModifier = Vector2.zero;

			for (int i = 0; i<4; i++) {
				switch (i)
				{
				case 0: //Top
					tileModifier = new Vector2 (0, -1);
					break;
				case 1: //Left
					tileModifier = new Vector2 (-1, 0);
					break;
				case 2: //Bottom
					tileModifier = new Vector2 (0, 1);
					break;
				case 3: //Right
					tileModifier = new Vector2 (1, 0);
					break;
				default:
					break;
				}

				GOTileObj goTile = null;
				foreach (GOTile tile in tiles) {
					if (tile.goTile.tileCoordinates == this.tileCoordinates + tileModifier) {
						goTile = tile.goTile;
					}
				}

				switch (i)
				{
				case 0: //Top
					topTile = goTile;
					break;
				case 1: //Left
					leftTile = goTile;
					break;
				case 2: //Bottom
					bottomTile = goTile;
					break;
				case 3: //Right
					rightTile = goTile;
					break;
				default:
					break;
				}
			}
		}

		public Vector2 getZRange () {
			return new Vector2 (vertices [0].z + position.z, vertices [2].z + position.z);
		}
		public Vector2 getXRange () {
			return new Vector2 (vertices [0].x + position.x, vertices [2].x + position.x);
		}

		#endregion

		#region Debug

		public void ShowDebugMarkers () {

			List<Vector3> list = new List<Vector3> ();
			list.AddRange (vertices);
			list.Add (tileCenter.convertCoordinateToVector ());

//			VERTICES CHECK (coordinates)
			foreach (Vector3 vertex in list) {

				//Spheres
				GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.transform.localScale = Vector3.one * 20;
				float h = altitudeForPoint (vertex);
				sphere.transform.position = new Vector3 (vertex.x, h, vertex.z);

			}

//			//Spheres
//			GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			s.transform.localScale = Vector3.one * 30;
//			float hh = altitudeForPoint (tileOrigin - new Vector3 (0,0,tileSize.y));
//			s.transform.position = new Vector3 (tileOrigin.x, hh, tileOrigin.z - tileSize.y);

		}

		#endregion
	}
}