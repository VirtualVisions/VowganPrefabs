using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vowgan.Contact.Physics
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ContactCollisionPreset))]
    public class ContactPhysicsPresetEditor : Editor
    {
        
        public VisualTreeAsset InspectorTree;
        
        private SerializedProperty propClips;


        private void OnEnable()
        {
            propClips = serializedObject.FindProperty(nameof(ContactCollisionPreset.Clips));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            InspectorTree.CloneTree(root);

            IMGUIContainer clipsContainer = root.Query<IMGUIContainer>("ClipsContainer");
            clipsContainer.onGUIHandler += ClipsContainerGUI;
            
            return root;
        }

        private void ClipsContainerGUI()
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(propClips);
                if (changed.changed) serializedObject.ApplyModifiedProperties();
            }
        }
    }
}