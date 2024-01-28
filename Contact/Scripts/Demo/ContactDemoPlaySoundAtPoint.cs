
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace Vowgan.Contact
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactDemoPlaySoundAtPoint : ContactBehaviour
    {
        
        public AudioClip Clip;
        public Transform Point;
        [Range(0, 1)] public float Volume = 1;
        public bool ParentClip;
        public bool CutOffPrevious;
        public float MaxDistance = 32;
        
        private ContactInstance instance;
        
        
        public override void Interact()
        {
            Transform playPoint = Point;
            if (playPoint == null) playPoint = transform;

            if (CutOffPrevious)
            {
                if (instance != null)
                {
                    if (instance)
                    {
                        instance._ReturnToPool(); 
                        instance = null;
                    }
                }
            }
            
            if (ParentClip)
            {
                instance = ContactAudio._PlaySoundChilded(Clip, playPoint, playPoint.position, MaxDistance, Volume);
            }
            else
            {
                instance = ContactAudio._PlaySound(Clip, playPoint.position, MaxDistance, Volume);
            }
        }
    }
    
}