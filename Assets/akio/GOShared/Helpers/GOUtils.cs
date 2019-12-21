using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace GoShared {

	public class GOUtils : MonoBehaviour {

		public static bool IsPointerOverUI () {

			EventSystem system = UnityEngine.EventSystems.EventSystem.current;

			if (system != null) {
				if (Application.isMobilePlatform) {
					if (Input.touchCount > 0) {
						return system.IsPointerOverGameObject (Input.GetTouch (0).fingerId);
					}
				} else {
					bool overUI = system.IsPointerOverGameObject();
					return overUI;
				}
			}
			return false;
		}

		public static string RemoveBetween(string s, char begin, char end)
		{
			Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
			return regex.Replace(s, " ");
		}

		public static string RemoveDivs(string s)
		{
			return Regex.Replace(s,
				@"(\</?DIV(.*?)/?\>)", 
				" ",
				RegexOptions.IgnoreCase);
		}

		public static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
		{
			// Unix timestamp is seconds past epoch
			System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
			return dtDateTime;
		}

		public class ClockwiseVector3Comparer : IComparer<Vector3>
		{
			public int Compare(Vector3 v1, Vector3 v2)
			{
				return Mathf.Atan2(v1.x, v1.z).CompareTo(Mathf.Atan2(v2.x, v2.z));
			}
		}

		public static float positiveAngle (float angle) {
			if (angle<0){
				angle += 360f;
			}
			if (angle>360){
				angle -= 360f;
			}

			return angle;
		}

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }

	}
}