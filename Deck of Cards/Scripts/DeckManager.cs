
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
    public class DeckManager : UdonSharpBehaviour
    {
        
        [UdonSynced, SerializeField, FieldChangeCallback(nameof(CardCount))] private int cardCount;
        public int CardCount
        {
            get => cardCount;
            set
            {
                cardCount = value;
                
                if (cardCount <= 0)
                {
                    Deck.localScale = Vector3.zero;
                }
                else
                {
                    Deck.localScale = new Vector3(1, cardCount + 0.002f, 1);
                }
            }
        }
        [UdonSynced] public int CardCurrent;
        [UdonSynced] public bool UseGravity;
        
        [Header("References")]
        public Transform Deck;
        [HideInInspector] public VRCObjectPool Pool;
        
        private VRCPlayerApi playerLocal;
        private CardLogic[] cards;
        private GameObject currentCard;
        
        
        private void Start()
        {
            playerLocal = Networking.LocalPlayer;
            Pool = GetComponent<VRCObjectPool>();
            
            cards = new CardLogic[Pool.Pool.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = Pool.Pool[i].GetComponentInChildren<CardLogic>();
                cards[i].UseGravity = UseGravity;
            }
            
            if (Networking.IsOwner(gameObject)) SendCustomEventDelayedSeconds(nameof(_ResetDeck), 1);
        }
        
        public void NextCard()
        {
            if (CardCurrent >= Pool.Pool.Length - 1)
            {
                Deck.localScale = Vector3.zero;
            }
            else
            {
                CardCount -= 1;
                CardCurrent += 1;
                RequestSerialization();
                
                Networking.SetOwner(playerLocal, Pool.gameObject);
                currentCard = Pool.TryToSpawn();
                Networking.SetOwner(playerLocal, currentCard);
                
                SetCurrentCardToTop();
            }
        }
        
        public void _ResetDeck()
        {
            Networking.SetOwner(playerLocal, gameObject);
            
            foreach (CardLogic card in cards)
            {
                Networking.SetOwner(playerLocal, card.gameObject);
                card.Grabbed = false;
                card.RequestSerialization();
                Pool.Return(card.transform.parent.gameObject);
                if (!card.gameObject.activeSelf) continue;
                card._Drop();
            }
            
            CardCurrent = -1;
            CardCount = Pool.Pool.Length;
            
            Pool.Shuffle();
            NextCard();
        }

        public void _ReturnCard(GameObject card)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            CardCount += 1;
            CardCurrent -= 1;
            Pool.Return(card);
            SetCurrentCardToTop();
        }

        private void SetCurrentCardToTop()
        {
            if (!currentCard) return;
            currentCard.transform.localPosition = new Vector3(0,  cardCount * 0.002f, 0);
            VRCObjectSync sync = currentCard.GetComponent<VRCObjectSync>();
            if (sync)
            {
                sync.SetKinematic(true);
                sync.FlagDiscontinuity();
            }
        }
    }
}