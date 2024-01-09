
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Vowgan
{
    public class VRoomba : UdonSharpBehaviour
    {
        
        public float moveSpeed = 0.5f;
        public float turnSpeed = 1f;
        public float decisionDelay = 0.2f;
        public float painThreshold = 1.0f;
        [UdonSynced] public bool isMoving;
        public bool isTurningLeft;
        public bool isTurningRight;
        public AudioClip clipBonk;
        public AudioSource sourceEffects;
        
        [Header("Vocals")] 
        public bool useVocals = true;
        public AudioClip[] clipsCollide;
        public AudioClip[] clipsEat;
        public AudioClip[] clipsFallen;
        public AudioClip[] clipsGrabAccept;
        public AudioClip[] clipsGrabDecline;
        
        private Rigidbody rb;
        private Animator anim;
        private VRCPickup pickup;
        private float decisionTarget;
        private float baseMoveSpeed;
        private float nextVocalBus;
        private float nextImpactBus;
        private bool queueLeft;
        private bool queueRight;
        private bool upright;
        private bool isRighting;
        private int speedIncreaseCount;
        
        private int hashRight;
        private int hashActive;


        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            pickup = GetComponent<VRCPickup>();

            baseMoveSpeed = moveSpeed;
            
            hashRight = Animator.StringToHash("Right");
            hashActive = Animator.StringToHash("Active");
            
            anim.SetBool(hashActive, isMoving);
        }
        
        private void Update()
        {
            if (!isMoving || isRighting || pickup.IsHeld) return;
            
            upright = Vector3.Dot(transform.up, Vector3.up) > 0.4f;

            if (upright)
            {
                HandleMovement();
            }
            else
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkUpright));
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (!Utilities.IsValid(other.gameObject) || !isMoving || isRighting) return;
            
            if (!other.transform.GetComponent<VRoomba>())
            {
                VRCObjectSync objectSync = (VRCObjectSync) other.transform.GetComponent(typeof(VRCObjectSync));
                if (Utilities.IsValid(objectSync))
                {
                    objectSync.Respawn();
                    if (useVocals) SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsEat));
                    moveSpeed = baseMoveSpeed * 2;
                    speedIncreaseCount += 1;
                    SendCustomEventDelayedSeconds(nameof(EndSpeed), 5);
                }
            }
            
            if (Time.timeSinceLevelLoad < nextImpactBus) return;
            nextImpactBus = Time.timeSinceLevelLoad + 2;
            
            float impact = other.relativeVelocity.magnitude;
            if (impact > 0.01f)
            {
                sourceEffects.PlayOneShot(clipBonk);
                if (isMoving && impact > painThreshold && useVocals)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsCollide));
                }
            }
        }

        public override void OnPickup()
        {
            if (!isMoving || !useVocals) return;
            if (Time.timeSinceLevelLoad < nextVocalBus) return;
            
            if (upright)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsGrabDecline));
            }
            else
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayClipsGrabAccept));
            }
        }
        
        public override void OnPickupUseDown()
        {
            isMoving = !isMoving;
            anim.SetBool(hashActive, isMoving);
        }
        
        private void HandleMovement()
        {
            if (!(queueLeft && queueRight)) MoveForward();
            
            if (Time.time >= decisionTarget)
            {
                decisionTarget = Time.time + decisionDelay;
                if (isTurningLeft)
                {
                    queueLeft = true;
                    queueRight = false;
                } 
                else if (isTurningRight)
                {
                    queueLeft = false;
                    queueRight = true;
                }
                else
                {
                    queueLeft = false;
                    queueRight = false;
                }
            }
            
            if (queueLeft)
            {
                TurnLeft();
            }
            else if (queueRight)
            {
                TurnRight();
            }
        }
        
        public void NetworkUpright()
        {
            isRighting = true;
            anim.SetTrigger(hashRight);
            if (useVocals) PlayClipsFallen();
        }
        
        private void TurnLeft()
        {
            Quaternion rotMovement = Quaternion.Euler(transform.up * turnSpeed);
            rb.MoveRotation(rb.rotation * rotMovement);
        }
        private void TurnRight()
        {
            Quaternion rotMovement = Quaternion.Euler(-transform.up * turnSpeed);
            rb.MoveRotation(rb.rotation * rotMovement);
        }
        private void MoveForward()
        {
            rb.MovePosition(rb.position + transform.forward * (Time.deltaTime * moveSpeed));
        }
        public void EndSpeed()
        {
            speedIncreaseCount -= 1;
            if (speedIncreaseCount <= 0) moveSpeed = baseMoveSpeed;
        }
        public void DoneRighting() // Called via Animations...
        {
            isRighting = false;
            if (isTurningLeft && isTurningRight) isTurningLeft = false;
        }
        
        public void PlayClipsCollide() { PlayVoice(clipsCollide); }
        public void PlayClipsEat() { PlayVoice(clipsEat); }
        public void PlayClipsFallen() { PlayVoice(clipsFallen); }
        public void PlayClipsGrabAccept() { PlayVoice(clipsGrabAccept); }
        public void PlayClipsGrabDecline() { PlayVoice(clipsGrabDecline); }
        private void PlayVoice(AudioClip[] clips)
        {
            if (Time.timeSinceLevelLoad < nextVocalBus) return;
            AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            if (clip)
            {
                sourceEffects.PlayOneShot(clip);
                nextVocalBus = Time.timeSinceLevelLoad + clip.length * 2;
            }
        }
    }
}