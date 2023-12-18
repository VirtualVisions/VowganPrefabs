
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Vowgan.Contact.Physics
{
    [CreateAssetMenu(fileName = "Collision Preset", menuName = "Contact/Collision Preset")]
    public class ContactCollisionPreset : ScriptableObject
    {

        [Tooltip("The random range for variation in the sound effect volume.")] [MinMax]
        public Vector2 VolumeRange = new Vector2(0.8f, 1f);

        [Tooltip("The random range for variation in the sound effect pitch.")] [MinMax]
        public Vector2 PitchRange = new Vector2(0.9f, 1.1f);

        [Tooltip("The minimum relative velocity required to trigger the sound effect.")]
        public float MinimumVelocity = 0;

        [Tooltip("Audio clips used for the collision sound effects.")]
        public List<AudioClip> Clips;


        private void Reset()
        {

#if UNITY_EDITOR
            List<AudioClip> selectedClips = Selection.GetFiltered<AudioClip>(SelectionMode.Assets).ToList();
            selectedClips = selectedClips.OrderBy(x => x.name).ToList();
            Clips = selectedClips;
#endif

        }
    }
}