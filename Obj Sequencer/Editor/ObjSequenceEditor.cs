using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VowganVR
{
    // [CustomEditor(typeof(ObjSequence))]
    public class ObjSequenceEditor : Editor
    {

        private ObjSequence script;
        private bool showDefaultInspector = false;
        
        private SerializedProperty propAnimating;
        private SerializedProperty propLoop;
        private SerializedProperty propReversing;
        private SerializedProperty propFramerate;
        private SerializedProperty propUseCustomMaterials;
        private SerializedProperty propSequenceObjects;
        private SerializedProperty propMaterials;

        private IMGUIContainer UdonHeaderContainer;
        
        private Button PlayButton;
        private Button PauseButton;
        private Button StopButton;
        private Button ReverseButton;

        private PropertyField AnimatingField;
        private PropertyField LoopField;
        private PropertyField ReversingField;
        private PropertyField FramerateField;
        private PropertyField UseCustomMaterialsField;
        private PropertyField SequenceObjectsField;
        private PropertyField MaterialsField;
        
        
        private void OnEnable()
        {
            script = target as ObjSequence;
            
            propAnimating = serializedObject.FindProperty(nameof(script.Animating));
            propLoop = serializedObject.FindProperty(nameof(script.Loop));
            propReversing = serializedObject.FindProperty(nameof(script.Reversing));
            propFramerate = serializedObject.FindProperty(nameof(script.Framerate));
            propSequenceObjects = serializedObject.FindProperty(nameof(script.SequenceObjects));
            propUseCustomMaterials = serializedObject.FindProperty(nameof(script.UseCustomMaterials));
            propMaterials = serializedObject.FindProperty(nameof(script.Materials));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("ObjSequencerInspector");
            uxml.CloneTree(root);

            UdonHeaderContainer = root.Query<IMGUIContainer>("UdonHeaderContainer");
            UdonHeaderContainer.onGUIHandler += UdonHeaderGUI;
            
            PlayButton = root.Query<Button>("PlayButton");
            PlayButton.clicked += PlayButtonOnClicked;
            
            PauseButton = root.Query<Button>("PauseButton");
            PauseButton.clicked += PauseButtonOnClicked;
            PauseButton.style.display = DisplayStyle.None;
            
            StopButton = root.Query<Button>("StopButton");
            StopButton.clicked += StopButtonOnClicked;
            
            ReverseButton = root.Query<Button>("ReverseButton");
            ReverseButton.clicked += ReverseButtonOnClicked;
            
            
            AnimatingField = root.Query<PropertyField>("AnimatingField");
            AnimatingField.BindProperty(propAnimating);
            LoopField = root.Query<PropertyField>("LoopField");
            LoopField.BindProperty(propLoop);
            ReversingField = root.Query<PropertyField>("ReversingField");
            ReversingField.BindProperty(propReversing);
            FramerateField = root.Query<PropertyField>("FramerateField");
            FramerateField.BindProperty(propFramerate);
            UseCustomMaterialsField = root.Query<PropertyField>("UseCustomMaterialsField");
            UseCustomMaterialsField.RegisterValueChangeCallback(CustomMaterialsCallback);
            
            SequenceObjectsField = root.Query<PropertyField>("SequenceObjectsField");
            MaterialsField = root.Query<PropertyField>("MaterialsField");
            
            
            EditorApplication.playModeStateChanged += PlayModeChanged;
            PlayModeStateChange playState = Application.isPlaying
                ? PlayModeStateChange.EnteredPlayMode
                : PlayModeStateChange.EnteredEditMode;
            
            PlayModeChanged(playState);
            CustomMaterialsCallback(null);

            return root;
        }

        private void UdonHeaderGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);
        }

        private void CustomMaterialsCallback(SerializedPropertyChangeEvent evt)
        {
            MaterialsField.style.display = script.UseCustomMaterials
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private void PlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    PlayButton.SetEnabled(false);
                    PauseButton.SetEnabled(false);
                    StopButton.SetEnabled(false);
                    ReverseButton.SetEnabled(false);
                    break;
                
                case PlayModeStateChange.EnteredPlayMode:
                    PlayButton.SetEnabled(true);
                    PauseButton.SetEnabled(true);
                    StopButton.SetEnabled(true);
                    ReverseButton.SetEnabled(true);
                    break;
            }
        }


        private void PlayButtonOnClicked()
        {
            propAnimating.boolValue = true;
            serializedObject.ApplyModifiedProperties();
            
            // script.Play();
            PlayButton.style.display = DisplayStyle.None;
            PauseButton.style.display = DisplayStyle.Flex;
        }

        private void PauseButtonOnClicked()
        {
            propAnimating.boolValue = false;
            serializedObject.ApplyModifiedProperties();
            
            // script.Pause();
            PlayButton.style.display = DisplayStyle.Flex;
            PauseButton.style.display = DisplayStyle.None;
        }

        private void StopButtonOnClicked()
        {
            script.Stop();
        }

        private void ReverseButtonOnClicked()
        {
            script.Reverse();
        }

        /*
        public override void OnInspectorGUI()
        {
            if(UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            
            // Button Layout
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (Application.isPlaying)
            {
                if (!script.Animating)
                {
                    if (GUILayout.Button(spritePlay, EditorStyles.toolbarButton)) script.Play();
                }
                else
                {
                    if (GUILayout.Button(spritePause, EditorStyles.toolbarButton)) script.Pause();
                }
                if (GUILayout.Button(spriteStop, EditorStyles.toolbarButton)) script.Stop();
                if (GUILayout.Button("Reverse", EditorStyles.toolbarButton)) script.Reverse();
                if (GUILayout.Button("Reset", EditorStyles.toolbarButton)) script.ResetState();
            }
            else
            {
                if (!script.Animating)
                {
                    if (GUILayout.Button(spritePlay, EditorStyles.toolbarButton)) script.Play();
                }
                else
                {
                    if (GUILayout.Button(spritePause, EditorStyles.toolbarButton)) script.Pause();
                }
                if (GUILayout.Button(spriteStop, EditorStyles.toolbarButton)) script.Pause();
                if (GUILayout.Button("Reverse", EditorStyles.toolbarButton)) script.Reverse();
            }

            GUILayout.EndHorizontal();

            //Display Variables
            EditorGUILayout.PropertyField(propAnimating);
            EditorGUILayout.PropertyField(propLoop);
            
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            EditorGUILayout.PropertyField(propReversing);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(propFramerate);
            
            //Push next segment 10 to the right
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(propUseCustomMaterials);
            GUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(propSequenceObjects);
            if (propUseCustomMaterials.boolValue)
            {
                EditorGUILayout.PropertyField(propMaterials);
            }
            EditorGUI.indentLevel--;
            
            GUILayout.EndVertical();
            GUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
            script.ApplyProxyModifications();
            
            GUILayout.Space(10);
            
            showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Show Default Options", true);
            if (showDefaultInspector)
            {
                DrawDefaultInspector();
            }
        }
        */
        
    }
}