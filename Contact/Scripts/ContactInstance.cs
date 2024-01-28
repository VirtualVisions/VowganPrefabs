
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Contact
{
    [RequireComponent(typeof(AudioSource))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactInstance : UdonSharpBehaviour
    {

        public int Index; 
        public bool IsPlaying;
        public bool IsChilded;
        public float StartTime;
        public float ClipLength;

        [HideInInspector] public ContactAudioPlayer AudioPlayer;
        [HideInInspector] public AudioSource Source;

        private VRCPlayerApi localPlayer;
        
        
        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            Source = GetComponent<AudioSource>();
            gameObject.SetActive(false);
        }
        
        public bool _PlaySound(AudioClip clip, Vector3 position, float maxDistance = 32, float volume = 1, float pitch = 1)
        {
            if (!clip)
            {
                _ReturnToPool();
                return false;
            }

            if (Vector3.Distance(localPlayer.GetPosition(), position) > maxDistance) 
            {
                _ReturnToPool();
                return false;
            }

            
            IsPlaying = true;
            StartTime = Time.timeSinceLevelLoad;
            ClipLength = clip.length;
            
            Source.clip = clip;
            Source.maxDistance = maxDistance;
            Source.volume = volume;
            Source.pitch = pitch;

            transform.position = position;
            gameObject.SetActive(true);
            
            return true;
        }
        
        public void _ReturnToPool()
        {
            IsPlaying = false;

            if (IsChilded)
            {
                IsChilded = false;
                transform.SetParent(AudioPlayer.transform);
                SetSiblingIndex(Index);
            }

            StartTime = 0;
            ClipLength = 0;
            
            gameObject.SetActive(false);
        }
        
        public void _ChildTo(Transform parent)
        {
            IsChilded = true;
            transform.SetParent(parent);
        }

        public void SetSiblingIndex(int index)
        {
            transform.SetSiblingIndex(index);
        }
        
    }
}