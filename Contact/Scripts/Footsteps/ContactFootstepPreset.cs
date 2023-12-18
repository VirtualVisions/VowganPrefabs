using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vowgan.Contact.Footsteps
{
    [CreateAssetMenu(fileName = "Footstep Preset", menuName = "Contact/Footstep Preset")]
    public class ContactFootstepPreset : ContactSurfacePreset
    {
        
        public List<AudioClip> JumpClips = new List<AudioClip>();
        public List<AudioClip> LandingClips = new List<AudioClip>();

    }
}