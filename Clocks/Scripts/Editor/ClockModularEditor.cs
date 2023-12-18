using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace Vowgan.Clocks
{
    [CustomEditor(typeof(ClockModular))]
    public class ClockModularEditor : Editor
    {

        private ClockModular script;
        
        private List<ClockUtility.ClockHand> handList = new List<ClockUtility.ClockHand>();
        private List<ClockUtility.ClockFill> fillList = new List<ClockUtility.ClockFill>();
        private List<ClockUtility.ClockLabel> labelList = new List<ClockUtility.ClockLabel>();
        
        private SerializedProperty propTimeZoneID;
        private SerializedProperty propUse24HourTime;
        private SerializedProperty propUseLocalTimeZone;
        private SerializedProperty propUseAudioTick;
        private SerializedProperty propAudioTick;


        private void OnEnable()
        {
            script = target as ClockModular;
            if (script == null) return;
            
            propUse24HourTime = serializedObject.FindProperty(nameof(script.Use24HourTime));
            propUseLocalTimeZone = serializedObject.FindProperty(nameof(script.UseLocalTimeZone));
            propTimeZoneID = serializedObject.FindProperty(nameof(script.TimeZoneID));
            propUseAudioTick = serializedObject.FindProperty(nameof(script.UseAudioTick));
            propAudioTick = serializedObject.FindProperty(nameof(script.AudioTick));

            PullFromScript();
            Undo.undoRedoPerformed += PullFromScript;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= PullFromScript;
        }


        public override void OnInspectorGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);
            // base.OnInspectorGUI();
            
            EditorGUILayout.PropertyField(propUse24HourTime);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(propUseAudioTick);
                if (propUseAudioTick.boolValue) EditorGUILayout.PropertyField(propAudioTick, GUIContent.none);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(propUseLocalTimeZone);
                if (!propUseLocalTimeZone.boolValue) 
                {
                    if (GUILayout.Button($"{propTimeZoneID.stringValue}", EditorStyles.popup))
                    {
                        ClockUtility.TimezoneSearchProvider window = CreateInstance<ClockUtility.TimezoneSearchProvider>();
                        window.Title = "Timezones";
                        window.SearchItems = ClockUtility.Timezones;
                        window.OnIndexCallback = OnTimezoneChange;

                        SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), window);
                    }
                }
            }
            
            EditorGUI.BeginChangeCheck();
            DisplayLists();
            if (EditorGUI.EndChangeCheck())
            {
                PushToScript();
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void OnTimezoneChange(string timezoneName)
        {
            propTimeZoneID.stringValue = timezoneName;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
        
        private void DisplayLists()
        {
            GUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(30))) handList.Add(new ClockUtility.ClockHand());
                    using (new EditorGUI.DisabledScope(handList.Count == 0))
                    {
                        if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                        {
                            handList.RemoveAt(handList.Count - 1);
                            return;
                        }
                    }
                    GUILayout.Label("Hands");
                    GUILayout.FlexibleSpace();
                }

                foreach (ClockUtility.ClockHand hand in handList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        hand.Hand = (Transform)EditorGUILayout.ObjectField(hand.Hand, typeof(Transform), true);
                        hand.Mode = (ClockHandMode)EditorGUILayout.EnumPopup(hand.Mode, GUILayout.Width(70));
                        if (GUILayout.Button("x", GUILayout.Width(30)))
                        {
                            handList.Remove(hand);
                            return;
                        }
                    }
                }
            }
            
            GUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(30))) fillList.Add(new ClockUtility.ClockFill());
                    using (new EditorGUI.DisabledScope(fillList.Count == 0))
                    {
                        if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                        {
                            fillList.RemoveAt(fillList.Count - 1);
                            return;
                        }
                    }
                    GUILayout.Label("Image Fills");
                    GUILayout.FlexibleSpace();
                }

                foreach (ClockUtility.ClockFill fill in fillList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        fill.Fill = (Image)EditorGUILayout.ObjectField(fill.Fill, typeof(Image), true);
                        fill.Mode = (ClockHandMode)EditorGUILayout.EnumPopup(fill.Mode, GUILayout.Width(70));
                        if (GUILayout.Button("x", GUILayout.Width(30)))
                        {
                            fillList.Remove(fill);
                            return;
                        }
                    }
                }
            }

            GUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(30))) labelList.Add(new ClockUtility.ClockLabel());
                    using (new EditorGUI.DisabledScope(labelList.Count == 0))
                    {
                        if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                        {
                            labelList.RemoveAt(labelList.Count - 1);
                            return;
                        }
                    }
                    GUILayout.Label("Labels");
                    GUILayout.FlexibleSpace();
                }

                foreach (ClockUtility.ClockLabel label in labelList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        label.Label = (TextMeshProUGUI)EditorGUILayout.ObjectField(label.Label, typeof(TextMeshProUGUI), true);
                        label.Mode = (ClockLabelMode)EditorGUILayout.EnumPopup(label.Mode, GUILayout.Width(150));
                        if (GUILayout.Button("x", GUILayout.Width(30)))
                        {
                            labelList.Remove(label);
                            return;
                        }
                    }
                }
            }
        }

        private void PullFromScript()
        {
            handList.Clear();
            if (script.Hands != null && script.HandModes != null)
            {
                for (int i = 0; i < Mathf.Min(script.Hands.Length, script.HandModes.Length); i++)
                {
                    ClockUtility.ClockHand hand = new ClockUtility.ClockHand
                    {
                        Hand = script.Hands[i],
                        Mode = script.HandModes[i]
                    };

                    handList.Add(hand);
                }
            }
            
            fillList.Clear();
            if (script.Fills != null && script.FillModes != null)
            {
                for (int i = 0; i < Mathf.Min(script.Fills.Length, script.FillModes.Length); i++)
                {
                    ClockUtility.ClockFill fill = new ClockUtility.ClockFill
                    {
                        Fill = script.Fills[i],
                        Mode = script.FillModes[i]
                    };

                    fillList.Add(fill);
                }
            }
            
            labelList.Clear();
            if (script.Labels != null && script.LabelModes != null)
            {
                for (int i = 0; i < Mathf.Min(script.Labels.Length, script.LabelModes.Length); i++)
                {
                    ClockUtility.ClockLabel hand = new ClockUtility.ClockLabel
                    {
                        Label = script.Labels[i],
                        Mode = script.LabelModes[i]
                    };

                    labelList.Add(hand);
                }
            }
        }

        private void PushToScript()
        {
            List<Transform> hands = new List<Transform>();
            List<ClockHandMode> handModes = new List<ClockHandMode>();

            foreach (ClockUtility.ClockHand hand in handList)
            {
                hands.Add(hand.Hand);
                handModes.Add(hand.Mode);
            }
            
            List<Image> fills = new List<Image>();
            List<ClockHandMode> fillModes = new List<ClockHandMode>();

            foreach (ClockUtility.ClockFill fill in fillList)
            {
                fills.Add(fill.Fill);
                fillModes.Add(fill.Mode);
            }

            List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
            List<ClockLabelMode> labelModes = new List<ClockLabelMode>();

            foreach (ClockUtility.ClockLabel label in labelList)
            {
                labels.Add(label.Label);
                labelModes.Add(label.Mode);
            }

            Undo.RecordObject(script, "Pushed values to script");
            script.Hands = hands.ToArray();
            script.HandModes = handModes.ToArray();
            script.Fills = fills.ToArray();
            script.FillModes = fillModes.ToArray();
            script.Labels = labels.ToArray();
            script.LabelModes = labelModes.ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
    }
}