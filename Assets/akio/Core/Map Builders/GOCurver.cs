using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GOCurver {
	
	//arrayToCurve is original Vector3 array, smoothness is the number of interpolations. 
	public static List<Vector3> MakeSmoothCurve(List<Vector3> arrayToCurve,int smoothness, bool loop = false){
		
		List<Vector3> points;
		List<Vector3> curvedPoints;
		int pointsLength = 0;
		int curvedLength = 0;	

		if(smoothness < 1) smoothness = 1;

//		pointsLength = loop? arrayToCurve.Count-1 : arrayToCurve.Count;
		pointsLength = arrayToCurve.Count;

		curvedLength = (pointsLength*smoothness)-1;
		curvedPoints = new List<Vector3>(curvedLength);

		float t = 0.0f;
		for(int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength+1;pointInTimeOnCurve++){
			
			t = Mathf.InverseLerp(0,curvedLength,pointInTimeOnCurve);

			points = new List<Vector3>(arrayToCurve);

			if (loop) {
				for (int j = pointsLength-1; j > 0; j--){
					for (int i = 0; i < j; i++){

						if (pointInTimeOnCurve == 0 && j ==1 && i == j-1)
							Debug.Log ("First");

						if (pointInTimeOnCurve == 0 && j == pointsLength-1 && i == 0)
							Debug.Log ("Last");

						points[i] = (1-t)*points[i] + t*points[i+1];


					}
				}
			} else {
				for (int j = pointsLength-1; j > 0; j--){
					for (int i = 0; i < j; i++){
						points[i] = (1-t)*points[i] + t*points[i+1];
					}
				}
			}

			curvedPoints.Add(points[0]);


		}

		return curvedPoints;
	}

	public static List<Vector3> MakeBetterCurve (List<Vector3> points, int resolution, bool loop)
	{

		List <Vector3> geometry = loop? points.GetRange (0, points.Count - 1) : points.GetRange (0, points.Count);
		List<Vector3> betterCurve = new List<Vector3> ();

		//Number of segments
		int segmentsCount = loop ? (resolution) * (geometry.Count) : (resolution) * (geometry.Count - 1);
		if (segmentsCount <= 1) //If the count is less than 2 points it's not a line, return.
			return betterCurve;

		Vector3 p0;
		Vector3 p1;
		Vector3 m0;
		Vector3 m1;


		int closedAdjustment = loop ? 0 : 1;

		for (int i = 0; i < geometry.Count - closedAdjustment; i++)
		{

			p0 = geometry[i];
			p1 = (loop == true && i == geometry.Count - 1) ? geometry[0] : geometry[i + 1];

			// m0
			if (i == 0)
				m0 = loop ? 0.5f * (p1 - geometry[geometry.Count - 1]) : p1 - p0;
			else
				m0 = 0.5f * (p1 - geometry[i - 1]);

			// m1
			if (loop)
			{
				if (i == 0)
					m1 = 0.5f * (geometry[i + 2] - p0);
				else
					m1 = 0.5f * (geometry[(i + 2) % geometry.Count] - p0);
			}
			else
			{
				if (i < geometry.Count - 2)
					m1 = 0.5f * (geometry[(i + 2) % geometry.Count] - p0);
				else
					m1 = p1 - p0;
			}

			float t;
			float pointStep = 1.0f / resolution;
			if ((i == geometry.Count - 2 && loop == false) || (i == geometry.Count - 1 && loop))
			{
				pointStep = 1.0f / (resolution - 1); // last point of last segment should reach p1
			}

			//Create Road segments
			for (int j = 0; j < resolution; j++)
			{
				t = j * pointStep;
				betterCurve.Add(CatmullRom.Interpolate(p0, p1, m0, m1, t));
			}
		}

		return betterCurve;

	}


}