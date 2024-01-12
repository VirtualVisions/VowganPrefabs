
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Inventory
{
    [RequireComponent(typeof(Rigidbody))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InventoryInserter : UdonSharpBehaviour
    {
        
        
        public virtual void _Highlight(bool value)
        {
            
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) return;
            if (!Networking.LocalPlayer.IsOwner(other.gameObject)) return;

            PickupProxy proxy = other.GetComponent<PickupProxy>();
            if (!proxy) return;
            proxy.InsertingToInventory = true;

            _Highlight(true);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!Utilities.IsValid(other)) return;
            if (!Networking.LocalPlayer.IsOwner(other.gameObject)) return;

            PickupProxy proxy = other.GetComponent<PickupProxy>();
            if (!proxy) return;
            proxy.InsertingToInventory = false;

            _Highlight(false);
        }
    }
}