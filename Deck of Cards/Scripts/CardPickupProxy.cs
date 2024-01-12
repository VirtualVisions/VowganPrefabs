
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.DeckOfCards
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CardPickupProxy : UdonSharpBehaviour
    {
        
        [HideInInspector] public CardLogic Card;
        
        private void Start()
        {
            Card = GetComponentInChildren<CardLogic>();
        }
        
        public override void OnPickup()
        {
            Card._OnPickup();
        }
        
        public override void OnDrop()
        {
            Card._OnDrop();
        }

        private void OnTriggerEnter(Collider other)
        {
            Card._OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            Card._OnTriggerExit(other);
        }
    }
}