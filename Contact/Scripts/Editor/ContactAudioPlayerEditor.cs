using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vowgan.Contact
{
    [CustomEditor(typeof(ContactAudioPlayer))]
    public class ContactAudioPlayerEditor : Editor
    {

        [SerializeField] private VisualTreeAsset InspectorTree;
        
        private SerializedProperty propAudioPrefab;


        private void OnEnable()
        {
            propAudioPrefab = serializedObject.FindProperty(nameof(ContactAudioPlayer.AudioPrefab));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement container = new VisualElement();
            InspectorTree.CloneTree(container);

            ObjectField audioPrefabField = container.Query<ObjectField>("AudioPrefabField");
            audioPrefabField.objectType = typeof(GameObject);
            audioPrefabField.BindProperty(propAudioPrefab);

            return container;
        }
    }
}