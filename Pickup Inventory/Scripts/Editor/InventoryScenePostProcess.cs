using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.Udon;

namespace Vowgan.Inventory
{
    public class InventoryScenePostProcess : MonoBehaviour
    {
        
        [PostProcessScene(-100)]
        public static void PostProcessScene()
        {
            PickupInventory inventory = FindObjectOfType<PickupInventory>();
            
            foreach (InventoryItem item in FindObjectsOfType<InventoryItem>())
            {
                if (!item.Pickup) continue;

                PickupProxy proxy = item.Pickup.gameObject.AddUdonSharpComponent<PickupProxy>();
                proxy.Item = item;
                proxy.Inventory = inventory;

                UdonBehaviour udonProxy = UdonSharpEditorUtility.GetBackingUdonBehaviour(proxy);

                if (EditorApplication.isPlaying)
                {
                    UdonManager.Instance.RegisterUdonBehaviour(udonProxy);
                }
                
            }
            
        }
    }
}