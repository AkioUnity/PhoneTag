using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoMap {

	[System.Serializable]
	public class GOStreetnamesSettings {

		public Shader textShader;
		public Font streetNameFont;
		public Color streetnameColor = new Color (61/255.0f, 61/255.0f, 83/255f);
		public int fontSize = 15;
		public int minFontSize = 12;
		public float characterSize = 1;
		public FontStyle fontStyle = FontStyle.Bold;
	}
}