
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vowgan.Contact
{
    public class ContactSurfacePresetAttribute : PropertyAttribute
    {
    }
    
    
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ContactSurfacePresetAttribute))]
    public class ContactSurfacePresetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(
                position, label, property.objectReferenceValue, typeof(ContactSurfacePreset), false);
            EditorGUI.EndProperty();
        }
    }

#endif
    
}