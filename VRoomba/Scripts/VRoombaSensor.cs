
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    public class VRoombaSensor : UdonSharpBehaviour
    {
        
        public VRoomba vRoomba;
        public bool leftSensor;
        
        [SerializeField] private int collisionCount;
        
        
        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) return;
            if (other.gameObject.name.Contains("Sensor")) return;
            if (other.isTrigger) return;
            collisionCount += 1;
            TurningCheck();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!Utilities.IsValid(other)) return;
            if (other.gameObject.name.Contains("Sensor")) return;
            if (other.isTrigger) return;
            collisionCount -= 1;
            TurningCheck();
        }

        private void TurningCheck()
        {
            if (collisionCount <= 0)
            {
                collisionCount = 0;
                if (leftSensor)
                {
                    vRoomba.isTurningLeft = false;
                }
                else
                {
                    vRoomba.isTurningRight = false;
                }
            } 
            else if (collisionCount > 0)
            {
                if (leftSensor)
                {
                    vRoomba.isTurningLeft = true;
                }
                else
                {
                    vRoomba.isTurningRight = true;
                }
            }
        }
    }
}