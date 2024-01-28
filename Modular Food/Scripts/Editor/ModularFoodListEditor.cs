using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Vowgan.ModularFood
{
    [CustomEditor(typeof(ModularFoodList))]
    public class ModularFoodListEditor : Editor
    {

        private ModularFoodList script;
        private List<FoodList> foodList;
        private ReorderableList foodListLayout;


        private void OnEnable()
        {
            script = target as ModularFoodList;
            if (script == null) return;

            Undo.undoRedoPerformed += PullFromScript;
            PullFromScript();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= PullFromScript;
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUI.BeginChangeCheck();
            foodListLayout.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                PushToScript();
            }
            base.OnInspectorGUI();
        }

        private void PullFromScript()
        {
            foodList = new List<FoodList>();

            int length = Mathf.Min(script.FoodParts?.Length ?? 0, script.PartHeights?.Length ?? 0);

            for (int i = 0; i < length; i++)
            {
                foodList.Add(new FoodList()
                {
                    Part = script.FoodParts[i],
                    Height = script.PartHeights[i],
                });
            }

            foodListLayout = new ReorderableList(foodList, typeof(FoodList))
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = DrawHeaderCallback
            };
        }

        private void PushToScript()
        {
            List<GameObject> foodParts = new List<GameObject>();
            List<float> partHeights = new List<float>();

            foreach (FoodList food in foodList)
            {
                foodParts.Add(food.Part);
                partHeights.Add(food.Height);
            }

            Undo.RecordObject(script, "Updated food parts");
            script.FoodParts = foodParts.ToArray();
            script.PartHeights = partHeights.ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(script);
        }

        private void DrawHeaderCallback(Rect rect)
        {
            Rect partRect = rect;
            partRect.width -= 50;
            
            EditorGUI.LabelField(partRect, "Food Part");

            Rect heightRect = rect;
            heightRect.position += new Vector2(partRect.width + 2, 0);
            heightRect.width = 48;

            EditorGUI.LabelField(heightRect, "Height");
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 2;
            
            Rect partRect = rect;
            partRect.width -= 50;
            
            foodList[index].Part = (GameObject)EditorGUI.ObjectField(partRect, foodList[index].Part, typeof(GameObject), true);

            Rect heightRect = rect;
            heightRect.position += new Vector2(partRect.width + 2, 0);
            heightRect.width = 48;
            
            foodList[index].Height = EditorGUI.FloatField(heightRect, foodList[index].Height);
        }
    }

    public class FoodList
    {
        public GameObject Part;
        public float Height;
    }

}