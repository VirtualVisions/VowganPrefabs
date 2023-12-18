
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Contact.Footsteps
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactFootstepOverride : ContactBehaviour
    {
        [Tooltip("Preset used to mimic a material.")] 
        [ContactFootstepPreset] public UnityEngine.Object Preset;

        public AudioClip[] FootstepClips;
        public AudioClip[] JumpClips;
        public AudioClip[] LandingClips;
    }
}