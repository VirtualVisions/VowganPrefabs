using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Vowgan.ModularFood
{
    [CustomEditor(typeof(ModularFoodPart))]
    public class ModularFoodPartEditor : Editor
    {

        private ModularFoodPart script;
        
        private void OnEnable()
        {
            script = target as ModularFoodPart;
            if (script == null) return;
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            if (!PrefabUtility.IsPartOfPrefabAsset(target))
            {
                if (script.FoodList == null)
                {
                    ModularFoodList list = FindObjectOfType<ModularFoodList>();
                    if (list)
                    {
                        Undo.RecordObject(script, "Added List");
                        script.FoodList = list;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(script);
                    }
                    else
                    {
                        base.OnInspectorGUI();
                        return;
                    }
                }
                
                List<string> partNames = new List<string>();
                foreach (GameObject part in script.FoodList.FoodParts)
                {
                    partNames.Add(part.name);
                }

                EditorGUI.BeginChangeCheck();
                int index = EditorGUILayout.Popup("Part Index", script.PartIndex, partNames.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(script, "Updated Index");
                    script.PartIndex = index;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(script);
                }
            }
            
            base.OnInspectorGUI();
        }
    }
}