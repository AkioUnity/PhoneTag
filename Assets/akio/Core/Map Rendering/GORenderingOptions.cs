using UnityEngine;
using GoShared;
using UnityEngine.Serialization;

namespace GoMap {

	[System.Serializable]
	public class GORenderingOptions
	{

		public GOFeatureKind kind;
		public Material material;
		[ConditionalHide("parent/layerType", "Roads")] public Material outlineMaterial;
		[ConditionalHide("parent/layerType", "Buildings")] public bool hasRoof;
		[ConditionalHide("parent/layerType", "Buildings")] public Material roofMaterial;


		public Material[] materials;

//		[ConditionalHide("parent/layerType", "Roads")]
		public float lineWidth;
		[ConditionalHide("parent/layerType", "Roads")] public float outlineWidth;
		[ConditionalHide("parent/layerType", "Roads", true)] public float polygonHeight;

		public string tag;

	}

}