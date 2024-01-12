
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Inventory
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AddItemsToInventory : UdonSharpBehaviour
    {
        
        public InventoryItem[] Items;
        public PickupInventory Inventory;
        
        [UdonSynced] public bool Used;
        
        
        private void Start()
        {
            foreach (InventoryItem item in Items)
            {
                if (Networking.LocalPlayer.IsOwner(item.gameObject))
                {
                    item._Hide();
                }
            }
        }

        public override void OnDeserialization()
        {
            DisableInteractive = Used;
        }

        public override void Interact()
        {
            foreach (InventoryItem item in Items)
            {
                Inventory._AddItem(item);
            }

            Used = true;
            RequestSerialization();
            OnDeserialization();
        }
    }
}