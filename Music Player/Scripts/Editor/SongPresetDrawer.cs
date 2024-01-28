using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace Vowgan.Music
{
    [CustomPropertyDrawer(typeof(SongPreset))]
    public class SongPresetDrawer : PropertyDrawer
    {
        
        private VisualTreeAsset songDrawer;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            songDrawer = Resources.Load<VisualTreeAsset>("SongPresetDrawer");

            songDrawer.CloneTree(root);

            return root;
        }
    }
}