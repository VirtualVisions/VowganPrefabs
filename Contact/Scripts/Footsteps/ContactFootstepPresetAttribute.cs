using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif


namespace Vowgan.Contact.Footsteps
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ContactFootstepPresetAttribute : PropertyAttribute
    {
    }


#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ContactFootstepPresetAttribute))]
    public class ContactFootstepsPresetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(
                position, label, property.objectReferenceValue, typeof(ContactFootstepPreset), false);
            EditorGUI.EndProperty();
        }
    }

#endif

}