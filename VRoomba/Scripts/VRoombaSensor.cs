
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRoombaSensor : UdonSharpBehaviour
    {
        
        public bool Active => collisionCount > 0;
        public float CollisionTime;
        
        private int collisionCount;
        
        
        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) return;
            if (other.isTrigger) return;
            collisionCount += 1;
            CollisionTime = Time.timeSinceLevelLoad;
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!Utilities.IsValid(other)) return;
            if (other.isTrigger) return;
            collisionCount -= 1;
        }
    }
}