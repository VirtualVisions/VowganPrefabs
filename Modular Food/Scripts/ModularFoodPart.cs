
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ModularFood
{
    [RequireComponent(typeof(VRCPickup))]
    [RequireComponent(typeof(VRCObjectSync))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModularFoodPart : UdonSharpBehaviour
    {

        public ModularFoodList FoodList;
        public int PartIndex;
        
        private VRCPickup pickup;
        private VRCObjectSync objectSync;
        
        
        private void Start()
        {
            GameObject part = Instantiate(FoodList.FoodParts[PartIndex], transform);
            pickup = GetComponent<VRCPickup>();
            objectSync = GetComponent<VRCObjectSync>();
        }

        public void _Respawn()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            pickup.Drop();
            objectSync.Respawn();
        }

    }
}