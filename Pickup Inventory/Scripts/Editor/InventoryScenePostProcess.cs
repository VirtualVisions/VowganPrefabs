
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
        
        
        [MenuItem ("CONTEXT/VRCPickup/Create Inventory Item")]
        private static void CreateInventoryItem (MenuCommand command) {
            VRCPickup pickup = (VRCPickup)command.context;
            if (PrefabUtility.IsPartOfImmutablePrefab(pickup)) return;
            
            foreach (InventoryItem invItem in FindObjectsOfType<InventoryItem>())
            {
                if (invItem.Pickup == pickup)
                {
                    EditorUtility.DisplayDialog("Inventory Pickups", $"{pickup.gameObject} is already an inventory item.", "Return");
                    Selection.objects = new[] { invItem.gameObject };
                    return;
                }
            }

            GameObject itemObj = new() { name = pickup.gameObject.name + " Item" };
            Transform itemTrans = itemObj.transform;
            Transform pickupTrans = pickup.transform;
            
            if (!PrefabUtility.IsPartOfAnyPrefab(pickup))
            {
                itemTrans.SetParent(pickupTrans.parent);
                itemTrans.SetPositionAndRotation(pickupTrans.position, pickupTrans.rotation);
                pickupTrans.SetParent(itemTrans);
            }

            InventoryItem item;

            if (pickup.GetComponent<VRCObjectSync>())
            {
                item = itemObj.AddUdonSharpComponent<InventoryItemSynced>();
            }
            else
            {
                item = itemObj.AddUdonSharpComponent<InventoryItem>();
            }
            
            item.Pickup = pickup;

            Undo.RegisterCreatedObjectUndo(itemObj, "Created Inventory Item");
            
            Selection.objects = new[] { itemObj };
        }
        
    }
}
