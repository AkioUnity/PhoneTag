using UnityEngine;
using System.Linq;

namespace GoShared {

	public class SimpleExtruder : MonoBehaviour
	{
		public Vector3 normal;
		public float height;

		public SimpleExtruder (float _height)
		{
			height = _height;
		}

		void Start () {

			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
			normal = mesh.normals [0];
	//		bool normalFaceDown = mesh.normals [0].y > 0 ;

			Matrix4x4 [] extrusionPath = new Matrix4x4 [2];
			extrusionPath[0] = gameObject.transform.worldToLocalMatrix * Matrix4x4.TRS(gameObject.transform.position, Quaternion.identity, Vector3.one);
			extrusionPath[1] = gameObject.transform.worldToLocalMatrix * Matrix4x4.TRS(gameObject.transform.position + new Vector3(0, height, 0), Quaternion.identity, Vector3.one);
			MeshExtrusion.ExtrudeMesh(mesh, gameObject.GetComponent<MeshFilter>().mesh, extrusionPath, false);

			mesh = Extrude (mesh, gameObject, height);

	//		//Check if normal are facing inside
	//		if (normalFaceDown) {
	//			gameObject.AddComponent<ReverseNormals>();
	//		}

		}

		public static Mesh Extrude(Mesh mesh, GameObject obj, float height) {

			bool normalFaceDown = mesh.normals [0].y > 0 ;

			Matrix4x4 [] extrusionPath = new Matrix4x4 [2];
			Matrix4x4 a = obj.transform.worldToLocalMatrix * Matrix4x4.TRS(obj.transform.position, Quaternion.identity, Vector3.one);
			Matrix4x4 b = obj.transform.worldToLocalMatrix * Matrix4x4.TRS(obj.transform.position + new Vector3(0, height, 0), Quaternion.identity, Vector3.one);

			//		Check if normal are facing inside
			if (!normalFaceDown) {
				extrusionPath [0] = a;
				extrusionPath [1] = b;
	//			obj.AddComponent<ReverseNormals>();
			} else {
				extrusionPath [0] = b;
				extrusionPath [1] = a;
			}



			MeshExtrusion.ExtrudeMesh(mesh, mesh, extrusionPath, false);

			mesh.RecalculateNormals ();


			return mesh;
		}
	}
}
