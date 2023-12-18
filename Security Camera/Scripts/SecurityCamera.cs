
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SecurityCamera : UdonSharpBehaviour
    {
        
        public Vector2 RotationRange = new Vector2(-45, 45);
        public byte State = 1;

        public float RotationSpeed = 10;
        public float TraversePoint;
        public float WaitTime = 3;
        public float WaitTimeClock;
        
        public Transform Axis;
        public Transform Body;
        public Transform RenderPoint;
        
        private const byte STATE_IDLE = 0;
        private const byte STATE_FORWARD = 1;
        private const byte STATE_FORWARD_WAIT = 2;
        private const byte STATE_REVERSE = 3;
        private const byte STATE_REVERSE_WAIT = 4;
        
        
        private void Update()
        {
            switch (State)
            {
                case STATE_IDLE:
                    break;
                case STATE_FORWARD:
                    TraversePoint += RotationSpeed * Time.deltaTime;
                    if (TraversePoint >= RotationRange.y)
                    {
                        State = STATE_FORWARD_WAIT;
                    }
                    break;
                case STATE_REVERSE:
                    TraversePoint -= RotationSpeed * Time.deltaTime;
                    if (TraversePoint <= RotationRange.x)
                    {
                        State = STATE_REVERSE_WAIT;
                    }
                    break;
                case STATE_FORWARD_WAIT:
                    WaitTimeClock += Time.deltaTime;
                    if (WaitTimeClock >= WaitTime)
                    {
                        State = STATE_REVERSE;
                        WaitTimeClock = 0;
                    }
                    break;
                case STATE_REVERSE_WAIT:
                    WaitTimeClock += Time.deltaTime;
                    if (WaitTimeClock >= WaitTime)
                    {
                        State = STATE_FORWARD;
                        WaitTimeClock = 0;
                    }
                    break;
            }
            
            Vector3 pivotOldRotation = Axis.localRotation.eulerAngles;
            
            Axis.localRotation = Quaternion.Euler(
                pivotOldRotation.x,
                pivotOldRotation.y,
                Mathf.SmoothStep(RotationRange.x, RotationRange.y, 
                    Mathf.InverseLerp(RotationRange.x, RotationRange.y, TraversePoint)));
        }
    }
}

