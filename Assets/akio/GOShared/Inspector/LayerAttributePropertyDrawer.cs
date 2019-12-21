#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;

using UnityEditor;


using UnityEngine;

//Thanks to: http://answers.unity.com/answers/1371058/view.html

[CustomPropertyDrawer(typeof(LayerAttribute))]
class LayerAttributeEditor : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // One line of  oxygen free code.
        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}
#endif