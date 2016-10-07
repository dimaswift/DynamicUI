using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DynamicUI
{
    [CustomEditor(typeof(DUIRaycaster))]
    public class DUIRaycasterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_onPointerDown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_onPointerUp"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);  
            }
        }
    }
}
