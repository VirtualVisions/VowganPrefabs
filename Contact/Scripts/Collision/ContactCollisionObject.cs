
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer;

namespace Vowgan.Contact.SoundPhysics
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactCollisionObject : ContactBehaviour
    {
        [Tooltip("Preset used for lighter collisions.")]
        [ContactCollisionPreset] public UnityEngine.Object LightPreset;
        [Tooltip("Preset used for heavier collisions.")]
        [ContactCollisionPreset] public UnityEngine.Object MediumPreset;
        [Tooltip("Preset used for heavier collisions.")]
        [ContactCollisionPreset] public UnityEngine.Object HeavyPreset;
        [Tooltip("Max distance from the player an audio effect will trigger.")]
        public float MaxDistance = 32;
        
        public float LightMinimumVelocity;
        public float MediumMinimumVelocity;
        public float HeavyMinimumVelocity;
        public AudioClip[] LightClips;
        public AudioClip[] MediumClips;
        public AudioClip[] HeavyClips;
        public Vector2 LightVolumeRange;
        public Vector2 MediumVolumeRange;
        public Vector2 HeavyVolumeRange;
        public Vector2 LightPitchRange;
        public Vector2 MediumPitchRange;
        public Vector2 HeavyPitchRange;
        
        private void OnCollisionEnter(Collision other)
        {
            float relativeMagnitude = other.relativeVelocity.magnitude;
            CollisionIntensityState state = GetIntensity(relativeMagnitude);

            switch (state)
            {
                default:
                case CollisionIntensityState.None:
                    break;
                case CollisionIntensityState.Light:
                    PlayClip(LightClips, other.GetContact(0).point, LightVolumeRange, LightPitchRange);
                    break;
                case CollisionIntensityState.Medium:
                    PlayClip(MediumClips, other.GetContact(0).point, MediumVolumeRange, MediumPitchRange);
                    break;
                case CollisionIntensityState.Heavy:
                    PlayClip(HeavyClips, other.GetContact(0).point, HeavyVolumeRange, HeavyPitchRange);
                    break;
            }
        }
        
        protected virtual void PlayClip(AudioClip[] clips, Vector3 position, Vector2 volumeRange, Vector2 pitchRange)
        {
            int index = UnityEngine.Random.Range(0, clips.Length);
            AudioClip clip = clips[index];
            float volume = UnityEngine.Random.Range(volumeRange.x, volumeRange.y);
            float pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);

            ContactAudio._PlaySound(clip, position, MaxDistance, volume, pitch);
        }

        protected virtual CollisionIntensityState GetIntensity(float magnitude)
        {
            if (magnitude > HeavyMinimumVelocity) return CollisionIntensityState.Heavy;
            if (magnitude > MediumMinimumVelocity) return CollisionIntensityState.Medium;
            if (magnitude > LightMinimumVelocity) return CollisionIntensityState.Light;
            return CollisionIntensityState.None;
        }
    }

    public enum CollisionIntensityState
    {
        None,
        Light,
        Medium,
        Heavy,
    }
}