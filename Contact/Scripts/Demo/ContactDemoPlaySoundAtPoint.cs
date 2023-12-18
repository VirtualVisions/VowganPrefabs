
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
        public float MaxDistance = ContactAudioPlayer.DEFAULT_MAX_DISTANCE;
        
        private DataDictionary audioClip;
        
        
        public override void Interact()
        {
            Transform playPoint = Point;
            if (playPoint == null) playPoint = transform;

            if (CutOffPrevious)
            {
                if (audioClip != null)
                {
                    if (audioClip[ContactAudioPlayer.IS_PLAYING].Boolean)
                    {
                        ContactAudio._ReturnSource(audioClip);
                        audioClip = null;
                    }
                }
            }
            
            if (ParentClip)
            {
                audioClip = ContactAudio._PlaySoundChilded(Clip, playPoint, playPoint.position, MaxDistance, Volume);
            }
            else
            {
                audioClip = ContactAudio._PlaySound(Clip, playPoint.position, MaxDistance, Volume);
            }
        }
    }
    
}