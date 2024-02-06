
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InsightRangeTrigger : UdonSharpBehaviour
    {

        [Range(0, 10)] public float Range = 1.5f;
        [Tooltip("Speed at which it moves between camera distances.")]
        public float FadeSpeed = 2;
        public InsightController Controller;
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || !player.isLocal) return;
            Controller.Range = Range;
            Controller.RangeFadeSpeed = FadeSpeed;
        }
    }
}