
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Vowgan.DeckOfCards
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CardLogic : UdonSharpBehaviour
    {
        
        public DeckManager DeckManager;
        [HideInInspector] [UdonSynced] public bool Grabbed;
        [HideInInspector] public bool UseGravity;
        
        private VRCPickup pickup;
        private VRCObjectSync sync;
        private bool toBeReturned;
        private bool initialized;
        
        
        private void Start()
        {
            if (!initialized) Init();
        }

        private void Init()
        {
            initialized = true;
            pickup = transform.parent.GetComponent<VRCPickup>();
            sync = pickup.GetComponent<VRCObjectSync>();
        }
        
        public void _OnPickup()
        {
            if (!initialized) Init();
            if (Grabbed) return;
            Grabbed = true;
            toBeReturned = false;
            sync.SetKinematic(!UseGravity);
            DeckManager.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(DeckManager.NextCard));
        }
        
        public void _OnTriggerEnter(Collider other)
        {
            if (other.transform == DeckManager.Deck)
            {
                toBeReturned = true;
            }
        }
        
        public void _OnTriggerExit(Collider other)
        {
            if (other.transform == DeckManager.Deck)
            {
                toBeReturned = false;
            }
        }
        
        public void _Drop()
        {
            if (!initialized) Init();
            pickup.Drop();
        }
        
        public void _OnDrop()
        {
            if (!initialized) Init();
            if (toBeReturned)
            {
                Grabbed = false;
                DeckManager._ReturnCard(pickup.gameObject);
            }
        }
    }
}