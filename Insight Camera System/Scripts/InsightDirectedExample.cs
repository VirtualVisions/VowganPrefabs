
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InsightDirectedExample : UdonSharpBehaviour
    {
        
        public bool BlendCamera;
        public bool LockPlayer = true;
        public float Duration = 3;
        public InsightController Controller;
        public Transform Target;
        public Transform LookAt;
        
        private bool directing;
        
        
        public override void Interact()
        {
            if (directing) return;
            directing = true;
            Controller._StartDirection(Target, LookAt, BlendCamera, LockPlayer);
            SendCustomEventDelayedSeconds(nameof(_EndDirection), Duration);
        }
        
        public void _EndDirection()
        {
            directing = false;
            Controller._EndDirection(LockPlayer);
        }
        
        
    }
}