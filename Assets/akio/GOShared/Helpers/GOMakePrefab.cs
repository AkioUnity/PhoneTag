using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;

namespace GoShared {

	public class GOMakePrefab : MonoBehaviour {

		private string prefabName;

		[CustomEditor(typeof(GOMakePrefab))]
		public class GOMapInspector : Editor
		{
			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();

				GOMakePrefab editor = (GOMakePrefab)target;

				EditorGUILayout.HelpBox ("The name for the exported prefab",MessageType.Info);
				editor.prefabName = EditorGUILayout.TextField ("Prefab Name",editor.prefabName);


				if(GUILayout.Button("Save object as prefab")) {
					editor.SaveAsPrefab ();
				}

				EditorGUILayout.Separator ();

				if(GUILayout.Button("Combine meshes")) {
					editor.CombineMesh ();
				}
			}
		}

		public void SaveAsPrefab () {

			try {
				MeshFilter filter = GetComponent<MeshFilter> ();
				Mesh mesh;
				if (filter != null) {
					mesh = filter.mesh;
				} else mesh = CombineMesh();
					
				if (!Directory.Exists ("Assets/GOMakePrefab")) {
					AssetDatabase.CreateFolder ("Assets", "GOMakePrefab");
					AssetDatabase.CreateFolder ("Assets/GOMakePrefab", "Objects");
					AssetDatabase.CreateFolder ("Assets/GOMakePrefab", "Meshes");
				}

				if (prefabName.Length == 0) {
					Debug.LogWarning("[GOMakePrefab] Please insert a valid name for the prefab");
					return;
				}

				AssetDatabase.CreateAsset(mesh, "Assets/GOMakePrefab/Meshes/"+prefabName+".asset");
				PrefabUtility.CreatePrefab ("Assets/GOMakePrefab/Objects/"+prefabName+".prefab", gameObject,ReplacePrefabOptions.Default);
				Debug.Log("[GOMakePrefab] Object saved correctly!");

			} catch (System.Exception ex) {
				Debug.LogWarning ("[GOMakePrefab] Error saving object: "+ex);
			}

		}

		Mesh CombineMesh()
		{

			MeshFilter[] mfChildren = GetComponentsInChildren<MeshFilter>();
			CombineInstance[] combine = new CombineInstance[mfChildren.Length];

			MeshRenderer[] mrChildren = GetComponentsInChildren<MeshRenderer>();

			MeshRenderer mrSelf = gameObject.AddComponent<MeshRenderer>();
			MeshFilter mfSelf = gameObject.AddComponent<MeshFilter>();

			List<Material> materials = new List<Material>();
			for (int i = 0; i < mrChildren.Length; i++) {
				Material mat = mrChildren[i].sharedMaterial;
				materials.Add(mat);
			}
			mrSelf.materials = materials.ToArray();

			Mesh newMesh = new Mesh();

			MeshRenderer meshRendererCombine = gameObject.GetComponent<MeshRenderer> ();

			for (int i = 0; i < mfChildren.Length; i++){
				if (!meshRendererCombine)
					meshRendererCombine = gameObject.AddComponent<MeshRenderer> ();   

				combine[i].mesh = mfChildren[i].sharedMesh;
				combine[i].transform = mfChildren[i].transform.localToWorldMatrix;

				Destroy (mfChildren [i].gameObject);

			}
				
			newMesh.CombineMeshes(combine, false, true);
			mfSelf.mesh = newMesh;

			return newMesh;
		}
	}
}
#endif
