using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaraudersPOI : MonoBehaviour {

	public TextMesh textMesh;
	public float offsetXAngle = 75;
	int minimumFontSize  =35;
	public bool adaptSize = true;

	public void Start () {
	
		textMesh.text = name.ToUpper();

		MeshRenderer renderer = textMesh.gameObject.GetComponent<MeshRenderer> ();

		if (!adaptSize)
			return;

		for (int i = textMesh.fontSize; i >= minimumFontSize-1 ; i--) {
			textMesh.fontSize = i;
			float tl = renderer.bounds.size.x;
			if (10 >= tl) {
				break;
			}
			if (i==minimumFontSize-1) {
				GameObject.DestroyImmediate (this.gameObject);
				break;
			}
		}

	}

	// Update is called once per frame
	void Update () {

//		transform.LookAt (Vector3.ProjectOnPlane(2 *  Camera.main.transform.position, Vector3.up)- transform.position);
//		Vector3 p = transform.eulerAngles;
//		p.x = offsetXAngle;
//		transform.eulerAngles = p;

		transform.LookAt (Camera.main.transform.position);
		Vector3 p = transform.eulerAngles;
		p.x += 90;
		transform.eulerAngles = p;
	}
}
