using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {

	public class GODropFeatures : MonoBehaviour {

		public GOMap goMap;

		public Material testLineMaterial;
		public Material testPolygonMaterial;
        public GOUVMappingStyle uvMappingStyle = GOUVMappingStyle.TopFitSidesRatio;
		// Use this for initialization
		IEnumerator Start () {

			//Wait for the location manager to have the world origin set.
			yield return StartCoroutine (goMap.locationManager.WaitForOriginSet ());

			//[NOTE THAT THIS EXAMPLE ONLY WORKS IN "PARIS" demo scene] 

			//Drop a point on the map
			dropTestPin();

			//Drop a line
			dropTestLine();

			//Drop a polygon
			dropTestPolygon();



		}

		void dropTestPin() {
		
			//1) create game object (you can instantiate a prefab instead)
			GameObject aBigRedSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			aBigRedSphere.transform.localScale = new Vector3 (10, 10, 10);
			aBigRedSphere.GetComponent<MeshRenderer> ().material.color = Color.red;


			//2) make a Coordinate class with your desired latitude longitude
			Coordinates coordinates = new Coordinates (48.87496f,2.29407f);

			//3) call drop pin passing the coordinates and your gameobject
			goMap.dropPin(coordinates,aBigRedSphere);
		
		}

		void dropTestLine() {

			//1) Create a list of coordinates that will represent the polyline
			List <Coordinates> polyline = new List<Coordinates> ();
			polyline.Add(new Coordinates (48.8741683959961,2.29363322257996));  
			polyline.Add(new Coordinates (48.8740196228027,2.2941370010376)); 
			polyline.Add(new Coordinates (48.8737373352051,2.2940993309021)); 
			polyline.Add(new Coordinates (48.8734474182129,2.2942488193512)); 
			polyline.Add(new Coordinates (48.8732261657715,2.29462647438049)); 
			polyline.Add(new Coordinates (48.8731727600098,2.29515218734741)); 
			polyline.Add (new Coordinates (48.8723258972168, 2.29526400566101));

			//2) Set line width
			float width = 3;

			//3) Set the line height
			float height = 2;

			//4) Choose a material for the line (this time we link the material from the inspector)
			Material material = testLineMaterial;

			//5) call drop line
            goMap.dropLine(polyline,width,height,material,uvMappingStyle);
				
		}

		void dropTestPolygon() {

			//Drop polygon is very similar to the drop line example, just make sure the coordinates will form a closed shape. 

			//1) Create a list of coordinates that will represent the polygon
			List <Coordinates> shape = new List<Coordinates> ();
			shape.Add(new Coordinates (48.8744621276855,2.29504323005676)); 
			shape.Add(new Coordinates (48.8744010925293,2.29542183876038)); 
			shape.Add(new Coordinates (48.8747596740723,2.29568862915039 )); 
			shape.Add(new Coordinates (48.8748931884766,2.29534268379211)); 
			shape.Add(new Coordinates (48.8748245239258,2.29496765136719)); 

			//2) Set the line height
			float height = 20;

			//3) Choose a material for the line (this time we link the material from the inspector)
			Material material = testPolygonMaterial;

			//4) call drop line
            goMap.dropPolygon(shape,height,material,uvMappingStyle);

		}

	}
}

