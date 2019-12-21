using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateProjector : MonoBehaviour {

	Projector projector;
	public bool continuous = false;
	public float minSize = 0f;
	public float maxSize = 10f;
	public float animationTime = 0.3f;
	public float interval = 0f;

	// Use this for initialization
	IEnumerator Start () {
		
		projector = GetComponent<Projector> ();

		yield return StartCoroutine(Animate(animationTime));

		yield return null;
	}

	private IEnumerator Animate(float time) {

		float size = maxSize;

		if (!continuous)
			size *= Vector3.Distance (Camera.main.transform.position, transform.position) / 200;

		float elapsedTime = 0;
		while (elapsedTime < time)
		{
			float t = (elapsedTime / time);

			projector.orthographicSize = Mathf.Lerp(minSize, size, Mathf.SmoothStep(0, 1.0f, t)
				);
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		elapsedTime = 0;
		while (elapsedTime < time)
		{
			float t = (elapsedTime / time);

			projector.orthographicSize = Mathf.Lerp(size, minSize, Mathf.SmoothStep(0, 1.0f, t)
				);
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		if (continuous) {
			yield return new WaitForSeconds (interval);
			yield return StartCoroutine(Animate(animationTime));
		} else GameObject.Destroy (gameObject);
	}

}
