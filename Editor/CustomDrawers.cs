namespace DynamicUI
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EnumFlagAttribute))]
    public class EnumFlagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        { 
            EditorGUI.BeginProperty(position, label, property);

            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        
            EditorGUI.EndProperty();
        }

    }


}
