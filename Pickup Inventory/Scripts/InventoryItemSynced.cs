
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Vowgan.Inventory
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InventoryItemSynced : InventoryItem
    {

        [Header("Synced Data")]
        [UdonSynced] public bool Active;

        private VRCObjectSync objectSync;
        
        
        public override void _Init()
        {
            base._Init();
            objectSync = Pickup.GetComponent<VRCObjectSync>();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (Active) return;
            
            if (player.isLocal && player.isMaster)
            {
                _Spawn(Pickup.transform);
                objectSync.Respawn();
            }
        }

        public override void OnDeserialization()
        {
            Pickup.gameObject.SetActive(Active);
            if (!Active) Pickup.Drop();
        }

        public override void _Spawn(Transform point)
        {
            base._Spawn(point);
            Active = true;
            objectSync.SetKinematic(true);
            objectSync.FlagDiscontinuity();
            RequestSerialization();
        }

        public override void _Hide()
        {
            base._Hide();
            
            Active = false;
            RequestSerialization();
        }

        public override void _RunFirstPickupAfterSpawn()
        {
            base._RunFirstPickupAfterSpawn();
            objectSync.SetKinematic(StartsKinematic);
        }
    }
}