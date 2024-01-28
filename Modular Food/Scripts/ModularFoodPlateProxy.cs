
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ModularFood
{
    [RequireComponent(typeof(VRCObjectSync))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModularFoodPlateProxy : UdonSharpBehaviour
    {

        public ModularFoodPlate Plate;
        private VRCPickup pickup;
        private VRCObjectSync objectSync;


        private void Start()
        {
            pickup = GetComponent<VRCPickup>();
            objectSync = GetComponent<VRCObjectSync>();
        }

        public override void OnPickup()
        {
            Networking.SetOwner(Networking.LocalPlayer, Plate.gameObject);
        }

        public override void OnPickupUseDown()
        {
            Plate._OnPickupUseDown();
        }

        public void _Respawn()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            pickup.Drop();
            objectSync.Respawn();
        }
    }
}