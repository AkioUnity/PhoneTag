using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attributes : MonoBehaviour {

	public string kind;
	public string detail;
	public string type;
	public KeyValue[] attributesList;
	public bool useName = false;

	string _name = null;

	Dictionary <string,object> attributes;

	public void loadWithDictionary (Dictionary <string,object> _attributes) {

		if (!Application.isEditor) {
			return;
		}

		if (_attributes.ContainsKey("kind")) {
			kind = (string)_attributes ["kind"];
		} else if (_attributes.ContainsKey("class")) {
			kind = (string)_attributes ["class"];
		}

		if (_attributes.ContainsKey("kind_detail")) {
			detail = (string)_attributes ["kind_detail"];
		}

		if (_attributes.ContainsKey("name")) {
			_name = (string)_attributes ["name"];
		}

		attributes = _attributes;

		if (kind != null) {
			gameObject.name = kind;
		}
		if (name != null && useName) {
			gameObject.name = _name;
		}

		List <KeyValue> list = new List <KeyValue>();
		foreach (string key in attributes.Keys) {
			KeyValue keyValue = new KeyValue ();
			keyValue.key = key;
			if (attributes[key] != null) {
				keyValue.value = attributes[key].ToString();
			}
			list.Add (keyValue);
		}
		attributesList = list.ToArray();
	}
		
}

[System.Serializable]
public class KeyValue
{
	public string key;
	public string value;
}
