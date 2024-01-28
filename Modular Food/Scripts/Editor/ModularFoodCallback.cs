using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;


namespace Vowgan.ModularFood
{
    public class ModularFoodCallback : MonoBehaviour
    {
        [PostProcessScene(-100)]
        public static void OnPostProcessScene()
        {
            ModularFoodList footList = FindObjectOfType<ModularFoodList>();
            if (footList == null) return;

            ModularFoodPart[] partsList = FindObjectsOfType<ModularFoodPart>();
            foreach (ModularFoodPart part in partsList)
            {
                part.FoodList = footList;
            }
            
            ModularFoodPlate[] plateList = FindObjectsOfType<ModularFoodPlate>();
            foreach (ModularFoodPlate plate in plateList)
            {
                plate.FoodList = footList;
            }
        }
    }
}