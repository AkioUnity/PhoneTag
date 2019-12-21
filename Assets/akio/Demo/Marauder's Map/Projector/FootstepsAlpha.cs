using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FootstepsAlpha : MonoBehaviour {

	Projector proj;
	public float alpha;

	public void reloadAnimation () {

		FadeInFoot ();
		StartCoroutine (FadeOutFoot ());
	}

	public IEnumerator FadeInFoot () {

		StopAllCoroutines ();

		if (proj == null)
			proj = GetComponent<Projector> ();

		yield return StartCoroutine (fade (0.3f,true));

	}

	public IEnumerator FadeOutFoot (float time = 10.0f) {
	
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine (fade (time,false));
	}

	private IEnumerator fade(float time, bool fadeIn) {

		float elapsedTime = 0;

		while (elapsedTime <= time)
		{
			Color c = proj.material.color;
			if (fadeIn)
				c.a = Mathf.Lerp(0f,1.5f, elapsedTime / time);
			else c.a = Mathf.Lerp(1.0f, -0.5f, elapsedTime / time);
			alpha = c.a;
			proj.material.color = c;
			elapsedTime += Time.deltaTime;

			yield return new WaitForEndOfFrame();
		}
	}
		
}
