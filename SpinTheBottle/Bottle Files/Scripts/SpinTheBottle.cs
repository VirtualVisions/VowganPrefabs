using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace VowganVR
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SpinTheBottle : UdonSharpBehaviour
    {

        [UdonSynced] public int BottleRotation;
        public float TimeBetweenClicks = 1;

        private VRCPlayerApi playerLocal;
        private Animator anim;
        private int hashSpin;
        private float lastSpinTime;


        private void Start()
        {
            playerLocal = Networking.LocalPlayer;
            anim = GetComponent<Animator>();
            hashSpin = Animator.StringToHash("Spin");
        }

        public override void OnDeserialization()
        {
            transform.rotation = Quaternion.Euler(0, BottleRotation, 0);
            anim.SetTrigger(hashSpin);
            lastSpinTime = Time.timeSinceLevelLoad;
        }

        public override void Interact()
        {
            if (Time.timeSinceLevelLoad - lastSpinTime < TimeBetweenClicks) return;
            
            Networking.SetOwner(playerLocal, gameObject);

            BottleRotation = Random.Range(0, 359);

            RequestSerialization();
            OnDeserialization();
        }
    }
}