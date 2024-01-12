
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;


namespace Vowgan.Inventory
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TakeItemsFromInventory : UdonSharpBehaviour
    {
        
        public PickupInventory Inventory;
        public string[] ItemsToRemove = new[] { "Apple", "Apple", "Apple" };
        
        
        public override void Interact()
        {
            DataList foundItemList = new DataList();
            DataList cachedItemList = Inventory.ItemList.DeepClone();

            for (int x = 0; x < ItemsToRemove.Length; x++)
            {
                string itemName = ItemsToRemove[x];
                for (int y = 0; y < cachedItemList.Count; y++)
                {
                    InventoryItem item = (InventoryItem)cachedItemList[y].DataDictionary[PickupInventory.ID_ITEM].Reference;
                    if (item.ItemName == itemName)
                    {
                        foundItemList.Add(cachedItemList[y].DataDictionary);
                        cachedItemList.Remove(cachedItemList[y].DataDictionary);
                        break;
                    }
                }
            }

            if (foundItemList.Count != ItemsToRemove.Length)
            {
                Debug.Log("Did not find all required items in inventory.");
                return;
            }
            
            for (int i = 0; i < foundItemList.Count; i++)
            {
                InventoryItem item = (InventoryItem)foundItemList[i].DataDictionary[PickupInventory.ID_ITEM].Reference;
                Inventory._RemoveItem(item);
            }
        }
    }
}