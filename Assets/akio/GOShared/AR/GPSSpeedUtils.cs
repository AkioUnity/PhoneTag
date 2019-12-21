using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GoShared {

	public static class GPSSpeedUtils {

		public static float GetSpeedFromCoordinatesList (List<Coordinates> locations) {
		
			if (locations.Count == 0)
				return 0;

			List<double> speeds = new List<double> ();
			for (int i = 0; i < locations.Count - 1; i++) {
				float d = locations [i+1].DistanceFromPoint (locations [i]);
				double time =  locations [i+1].intervalBetweenTimestamps (locations [i]);
//				Debug.Log ("Coordinates count: "+locations.Count + " Distance: "+d+" Time: "+time+" Speed: "+d/time);
				speeds.Add ((double) (d / time));
			}

			DynamicKalman kalman = new DynamicKalman ();

			for (int i = 0; i < speeds.Count; i++) {
				double smoothS =  kalman.update (speeds[i]);
//				Debug.Log("Speed: "+speeds[i]+" Smootheed: "+smoothS);
				if (i == speeds.Count-1) {

					smoothS = double.IsNaN (smoothS) ? 0 : smoothS;
					smoothS = double.IsInfinity (smoothS) ? 1000 : smoothS;

					return (float)smoothS;
				}
			}

			return 0f;
		}

	}

	public class SimpleKalman
	{
		private static double Q = 0.000001;
		private static double R = 0.01;
		private static double P = 1, X = 0, K;

		private static void measurementUpdate()
		{
			K = (P + Q) / (P + Q + R);
			P = R * (P + Q) / (R + P + Q);
		}

		public static double update(double measurement)
		{
			measurementUpdate();
			double result = X + (measurement - X) * K;
			X = result;
			return result;
		}
	}

	public class DynamicKalman
	{
		private double Q = 0.000001;
		private double R = 0.01;
		private double P = 1, X = 0, K;

		private void measurementUpdate()
		{
			K = (P + Q) / (P + Q + R);
			P = R * (P + Q) / (R + P + Q);
		}

		public double update(double measurement)
		{
			measurementUpdate();
			double result = X + (measurement - X) * K;
			X = result;
			return result;
		}
	}
}
