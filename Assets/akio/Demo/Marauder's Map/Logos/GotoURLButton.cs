using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GotoURLButton : MonoBehaviour {

	public string website;

	public void OpenWebsite () {
		Application.OpenURL (website);
//		Application.ExternalEval(string.Format("window.open(\"{0}\")",website));
//		Application.ExternalEval(string.Format("window.open(\"{0}\")",website));

	}


}
