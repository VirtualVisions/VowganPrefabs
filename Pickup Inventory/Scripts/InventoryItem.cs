
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace Vowgan.Inventory
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InventoryItem : UdonSharpBehaviour
    {
        
        [Header("Item Settings")]
        public Sprite Icon;
        public string ItemName;
        public string ItemDescription;

        [Header("References")]
        public VRCPickup Pickup;
        public float StoredTimestamp;
        public bool JustSpawned;

        protected GameObject PickupObj;
        protected Rigidbody Rb;
        protected bool StartsKinematic;
        
        
        private void Start()
        {
            _Init();
        }
        
        public virtual void _Init()
        {
            Rb = Pickup.GetComponent<Rigidbody>();
            StartsKinematic = Rb.isKinematic;
            PickupObj = Pickup.gameObject;
        }

        public virtual void _Spawn(Transform point)
        {
            JustSpawned = true;
            Pickup.transform.SetPositionAndRotation(point.position, point.rotation);
            PickupObj.SetActive(true);
            Rb.isKinematic = true;
        }

        public virtual void _Hide()
        {
            Pickup.Drop();
            PickupObj.SetActive(false);
            Rb.velocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
            StoredTimestamp = Time.realtimeSinceStartup;
        }

        public virtual void _RunFirstPickupAfterSpawn()
        {
            Rb.isKinematic = StartsKinematic;
        }
        
    }
}