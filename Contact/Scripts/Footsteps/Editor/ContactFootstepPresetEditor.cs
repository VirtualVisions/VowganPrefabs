using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vowgan.Contact.Footsteps
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ContactFootstepPreset))]
    public class ContactFootstepPresetEditor : Editor
    {
        
        public VisualTreeAsset InspectorTree;


        private void OnEnable()
        {
            
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            InspectorTree.CloneTree(root);
            
            return root;
        }
        
    }
}