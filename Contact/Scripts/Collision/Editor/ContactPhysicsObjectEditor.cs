
using System;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Vowgan.Contact.Physics
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ContactCollisionObject))]
    public class ContactPhysicsObjectEditor : Editor
    {
        public VisualTreeAsset InspectorTree;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorTree.CloneTree(root);
            return root;
        }
    }
}