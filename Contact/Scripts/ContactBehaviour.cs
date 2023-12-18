
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Contact
{
    public abstract class ContactBehaviour : UdonSharpBehaviour
    {
        [HideInInspector] public ContactAudioPlayer ContactAudio;

        protected bool Initialized;
        protected int CurrentId;


        private void Start() => _Init();

        protected virtual void _Init()
        {
            if (Initialized) return;
            Initialized = true;
        }
        
        protected void _Log(string log)
        {
            Debug.Log($"<color=yellow>{this.name}:</color> {log}");
        }
    }
}