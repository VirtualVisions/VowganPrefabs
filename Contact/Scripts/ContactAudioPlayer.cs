
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Contact
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactAudioPlayer : UdonSharpBehaviour
    {

        public GameObject AudioPrefab;
        public int SourceCount = 64;

        [HideInInspector] public ContactInstance[] Instances;

        private VRCPlayerApi localPlayer;
        private int lastPlayedIndex = -1;
        

        private void Update()
        {
            for (int i = 0; i < Instances.Length; i++)
            {
                ContactInstance instance = Instances[i];
                if (!instance.IsPlaying) continue;

                float instanceTime = Time.timeSinceLevelLoad - instance.StartTime;
                
                if (instanceTime >= instance.ClipLength)
                {
                    instance._ReturnToPool();
                }
            }
        }
        
        public ContactInstance _PlaySoundChilded(AudioClip clip, Transform parent, Vector3 position, float maxDistance = 32, float volume = 1, float pitch = 1)
        {
            ContactInstance instance = TryGetSource();
            if (instance == null) return null;
            
            instance._ChildTo(parent);
            instance._PlaySound(clip, position, maxDistance, volume, pitch);
            
            return instance;
        }
        
        public ContactInstance _PlaySound(AudioClip[] clips, Vector3 position, float maxDistance = 32, float volume = 1, float pitch = 1)
        {
            if (clips == null || clips.Length == 0) return null;
            AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            
            ContactInstance instance = _PlaySound(clip, position, maxDistance, volume, pitch);
            return instance;
        }
        
        public ContactInstance _PlaySound(AudioClip clip, Vector3 position, float maxDistance = 32, float volume = 1, float pitch = 1)
        {
            if (clip == null)
            {
                _Log("Tried to play null clip.");
                return null;
            }
            
            ContactInstance instance = TryGetSource();
            if (instance == null) return null;
            
            if (instance._PlaySound(clip, position, maxDistance, volume, pitch))
            {
                return instance;
            }
            else
            {
                return null;
            }
        }

        private ContactInstance TryGetSource()
        {
            for (int i = 0; i < SourceCount; i++)
            {
                lastPlayedIndex++;
                if (lastPlayedIndex >= SourceCount) lastPlayedIndex = 0;
                
                ContactInstance instance = Instances[lastPlayedIndex];
                if (instance.IsPlaying) continue;

                return instance;
            }

            _Log("No available Audio Sources found.");
            return null;
        }
        
        public void _Log(string log)
        {
            Debug.Log($"<color=yellow>{this.name}:</color> {log}", this);
        }
    }
}
