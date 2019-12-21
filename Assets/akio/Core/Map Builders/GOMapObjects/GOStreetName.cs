using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Profiling;

namespace GoMap {
	


	public class GOStreetName : MonoBehaviour {


		public float roadLenght;
		public float textLenght;
		public Vector3 rot;

		public GOStreetNameRotationStyle rotationStyle = GOStreetNameRotationStyle.Fixed;
		public enum GOStreetNameRotationStyle  {

			Fixed,
			Continuous,
			Hybrid
		}

		public IEnumerator Build (string name, GOFeature feature) {

			Profiler.BeginSample ("[GOStreetName] text mesh settings");
			TextMesh textMesh = gameObject.GetComponent<TextMesh> ();
			if (IsHebrew (name.ToCharArray()[0])) {
				textMesh.text = GOStreetName.Reverse (name);
			} else {
				textMesh.text = name;
			}	
				
			GOStreetnamesSettings settings;
			switch (feature.goTile.mapType) {
			case GOMap.GOMapType.Mapzen_Legacy:
				settings = feature.goTile.streetNames;
				break;
			default:
				settings = feature.labelsLayer.streetNames;
				break;
			}

			textMesh.color = settings.streetnameColor; //new Color (settings.streetnameColor.r,color.g,color.b);
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.alignment = TextAlignment.Center;

			textMesh.fontStyle = settings.fontStyle;
			textMesh.color = settings.streetnameColor;
			float minimumFontSize = settings.minFontSize;
			textMesh.font = settings.streetNameFont != null? settings.streetNameFont : textMesh.font;
			textMesh.characterSize = settings.characterSize;
			textMesh.fontSize = settings.fontSize;


			MeshRenderer renderer = GetComponent<MeshRenderer> ();
//			renderer.shadowCastingMode = feature.layer.castShadows;
			textLenght = renderer.bounds.size.x;
			renderer.material = textMesh.font.material;
			Profiler.EndSample ();

			if (feature.convertedGeometry.Count > 1) {
				
				Profiler.BeginSample ("[GOStreetName] find middle point");
				GOSegment segment = feature.preloadedLabelData; //GOSegment.FindTheLongestStreightSegment (feature.convertedGeometry, 0);
				transform.localPosition = segment.findMiddlePoint (0.04f); //LineCenter (road._verts); 
				transform.localScale = Vector3.one * 3;
				Profiler.EndSample ();

				Profiler.BeginSample ("[GOStreetName] find correct size");
				//Find correct size
				for (int i = textMesh.fontSize; i >= minimumFontSize - 1; i--) {
					textMesh.fontSize = i;
					float tl = renderer.bounds.size.x;
					if (segment.distance >= tl) {
						break;
					}
					if (i == minimumFontSize - 1) {
						GameObject.DestroyImmediate (this.gameObject);
						yield break;
					}
				}
				Profiler.EndSample ();	

				var rotation = transform.eulerAngles;
				rotation.x = 90;

				Vector3 targetDir = segment.direction ();
				if (targetDir.Equals (Vector3.zero)) {
					rotation.y = 90;
				} else {
					Quaternion finalRotation = Quaternion.LookRotation (targetDir);
					rotation.y = finalRotation.eulerAngles.y + 90;

					rotation.y = (rotation.y % 360 + 360) % 360;

					if (rotation.y > 90 && rotation.y < 180) {
						rotation.y -= 180;
					} else if (rotation.y > 0 && rotation.y < 90) {
						rotation.y += 180;
					} 
				}
				rot = rotation;
				transform.eulerAngles = rotation;

			} else if (feature.convertedGeometry.Count == 1){
				transform.localPosition = feature.convertedGeometry [0];
				transform.localScale = Vector3.one * 3;
				var rotation = transform.eulerAngles;
				rotation.x = 90;
				transform.eulerAngles = rotation;
			}


			if (settings.textShader != null) {
				Material m = renderer.sharedMaterial;
				m.shader = settings.textShader;
				m.color = textMesh.color;
			}


			yield return null;

		}

		//Update rotation with main camera position
//		void Update () {
//
//			switch (rotationStyle) {
//			case GOStreetNameRotationStyle.Continuous:
//
//				Vector3 pos = transform.position;
//				pos.y = 5;
//				transform.position = pos;
//				transform.LookAt (2* transform.position - Camera.main.transform.position);
//
//				break;
//
//			case GOStreetNameRotationStyle.Hybrid:
//				
//				Vector3 r = transform.eulerAngles;
//
//				Vector3 dir1 = transform.position - (transform.right * 50) - (transform.position + (transform.right * 50));
//				Vector3 dir2 = Vector3.ProjectOnPlane (Camera.main.transform.position, Vector3.up) - transform.position;
//				float signedAngle = Vector3.SignedAngle (dir1, dir2, Vector3.up);
////				Debug.DrawLine (Vector3.ProjectOnPlane (Camera.main.transform.position, Vector3.up), transform.position, Color.red, 0.1f);
////				Debug.DrawLine (transform.position - (transform.right * 50), transform.position + (transform.right * 50), Color.blue, 0.1f);
//				if (signedAngle > 0)
//					r.y -= 180;
//					
//				transform.eulerAngles = r;
//
//				break;
//			default: break;
//			}
//		}



		public static string Reverse(string s)
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		private const char FirstHebChar = (char)1488; //א
		private const char LastHebChar = (char)1514; //ת
		public static bool IsHebrew(char c)
		{
			return c >= FirstHebChar && c <= LastHebChar;
		}

	}


	public class GOSegment {

		public Vector3 pointA;
		public Vector3 pointB;
		public float distance;
		public float angle;

        public static int SortByDistance(GOSegment o1, GOSegment o2)
        {
            return o1.distance.CompareTo(o2.distance);
        }

		public static GOSegment FindTheLongestStreightSegment(List<Vector3> line, float maxAngle) {

			Vector3 pointA = Vector3.zero;
			Vector3 pointB = Vector3.zero;
			float d = 0;
			float angle = 0;

			GOSegment spare = null;

			if (line.Count == 0) {
			
			}


			for (int i = 1; i < line.Count; i++) {

				if (i == 1) {
					pointA = line [0];
					pointB = line [1];
					d = Vector3.Distance (pointA, pointB);
					angle = AngleBetweenVector2XZ (pointA, pointB);
					continue;
				}

				Vector3 stepA = line [i - 1];
				Vector3 stepB = line [i];
				float stepD = Vector3.Distance (stepA, stepB);
				float stepAngle = AngleBetweenVector2XZ (stepA, stepB);
				float angleDiff = Mathf.Abs (stepAngle - angle);

				if (spare != null && Mathf.Abs (stepAngle - spare.angle) <= maxAngle) {
					stepD += spare.distance;
					stepA = spare.pointA;
					spare = null;
				}


				if (angleDiff > maxAngle) { //angle is too wide
				
					if (stepD > d) { //Reset segment
						pointA = stepA;
						pointB = stepB;
						d = stepD;
						angle = stepAngle;
//						Debug.Log ("Reset segment");
					} 
					else { //Save this segment for next step, just in case
//						Debug.Log ("Save segment");
						GOSegment s = new GOSegment ();
						s.pointA = stepA;
						s.pointB = stepB;
						s.angle = stepAngle;
						s.distance = stepD;
						spare = s;
					}
	
				} else { //angle is ok, add the current segment

					pointB = stepB;
					d += stepD;
//					Debug.Log ("Add segment "+angle+ " " + stepAngle);
				}
			}

			GOSegment segment = new GOSegment ();
			segment.pointA = pointA;
			segment.pointB = pointB;
			segment.angle = angle;
			segment.distance = d;

			return segment;



		}
	


		public static float AngleBetweenVector2XZ(Vector3 vec1, Vector3 vec2)
		{
			Vector3 diference = vec2 - vec1;
			float sign = (vec2.z < vec1.z)? -1.0f : 1.0f;
			return Vector3.Angle(Vector3.right, diference) * sign;
		}

		public static float AngleBetweenVector3(Vector3 vec1, Vector3 vec2)
		{
			Vector3 diference = vec2 - vec1;
			float sign = (vec2.y < vec1.y)? -1.0f : 1.0f;
			return Vector3.Angle(Vector3.right, diference) * sign;
		}


		public Vector3 findMiddlePoint (float y) {

			Vector3 v = Vector3.Lerp(pointA, pointB,0.5f);
			v.y += y;
			return v;
		}

		public Vector3 direction () {

			return  (pointB - pointA);
		}

		public void DebugSegment() {

			Debug.DrawLine(pointA, pointB, Color.red, 5000000000, false);
		}
	}
}