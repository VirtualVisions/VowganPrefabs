using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Vowgan
{
    [CustomEditor(typeof(SecurityCamera))]
    public class SecurityCameraEditor : Editor
    {
        
        private SecurityCamera script;
        private Transform trans;
        private Texture2D banner;
        private float bodyAngle;

        private SerializedProperty propRotationRange;
        private SerializedProperty propRotationSpeed;
        private SerializedProperty propWaitTime;
        private SerializedProperty propState;
        private SerializedProperty propTraversePoint;
        private SerializedProperty propWaitTimeClock;
        private SerializedProperty propAxis;
        private SerializedProperty propBody;
        private SerializedProperty propRenderPoint;
        
        
        private float BodyAngle
        {
            get => bodyAngle;
            set
            {
                bodyAngle = value;
                if (script.Body == null) return;
                Vector3 bodyOldRotation = script.Body.localRotation.eulerAngles;
                script.Body.localRotation = Quaternion.Euler(
                    value,
                    bodyOldRotation.y,
                    bodyOldRotation.z);
            }
        }
        
        private void OnEnable()
        {
            script = target as SecurityCamera;
            if (script == null) return;
            trans = script.transform;
            
            PullBodyAngle();
            
            banner = Resources.Load<Texture2D>("VV_SecurityCameraBanner");
            
            propRotationRange = serializedObject.FindProperty(nameof(SecurityCamera.RotationRange));
            propRotationSpeed = serializedObject.FindProperty(nameof(SecurityCamera.RotationSpeed));
            propWaitTime = serializedObject.FindProperty(nameof(SecurityCamera.WaitTime));
            
            propState = serializedObject.FindProperty(nameof(SecurityCamera.State));
            propTraversePoint = serializedObject.FindProperty(nameof(SecurityCamera.TraversePoint));
            propWaitTimeClock = serializedObject.FindProperty(nameof(SecurityCamera.WaitTimeClock));
            
            propAxis = serializedObject.FindProperty(nameof(SecurityCamera.Axis));
            propBody = serializedObject.FindProperty(nameof(SecurityCamera.Body));
            propRenderPoint = serializedObject.FindProperty(nameof(SecurityCamera.RenderPoint));
        }

        private void PullBodyAngle()
        {
            if (script.Axis)
            {
                BodyAngle = script.Body.localRotation.eulerAngles.x;
            }
            if (BodyAngle > 180) BodyAngle -= 360;
        }
        
        public override void OnInspectorGUI()
        {
            Repaint();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Box(banner, GUIStyle.none);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((UdonSharpBehaviour)target), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            StartArea("Camera Angle");
            GUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider("Rotation Range", ref script.RotationRange.x, ref script.RotationRange.y, -180, 180);
            Vector2 rotationRangeInt = new Vector2((int)script.RotationRange.x, (int)script.RotationRange.y);
            propRotationRange.vector2Value = EditorGUILayout.Vector2Field("", rotationRangeInt, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            
            BodyAngle = EditorGUILayout.Slider("Body Angle", BodyAngle, -90, 90);
            EditorGUILayout.PropertyField(propRotationSpeed);
            EditorGUILayout.PropertyField(propWaitTime);
            EndArea();
            
            StartArea("References");
            EditorGUILayout.PropertyField(propAxis);
            EditorGUILayout.PropertyField(propBody);
            EditorGUILayout.PropertyField(propRenderPoint);
            EndArea();
            
            StartArea("Values", true, true);
            EditorGUILayout.EnumPopup("State", (CameraState) propState.intValue);
            EditorGUILayout.PropertyField(propTraversePoint);
            EditorGUILayout.PropertyField(propWaitTimeClock);
            EndArea();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnSceneGUI()
        {
            if (script == null || script.Axis == null) return;
            
            Vector3 origin = script.Axis.position;
            Vector3 direction = trans.up;
            Vector3 startAngle = Quaternion.Euler(0, script.RotationRange.y, 0) * trans.forward;
            float fullAngle = script.RotationRange.x - script.RotationRange.y;
            
            Handles.color = new Color(0, 1, 1, 0.2f);
            Handles.DrawSolidArc(origin, direction, startAngle, fullAngle, 0.15f);
            Handles.Label(origin, $"{Mathf.Abs(fullAngle)}º");
        }
        
        private void StartArea(string label = "", bool disabled = false, bool useBox = false)
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(disabled);
            GUILayout.BeginVertical(useBox ? EditorStyles.helpBox : GUIStyle.none);
        }
        
        private void EndArea()
        {
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
            GUILayout.Space(10);
        }
        
        private enum CameraState
        {
            Idle,
            Forward,
            ForwardWait,
            Reverse,
            ReverseWait,
        }
        
    }
}