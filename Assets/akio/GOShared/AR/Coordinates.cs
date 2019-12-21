using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace GoShared {
	
	[System.Serializable]
	public class Coordinates {

		public double latitude; 
		public double longitude; 
		public double altitude; 
		[HideInInspector] public double timestampLastUpdate;

		//CONSTRUCTORS

		public Coordinates (double latitude, double longitude){

			this.latitude = latitude;
			this.longitude = longitude;
			this.altitude = 0;
			this.timestampLastUpdate = getCurrentTimestamp();
		}

		public Coordinates (double latitude, double longitude, double altitude){

			this.latitude = latitude;
			this.longitude = longitude;
			this.altitude = altitude;
			this.timestampLastUpdate = getCurrentTimestamp();
		}

		public Coordinates (LocationInfo location){

			this.latitude = location.latitude;
			this.longitude = location.longitude;
			this.altitude = location.altitude;
			this.timestampLastUpdate = getCurrentTimestamp();

		}

		public Coordinates (Vector2 tileCoords,int zoom){

			Vector2 tileCenter = TileToWorldPos (tileCoords.x+0.5, tileCoords.y+0.5, zoom);
			this.latitude = tileCenter.y;
			this.longitude = tileCenter.x;
			this.altitude = 0;
			this.timestampLastUpdate = getCurrentTimestamp();

		}

		public Coordinates (string s) {
		
			Regex.Replace(s, @"\t\n\r ", "");

			string[] splitString = s.Split(new string[] {","}, StringSplitOptions.None);

			double.TryParse (splitString [0], out this.latitude);
			double.TryParse (splitString [1], out this.longitude);
			this.altitude = 0;
			this.timestampLastUpdate = getCurrentTimestamp();

		}

		//CONVERSIONS

		public void updateLocation (LocationInfo location){

			this.latitude = location.latitude;
			this.longitude = location.longitude;
			this.altitude = location.altitude;
			this.timestampLastUpdate = getCurrentTimestamp();

		}
					
		public float gpsAngle(float longitude_o, float latitude_o) { //Given an origin
			
			if (longitude == longitude_o)
				return 0;
			if (longitude-longitude_o < 0) {
				return Mathf.Atan((float)(latitude-latitude_o)/(float)(longitude-longitude_o))+Mathf.PI;
			} else {
				return Mathf.Atan((float)(latitude-latitude_o)/(float)(longitude-longitude_o));
			}
		}

		public float DistanceFromPoint(Coordinates pt)
		{
			Vector3 ptV = pt.convertCoordinateToVector();
			Vector3 thisV = convertCoordinateToVector();
			return Vector3.Distance (ptV, thisV);
		}

		public double DistanceFromOtherGPSCoordinate (Coordinates targetCoordinates)
		{
			var baseRad = Math.PI * latitude / 180;
			var targetRad = Math.PI * targetCoordinates.latitude/ 180;
			var theta = longitude - targetCoordinates.longitude;
			var thetaRad = Math.PI * theta / 180;

			double dist =
				Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
				Math.Cos(targetRad) * Math.Cos(thetaRad);
			dist = Math.Acos(dist);

			dist = dist * 180 / Math.PI;
			dist = dist * 60 * 1.1515;

			return dist*1.609344/1000f;
		}

		public Vector3 convertCoordinateToVector () {
			Vector3 converted = GPSEncoder.GPSToUCS (new Vector2 ((float)latitude, (float)longitude));
			converted.y = (float)altitude;
			return converted;
		}

		public Vector3 convertCoordinateToVector (float y) {
			Vector3 converted = GPSEncoder.GPSToUCS (new Vector2 ((float)latitude, (float)longitude));
			converted.y = y;
			return converted;
		}

		public Vector2 convertCoordinateToVector2D () {
			Vector3 converted = GPSEncoder.GPSToUCS (new Vector2 ((float)latitude, (float)longitude));
			return new Vector2 (converted.x,converted.z);
		}

		public bool isEqualToCoordinate(Coordinates coordinate) {

			return (coordinate.latitude == latitude && coordinate.longitude == longitude);
		}

		public bool Equals(Coordinates coordinate) {

			return (coordinate.latitude == latitude && coordinate.longitude == longitude);
		}

		public bool isZeroCoordinates () {
			return latitude == 0 && longitude == 0 && altitude == 0;
		}

		public double intervalBetweenTimestamps (Coordinates coordinate) {
			return Math.Abs(coordinate.timestampLastUpdate - timestampLastUpdate);
		} 

		////TILES

		public Vector2 tileCoordinates (int zoom) {

			Vector2 tileCoords = WorldToTilePos (longitude,latitude,zoom);
			return tileCoords;
		}

		public Coordinates tileCenter (int zoom) {

			Vector2 tileCoords = tileCoordinates (zoom);
			Vector2 tileCenter = TileToWorldPos (tileCoords.x+0.5, tileCoords.y+0.5, zoom);
			return new Coordinates (tileCenter.y,tileCenter.x,0);
		}

		public Coordinates tileOrigin (int zoom) {

			Vector2 tileCoords = tileCoordinates (zoom);
			Vector2 tileCenter = TileToWorldPos (tileCoords.x, tileCoords.y, zoom);
			return new Coordinates (tileCenter.y,tileCenter.x,0);
		}

		public float diagonalLenght (int zoom) {
		
			Coordinates center = tileCenter (zoom);
			Coordinates origin = tileOrigin (zoom);
			return 2*Mathf.Abs(Vector2.Distance (center.convertCoordinateToVector2D(), origin.convertCoordinateToVector2D()));

		}

		public float tileSize (int zoom) {
		
			Vector2 tileCoords = tileCoordinates (zoom);
			Vector3 v0 = GPSEncoder.GPSToUCS (TileToWorldPos (tileCoords.x, tileCoords.y,zoom).flipped());
			Vector3 v1 = GPSEncoder.GPSToUCS (TileToWorldPos (tileCoords.x+1, tileCoords.y, zoom).flipped());

			float size = Mathf.Abs (Vector2.Distance (v0, v1));
			Debug.Log ("Tile size: " + size);
			return size;	
		}

		public List <Vector3> tileVertices (int zoom) {
		
			Vector2 tileCoords = tileCoordinates (zoom);

			Vector2 v0TileCoords = TileToWorldPos (tileCoords.x + 1, tileCoords.y + 1, zoom).flipped ();
			Vector3 v0 = GPSEncoder.GPSToUCS (v0TileCoords);
			Vector3 v1 = GPSEncoder.GPSToUCS (TileToWorldPos (tileCoords.x, tileCoords.y+1, zoom).flipped());
			Vector3 v2 = GPSEncoder.GPSToUCS (TileToWorldPos (tileCoords.x, tileCoords.y, zoom).flipped());
			Vector3 v3 = GPSEncoder.GPSToUCS (TileToWorldPos (tileCoords.x+1, tileCoords.y, zoom).flipped());

	//		Debug.DrawLine (v0, v0 + Vector3.up*100, Color.green,1000);
	//		Debug.DrawLine (v1, v1 + Vector3.up*100, Color.blue,1000);
	//		Debug.DrawLine (v2, v2 + Vector3.up*100, Color.red,1000);
	//		Debug.DrawLine (v3, v3 + Vector3.up*100, Color.yellow,1000);
	//
			return new List<Vector3> {v1, v2, v3, v0};

		}


		public List<Vector2> adiacentNTiles (int zoom, int buffer){

			List <Vector2> adiacentTiles = new List<Vector2> ();
			Vector2 centerTileCoords = tileCoordinates (zoom);

			for (int y = -buffer; y <= buffer; y++) {
				for (int x = -buffer; x <= buffer; x++) {

					adiacentTiles.Add (new Vector2 (centerTileCoords.x + x,centerTileCoords.y + y));
				}
			}
			adiacentTiles.Sort(delegate(Vector2 a, Vector2 b) {

				float distA = Mathf.Abs(Vector2.Distance(centerTileCoords,a));
				float distB = Mathf.Abs(Vector2.Distance(centerTileCoords,b));
				return distA.CompareTo(distB);
			});

			return adiacentTiles;

		}

        public static int tileBufferCount (int tileBuffer) {

            return (int) Mathf.Pow(1f + ((float)tileBuffer * 2f), 2f); 
        }

		public List<Vector2> subTiles (Vector2 tileCoords) {
		
			//2x, 2y     -    2x+1, 2y
			//2x, 2y +1   -   2x+1, 2y +1

			return new List<Vector2> () {
				new Vector2 (tileCoords.x * 2 + 1, tileCoords.y * 2 +1),
				new Vector2 (tileCoords.x * 2, tileCoords.y * 2 +1),
				new Vector2 (tileCoords.x * 2 , tileCoords.y * 2),
				new Vector2 (tileCoords.x * 2 + 1, tileCoords.y * 2)
			};


		}

		////UTILS

		Vector2 WorldToTilePos(double lon, double lat, int zoom)
		{
			Vector2 p = new Vector2();
			p.x = (int)((lon + 180.0) / 360.0 * (1 << zoom));
			p.y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 
				1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

			return p;
		}

		Vector2 TileToWorldPos(double tile_x, double tile_y, int zoom) 
		{
			Vector2 p = new Vector2();
			double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

			p.x = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
			p.y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

			return p;
		}

		public string description () {
			return "lat: "+latitude + ", lon: " + longitude;
		}

		public string toLatLongString() {

			return latitude.ToString () + "," + longitude.ToString ();

		}

        public bool TooFarFromOrigin(LocationInfo locationInfo) {

            double distance = DistanceFromPoint(new Coordinates(Input.location.lastData));
            Debug.Log("Distance from origin: "+distance);
            if (distance > 1000) {
                return true;
            }
            return false;
        }


		////STATIC

		public static Coordinates convertVectorToCoordinates (Vector3 vector) {

			Coordinates coordinates = new Coordinates (0, 0, 0);

			Vector2 latlon = GPSEncoder.USCToGPS (vector);
			coordinates.latitude = latlon.x;
			coordinates.longitude = latlon.y;
			coordinates.altitude = vector.y;
			return coordinates;
		} 

		public static void setWorldOrigin (Coordinates originCoords,float worldScale) {

			GPSEncoder.SetLocalOrigin (new Vector2 ((float)originCoords.latitude,(float)originCoords.longitude),worldScale);
		}

		public static Coordinates getWorldOrigin () {

			Vector2 localOrigin = GPSEncoder.GetLocalOrigin ();
			return new Coordinates (localOrigin.x, localOrigin.y,0);
		}

		public static double getCurrentTimestamp() {

			var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
			var timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
			return timestamp;
		}

	} 

	public static class CoordExtensions
	{
		public static Vector2 ToVector2xz(this Vector3 v)
		{
			return new Vector2(v.x, v.z);
		}

		public static Vector2 flipped(this Vector2 v)
		{
			return new Vector2(v.y, v.x);
		}

		public static Vector3 ToVector3xz(this Vector2 v)
		{
			return new Vector3(v.x, 0,v.y);
		}

		public static Vector3 ToVector3xz(this Vector2 v, float h)
		{
			return new Vector3(v.x, h,v.y);
		}

        public static Vector3 ToVector3xz(this Vector3 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector3 ToProjectedVector3xz(this Vector3 v)
        {
            return Vector3.ProjectOnPlane(v,Vector3.up);
        }
    }
}
