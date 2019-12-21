using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;
using System;

public class FootstepsController : MonoBehaviour {

	public GameObject footPrefab;
	public Texture[] textures;

	public float distance = 5;

	Vector3 lastPosition = Vector3.zero;
	float lastTime;
	bool lastStepRight = true; //0 left, 1 // right
	Vector3 rot;

	public int poolSize;
	List<GameObject> objectPool;



	void Start () {

		objectPool = new List<GameObject> ();
		rot = transform.rotation.eulerAngles;
	}


	// Update is called once per frame
	void Update () {

		#if UNITY_WEBGL

		moveAvatar();

		#endif


		Vector3 positionInWorld = transform.parent.position;
		float d = Vector3.Distance (positionInWorld, lastPosition);
		if ((d >= distance || objectPool.Count <2) && (LocationManager.IsOriginSet || !LocationManager.UseLocationServices)){
			lastPosition = positionInWorld;

			GameObject footstep = getObjectFromPool ();
			Projector projector = footstep.GetComponent<Projector> ();
			if (lastStepRight)
				projector.material.SetTexture("_ShadowTex",textures[1]);
			else
				projector.material.SetTexture("_ShadowTex",textures[0]);

			lastStepRight = !lastStepRight;

			rot.y = transform.rotation.eulerAngles.y;
			footstep.transform.eulerAngles = rot;
			footstep.transform.position = new Vector3 (lastPosition.x, transform.position.y, lastPosition.z);
			footstep.SetActive(true);

//			footstep.GetComponent<FootstepsAlpha> ().reloadAnimation ();

			footstep.GetComponent <FootstepsAlpha> ().StartCoroutine(footstep.GetComponent <FootstepsAlpha> ().FadeInFoot ());

			if (objectPool.Count > 2) {

				float time = Time.time;
				float deltaT = time - lastTime;
				lastTime = time;

				GameObject last = objectPool [2];
				FootstepsAlpha fa = last.GetComponent <FootstepsAlpha> ();
				fa.StartCoroutine (fa.FadeOutFoot (5*deltaT));
			}

		}
	}

	GameObject getObjectFromPool () {

		if (objectPool.Count < poolSize) {
			GameObject n = GameObject.Instantiate (footPrefab);
			Projector projector = n.GetComponent<Projector> ();
			projector.material = Material.Instantiate(projector.material);

			objectPool.Insert(0,n);
			return n;
		}

		GameObject r = objectPool [objectPool.Count - 1];
		objectPool.Remove (r);
		objectPool.Insert(0,r);

		return r;
	}

	public void moveAvatar () {

		Vector3 dir = Vector3.ProjectOnPlane (transform.up, Vector3.up);
		Vector3 axis = Vector3.down;

		Vector3 localEuler = transform.localEulerAngles;

		if (Input.GetKey (KeyCode.D))
			localEuler.y += 5;
		else if (Input.GetKey (KeyCode.A))
			localEuler.y -= 5;

		transform.localEulerAngles = localEuler;

		Debug.DrawLine (transform.position, transform.position + dir*100, Color.green,1);

		bool thrust = Input.GetKey (KeyCode.W);
		bool reverseThrust = Input.GetKey(KeyCode.S);

		int reverse = 1;
		if (reverseThrust) {
			reverse = -1;
			thrust = true;
		}

		float speed = 0.8f;

		if (thrust && !GOUtils.IsPointerOverUI ()) {
			transform.parent.Translate (Time.deltaTime * (speed * 20 * transform.up * reverse));
		}
	}

}
