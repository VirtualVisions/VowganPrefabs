using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vowgan.Contact.Footsteps
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ContactFootstepOverride))]
    public class ContactFootstepOverrideEditor : Editor
    {
        
        [SerializeField] private VisualTreeAsset InspectorTree;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorTree.CloneTree(root);
            return root;
        }
        
    }
}