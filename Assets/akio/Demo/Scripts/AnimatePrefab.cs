using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnimatePrefab : MonoBehaviour {

	public float speed = 500;
	public bool continousRotation;
	bool isAnimating;


	void OnCollisionEnter(Collision collision) {

		if (!isAnimating && !continousRotation)
			StartCoroutine (rotate(1));
		else if (continousRotation)
			transform.Rotate(transform.eulerAngles.x,speed*Time.deltaTime,transform.eulerAngles.z);
	}

	private IEnumerator rotate(float time) {

		isAnimating = true;
		float elapsedTime = 0;

		while (elapsedTime < time)
		{
			float value = Mathf.Lerp (0, 360, elapsedTime);
			transform.eulerAngles = new Vector3 (transform.eulerAngles.x, value, transform.eulerAngles.z);
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		isAnimating = false;
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, 0, transform.eulerAngles.z);

	}

}
