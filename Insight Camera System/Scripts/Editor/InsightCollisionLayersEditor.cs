using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
namespace Vowgan
{
    [CustomEditor(typeof(InsightCollisionLayers))]
    public class InsightCollisionLayersEditor : Editor
    {

        private InsightCollisionLayers script;

        private SerializedProperty propLayers;
        private SerializedProperty propColliders;
        
        
        private void OnEnable()
        {
            script = target as InsightCollisionLayers;
            if (script == null) return;

            propLayers = serializedObject.FindProperty(nameof(script.CollisionLayers));
            propColliders = serializedObject.FindProperty(nameof(script.Colliders));
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("This lets you change what layers the cameras collide with.\nThis only exists because Cinemachine Colliders aren't exposed to Udon.", MessageType.Info);
            GUILayout.Space(5);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propLayers);
            EditorGUILayout.PropertyField(propColliders);
            serializedObject.ApplyModifiedProperties();
            
            if (EditorGUI.EndChangeCheck())
            {
                foreach (CinemachineCollider col in script.Colliders)
                {
                    col.m_CollideAgainst = script.CollisionLayers;
                }
            }

        }
    }
}
#endif