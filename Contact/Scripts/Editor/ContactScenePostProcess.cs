
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using Vowgan.Contact.Footsteps;
using Vowgan.Contact.Physics;

namespace Vowgan.Contact
{
    public class ContactScenePostProcess : MonoBehaviour
    {

        private static ContactAudioPlayer contactAudioPlayer;


        [PostProcessScene(-100)]
        public static void ScenePostProcess()
        {
            contactAudioPlayer = FindObjectOfType<ContactAudioPlayer>();
            if (!contactAudioPlayer) return;

            Listeners();
            ContactPool();
            PhysicsPresets();
            FootstepPresets();
            GunshotsPresets();
        }

        private static void Listeners()
        {
            ContactBehaviour[] listeners = FindObjectsOfType<ContactBehaviour>();
            foreach (ContactBehaviour listener in listeners)
            {
                listener.ContactAudio = contactAudioPlayer;
            }
        }

        private static void ContactPool()
        {
            List<AudioSource> audioSourceList = new List<AudioSource>();
            List<GameObject> gameObjectList = new List<GameObject>();
            List<Transform> transformList = new List<Transform>();

            for (int i = 0; i < contactAudioPlayer.SourceCount; i++)
            {
                GameObject obj = Instantiate(contactAudioPlayer.AudioPrefab, contactAudioPlayer.transform);
                obj.name = $"{contactAudioPlayer.AudioPrefab.name} {i}";

                audioSourceList.Add(obj.GetComponent<AudioSource>());
                gameObjectList.Add(obj);
                transformList.Add(obj.transform);
            }

            contactAudioPlayer.AudioSourceList = audioSourceList.ToArray();
            contactAudioPlayer.GameObjectList = gameObjectList.ToArray();
            contactAudioPlayer.TransformList = transformList.ToArray();
        }

        private static void PhysicsPresets()
        {
            ContactCollisionObject[] physicsObjects = FindObjectsOfType<ContactCollisionObject>();

            foreach (ContactCollisionObject physicsObject in physicsObjects)
            {
                ContactCollisionPreset lightPreset = (ContactCollisionPreset)physicsObject.LightPreset;
                physicsObject.LightClips = lightPreset.Clips.ToArray();
                physicsObject.LightPitchRange = lightPreset.PitchRange;
                physicsObject.LightVolumeRange = lightPreset.VolumeRange;
                physicsObject.LightMinimumVelocity = lightPreset.MinimumVelocity;
                physicsObject.LightPreset = null;

                ContactCollisionPreset mediumPreset = (ContactCollisionPreset)physicsObject.MediumPreset;
                physicsObject.MediumClips = mediumPreset.Clips.ToArray();
                physicsObject.MediumPitchRange = mediumPreset.PitchRange;
                physicsObject.MediumVolumeRange = mediumPreset.VolumeRange;
                physicsObject.MediumMinimumVelocity = mediumPreset.MinimumVelocity;
                physicsObject.MediumPreset = null;

                ContactCollisionPreset heavyPreset = (ContactCollisionPreset)physicsObject.HeavyPreset;
                physicsObject.HeavyClips = heavyPreset.Clips.ToArray();
                physicsObject.HeavyPitchRange = heavyPreset.PitchRange;
                physicsObject.HeavyVolumeRange = heavyPreset.VolumeRange;
                physicsObject.HeavyMinimumVelocity = heavyPreset.MinimumVelocity;
                physicsObject.HeavyPreset = null;
            }
        }

        private static void FootstepPresets()
        {
            ContactFootsteps contactFootsteps = FindObjectOfType<ContactFootsteps>();
            if (contactFootsteps)
            {
                List<Material[]> materials = new List<Material[]>();
                List<AudioClip[]> footsteps = new List<AudioClip[]>();
                List<AudioClip[]> jumpClips = new List<AudioClip[]>();
                List<AudioClip[]> landingClips = new List<AudioClip[]>();

                foreach (UnityEngine.Object preset in contactFootsteps.Presets)
                {
                    ContactFootstepPreset footstepPresets = (ContactFootstepPreset)preset;
                    materials.Add(footstepPresets.Materials.ToArray());
                    footsteps.Add(footstepPresets.Clips.ToArray());
                    jumpClips.Add(footstepPresets.JumpClips.ToArray());
                    landingClips.Add(footstepPresets.LandingClips.ToArray());
                }

                contactFootsteps.Materials = materials.ToArray();
                contactFootsteps.FootstepClips = footsteps.ToArray();
                contactFootsteps.JumpClips = jumpClips.ToArray();
                contactFootsteps.LandingClips = landingClips.ToArray();

                contactFootsteps.Presets = null;
            }

            ContactFootstepOverride[] overrides = FindObjectsOfType<ContactFootstepOverride>();
            foreach (ContactFootstepOverride footstepOverride in overrides)
            {
                footstepOverride.FootstepClips = ((ContactFootstepPreset)footstepOverride.Preset).Clips.ToArray();
                footstepOverride.JumpClips = ((ContactFootstepPreset)footstepOverride.Preset).JumpClips.ToArray();
                footstepOverride.LandingClips = ((ContactFootstepPreset)footstepOverride.Preset).LandingClips.ToArray();

                footstepOverride.Preset = null;
            }
        }

        private static void GunshotsPresets()
        {
            Gunshots.ContactGunshots contactGunshots = FindObjectOfType<Gunshots.ContactGunshots>();
            if (contactGunshots)
            {
                List<Material[]> materials = new List<Material[]>();
                List<AudioClip[]> clips = new List<AudioClip[]>();

                foreach (UnityEngine.Object preset in contactGunshots.Presets)
                {
                    ContactSurfacePreset surfacePreset = (ContactSurfacePreset)preset;
                    materials.Add(surfacePreset.Materials.ToArray());
                    clips.Add(surfacePreset.Clips.ToArray());
                }

                contactGunshots.Materials = materials.ToArray();
                contactGunshots.Clips = clips.ToArray();

                contactGunshots.Presets = null;
            }
        }
    }
}