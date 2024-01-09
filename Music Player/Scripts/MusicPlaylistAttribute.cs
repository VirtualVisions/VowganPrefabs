
using System;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Vowgan.Music
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MusicPlaylistAttribute : PropertyAttribute { }
    
    
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(MusicPlaylistAttribute))]
    public class MusicPlaylistDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(
                position, label, property.objectReferenceValue, typeof(MusicPlayerPlaylist), false);
            EditorGUI.EndProperty();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ObjectField field = new ObjectField(property.displayName);
            field.BindProperty(property);
            return field;
        }
    }

#endif
    
}