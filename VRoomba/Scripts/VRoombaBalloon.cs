
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Vowgan
{
    public class VRoombaBalloon : UdonSharpBehaviour
    {
        private AudioSource source;
        private GameObject visual;
        private bool triggered;

        private void Start()
        {
            visual = transform.GetChild(0).gameObject;
            source = GetComponentInChildren<AudioSource>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other.gameObject)) return;
            if (!triggered && other.gameObject.name == "Knife")
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Knife));
            }
            else if (other.name.Contains("ReBalloon"))
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReBalloon));
            }
        }
        
        public void Knife()
        {
            triggered = true;
            source.Play();
            visual.SetActive(false);
        }

        public void ReBalloon()
        {
            triggered = false;
            visual.SetActive(true);
        }
        
    }
}