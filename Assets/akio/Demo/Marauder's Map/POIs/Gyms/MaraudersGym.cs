using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaraudersGym : MonoBehaviour {

	public Color[] color; 

	// Use this for initialization
	void Start () {

		MeshRenderer mr = GetComponent<MeshRenderer> ();

		int rand = Random.Range (0, 4);
		mr.material.color = color[rand];
//		switch (rand) {
//
//		case 0:
//			mr.material.color = Color.red;
//			break;
//		case 1:
//			mr.material.color = Color.green;
//			break;
//		case 2:
//			mr.material.color = Color.blue;
//			break;
//		case 3:
//			mr.material.color = Color.yellow;
//			break;
//		}
	}

}
