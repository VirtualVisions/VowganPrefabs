
using System;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace Vowgan
{
    public enum VRoombaState
    {
        Idle,
        Active,
        Righting,
    }

    public enum VRoombaTurnState
    {
        None,
        Left,
        Right,
    }
    
    public class VRoomba : UdonSharpBehaviour
    {

        [UdonSynced] public VRoombaState State;
        public VRoombaTurnState TurnState;
        
        public float MoveSpeed = 0.5f;
        public float TurnSpeed = 1f;
        public float DecisionDelay = 0.2f;
        public float PainThreshold = 1.0f;
        public float UprightThreshold = 0.4f;
        
        public AudioSource SourceEffects;
        public AudioClip[] ClipsBonk;
        public VRoombaSensor SensorLeft;
        public VRoombaSensor SensorRight;
        
        [Header("Vocals")] 
        public bool UseVoice = true;
        public AudioClip[] VoiceCollide;
        public AudioClip[] VoiceEat;
        public AudioClip[] VoiceFallen;
        public AudioClip[] VoiceGrabAccept;
        public AudioClip[] VoiceGrabDecline;

        private VRCPlayerApi localPlayer;
        private Rigidbody rb;
        private Animator anim;
        private VRCPickup pickup;
        private float baseMoveSpeed;
        private float nextVocalTime;
        private float nextImpactTime;
        private float nextTurnCheckTime;
        private int speedIncreaseCount;
        private bool upright;
        private bool isOwner;
        
        private int hashState;


        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            pickup = GetComponent<VRCPickup>();

            baseMoveSpeed = MoveSpeed;
            
            hashState = Animator.StringToHash("State");

            OnDeserialization();
        }

        public override void OnDeserialization()
        {
            anim.SetInteger(hashState, (int)State);
        }

        private void FixedUpdate()
        {
            isOwner = localPlayer.IsOwner(gameObject);
            if (!isOwner) return;
            
            if (pickup.IsHeld) return;
            
            switch (State)
            {
                case VRoombaState.Idle:
                    break;
                case VRoombaState.Active:
                    upright = Vector3.Dot(transform.up, Vector3.up) > UprightThreshold;
                    if (!upright)
                    {
                        State = VRoombaState.Righting;
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsFallen));
                        OnDeserialization();
                    }
                    HandleMovement();
                    break;
                case VRoombaState.Righting:
                    break;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isOwner) return;
            if (State != VRoombaState.Active) return;
            if (!Utilities.IsValid(other.gameObject)) return;
            
            VRCObjectSync objectSync = other.gameObject.GetComponent<VRCObjectSync>();
            if (objectSync)
            {
                if (!other.gameObject.GetComponent<VRoomba>())
                {
                    Networking.SetOwner(localPlayer, objectSync.gameObject);
                    objectSync.Respawn();
                    if (UseVoice) SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsEat));
                    MoveSpeed = baseMoveSpeed * 2;
                    speedIncreaseCount += 1;
                    SendCustomEventDelayedSeconds(nameof(EndSpeed), 5);
                }
            }
            
            if (Time.timeSinceLevelLoad < nextImpactTime) return;
            nextImpactTime = Time.timeSinceLevelLoad + 2;
            
            float impact = other.relativeVelocity.magnitude;

            if (impact > 0.01f)
            {
                if (impact > PainThreshold && UseVoice)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsCollide));
                }
                else
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsBonk));
                }
            }
        }

        public override void OnPickup()
        {
            if (!UseVoice) return;
            if (Time.timeSinceLevelLoad < nextVocalTime) return;

            switch (State)
            {
                case VRoombaState.Idle:
                    break;
                case VRoombaState.Active:
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsGrabDecline));
                    break;
                case VRoombaState.Righting:
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsGrabAccept));
                    break;
            }
        }
        
        public override void OnPickupUseDown()
        {
            State = (State == VRoombaState.Idle) ? VRoombaState.Active : VRoombaState.Idle;
            OnDeserialization();
        }
        
        private void HandleMovement()
        {
            upright = Vector3.Dot(transform.up, Vector3.up) > UprightThreshold;

            if (Time.timeSinceLevelLoad >= nextTurnCheckTime)
            {
                nextTurnCheckTime = Time.timeSinceLevelLoad + DecisionDelay;

                bool leftSensor = SensorLeft.Active;
                bool rightSensor = SensorRight.Active;

                if (leftSensor || rightSensor)
                {
                    if (TurnState == VRoombaTurnState.None)
                    {
                        if (leftSensor && rightSensor)
                        {
                            TurnState = SensorLeft.CollisionTime < SensorRight.CollisionTime
                                ? VRoombaTurnState.Left
                                : VRoombaTurnState.Right;
                        }
                        else if (leftSensor)
                        {
                            TurnState = VRoombaTurnState.Left;
                        }
                        else if (rightSensor)
                        {
                            TurnState = VRoombaTurnState.Right;
                        }
                    }
                }
                else
                {
                    TurnState = VRoombaTurnState.None;
                }
            }
            
            MoveForward();
            switch (TurnState)
            {
                case VRoombaTurnState.None:
                    break;
                case VRoombaTurnState.Left:
                    TurnLeft();
                    break;
                case VRoombaTurnState.Right:
                    TurnRight();
                    break;
            }
        }
        
        private void TurnLeft()
        {
            Quaternion rotMovement = Quaternion.Euler(transform.up * TurnSpeed);
            rb.MoveRotation(rb.rotation * rotMovement);
        }
        private void TurnRight()
        {
            Quaternion rotMovement = Quaternion.Euler(-transform.up * TurnSpeed);
            rb.MoveRotation(rb.rotation * rotMovement);
        }
        private void MoveForward()
        {
            rb.MovePosition(rb.position + transform.forward * (Time.deltaTime * MoveSpeed));
        }
        public void EndSpeed()
        {
            speedIncreaseCount -= 1;
            if (speedIncreaseCount <= 0) MoveSpeed = baseMoveSpeed;
        }
        
        [UsedImplicitly]
        public void DoneRighting() // Called via Animations...
        {
            TurnState = VRoombaTurnState.None;
            if (!isOwner) return;
            
            State = VRoombaState.Active;
            OnDeserialization();
        }
        
        public void PlayClipsCollide() => PlayClip(VoiceCollide);
        public void PlayClipsEat() => PlayClip(VoiceEat);
        public void PlayClipsBonk() => PlayClip(ClipsBonk);
        public void PlayClipsFallen() => PlayClip(VoiceFallen);
        public void PlayClipsGrabAccept() => PlayClip(VoiceGrabAccept);
        public void PlayClipsGrabDecline() => PlayClip(VoiceGrabDecline);
        private void PlayClip(AudioClip[] voiceClips)
        {
            if (Time.timeSinceLevelLoad < nextVocalTime) return;
            AudioClip clip = voiceClips[UnityEngine.Random.Range(0, voiceClips.Length)];
            if (!clip) return;
            
            SourceEffects.PlayOneShot(clip);
            nextVocalTime = Time.timeSinceLevelLoad + clip.length * 2;
        }
    }
}