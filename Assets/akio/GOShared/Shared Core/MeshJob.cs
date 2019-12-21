using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

public class MeshJob : ThreadedJob {

	public Poly2Mesh.Polygon poly;
	public GOMesh premesh;

	protected override void ThreadFunction()
	{
		premesh = Poly2Mesh.CreateMeshInBackground (poly);
	}
//	protected override void OnFinished()
//	{
//		Debug.Log("Mesh created in background");
//	}
}
