
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vowgan.Contact
{
    [CreateAssetMenu(fileName = "Surface Preset", menuName = "Contact/Surface Preset")]
    public class ContactSurfacePreset : ScriptableObject
    {

        public List<Material> Materials = new List<Material>();
        public List<AudioClip> Clips = new List<AudioClip>();


        private void Reset()
        {

#if UNITY_EDITOR
            List<Material> selectedMaterials = Selection.GetFiltered<Material>(SelectionMode.Assets).ToList();
            List<AudioClip> selectedClips = Selection.GetFiltered<AudioClip>(SelectionMode.Assets).ToList();

            selectedMaterials = selectedMaterials.OrderBy(x => x.name).ToList();
            selectedClips = selectedClips.OrderBy(x => x.name).ToList();

            Materials = selectedMaterials;
            Clips = selectedClips;
#endif


        }
    }
}