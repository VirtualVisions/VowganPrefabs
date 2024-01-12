
using System;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Vowgan.Contact.Footsteps
{
    public class ContactFootsteps : ContactBehaviour
    {

        [ContactFootstepPreset] public UnityEngine.Object[] Presets;

        public Material[][] Materials;
        public int[][] MaterialIds;
        public AudioClip[][] FootstepClips;
        public AudioClip[][] JumpClips;
        public AudioClip[][] LandingClips;

        public bool UseFallbackPreset = true;

        public float MinimumLandingVelocity = 1;
        public LayerMask GroundLayers;

        public AnimationCurve FootstepRate;
        public AnimationCurve VolumeCurve;

        private ContactFootstepOverride presetOverride;
        private float footstepInterval;
        private Vector3 lastPlayerVelocity;
        private float lastPlayerVelocityMagnitude;
        private bool newGrounded;
        private bool grounded;
        private bool usingPreset;
        private float footstepTarget;
        private float fallingSpeed;

        private VRCPlayerApi localPlayer;
        private const float MINIMUM_TRIGGER_VELOCITY = 0.5f;


        protected override void _Init()
        {
            base._Init();
            localPlayer = Networking.LocalPlayer;

            MaterialIds = ContactUtilities.CaptureMaterialIds(Materials);
        }

        private void Update()
        {
            newGrounded = localPlayer.IsPlayerGrounded();

            if (grounded)
            {
                lastPlayerVelocityMagnitude = lastPlayerVelocity.magnitude;

                if (lastPlayerVelocityMagnitude >= MINIMUM_TRIGGER_VELOCITY)
                {
                    footstepInterval += Time.deltaTime;
                    footstepTarget = FootstepRate.Evaluate(lastPlayerVelocityMagnitude);
                    if (footstepInterval >= footstepTarget)
                    {
                        footstepInterval = 0;

                        PlayFootstep();
                    }
                }
            }
            else
            {
                if (newGrounded)
                {
                    fallingSpeed = Mathf.Abs(lastPlayerVelocity.y);
                    if (fallingSpeed >= MinimumLandingVelocity)
                    {
                        PlayLanding();
                    }
                }
            }

            grounded = newGrounded;
            lastPlayerVelocity = localPlayer.GetVelocity();
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!value) return;

            if (localPlayer.IsPlayerGrounded())
            {
                PlayJumping();
                footstepInterval = 0;
            }
        }

        private void PlayFootstep()
        {
            CheckGround();

            AudioClip[] clips = usingPreset ? presetOverride.FootstepClips : FootstepClips[CurrentId];
            
            ContactAudio._PlaySound(
                clips,
                localPlayer.GetPosition(),
                ContactAudioPlayer.DEFAULT_MAX_DISTANCE,
                VolumeCurve.Evaluate(lastPlayerVelocityMagnitude));
        }

        private void PlayJumping()
        {
            CheckGround();

            AudioClip[] clips = usingPreset ? presetOverride.JumpClips : JumpClips[CurrentId];
            
            ContactAudio._PlaySound(
                clips,
                localPlayer.GetPosition(),
                ContactAudioPlayer.DEFAULT_MAX_DISTANCE,
                VolumeCurve.Evaluate(lastPlayerVelocityMagnitude));
        }

        private void PlayLanding()
        {
            CheckGround();
            
            AudioClip[] clips = usingPreset ? presetOverride.LandingClips : LandingClips[CurrentId];
            AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            
            ContactAudio._PlaySound(clip, localPlayer.GetPosition());
        }

        private void CheckGround()
        {
            Vector3 position = localPlayer.GetPosition() + (Vector3.up / 2f);
            bool foundSurface = false;

            if (UnityEngine.Physics.Raycast(position, Vector3.down, out RaycastHit hit, 1, GroundLayers))
            {
                presetOverride = hit.transform.GetComponent<ContactFootstepOverride>();
                usingPreset = presetOverride;
                if (presetOverride) return;
                
                MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();
                if (!meshRenderer) return;

                int instanceId = meshRenderer.sharedMaterial.GetInstanceID();

                for (int i = 0; i < MaterialIds.Length; i++)
                {
                    if (Array.IndexOf(MaterialIds[i], instanceId) != -1)
                    {
                        CurrentId = i;
                        foundSurface = true;
                    }
                }
            }

            if (!foundSurface && UseFallbackPreset)
            {
                CurrentId = 0;
            }

        }
    }
}