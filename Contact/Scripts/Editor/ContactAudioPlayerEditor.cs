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
        
        private ContactAudioPlayer script;
        private SerializedProperty propAudioPrefab;
        
        
        private void OnEnable()
        {
            script = target as ContactAudioPlayer;
            propAudioPrefab = serializedObject.FindProperty(nameof(ContactAudioPlayer.AudioPrefab));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("ContactAudioPlayer");
            uxml.CloneTree(root);

            ObjectField audioPrefabField = root.Query<ObjectField>("AudioPrefabField");
            audioPrefabField.objectType = typeof(GameObject);
            audioPrefabField.BindProperty(propAudioPrefab);

            return root;
        }
    }
}