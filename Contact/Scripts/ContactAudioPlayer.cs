
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Contact
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactAudioPlayer : UdonSharpBehaviour
    {

        public GameObject AudioPrefab;
        public int SourceCount = 64;

        [HideInInspector] public AudioSource[] AudioSourceList;
        [HideInInspector] public GameObject[] GameObjectList;
        [HideInInspector] public Transform[] TransformList;
        [HideInInspector] public DataList AudioList = new DataList();

        private VRCPlayerApi localPlayer;
        private int lastPlayedIndex = -1;
        
        public const string INDEX = "Index";
        public const string IS_PLAYING = "IsPlaying";
        public const string IS_CHILDED = "IsChilded";
        public const string START_TIME = "StartTime";
        public const string CLIP_LENGTH = "ClipLength";

        public const float DEFAULT_MAX_DISTANCE = 32;

        private bool initialized;


        private void Start()
        {
            if (!initialized) _Init();
        }

        private void _Init()
        {
            initialized = true;
            
            localPlayer = Networking.LocalPlayer;
            
            for (int i = 0; i < AudioSourceList.Length; i++)
            {
                DataDictionary data = new DataDictionary();

                data[INDEX] = i;
                data[IS_PLAYING] = false;
                data[IS_CHILDED] = false;
                data[START_TIME] = 0;
                data[CLIP_LENGTH] = 0;
                
                GameObjectList[i].SetActive(false);
                AudioList.Add(data);
            }
        }

        private void Update()
        {
            for (int i = 0; i < AudioList.Count; i++)
            {
                DataDictionary instance = AudioList[i].DataDictionary;
                if (instance[IS_PLAYING].Boolean == false) continue;

                float clipTime = Time.timeSinceLevelLoad - instance[START_TIME].Float;
                
                if (clipTime >= instance[CLIP_LENGTH].Float)
                {
                    _ReturnSource(instance);
                }
            }
        }

        public DataDictionary _PlaySoundChilded(AudioClip clip, Transform parent, Vector3 position, float maxDistance = DEFAULT_MAX_DISTANCE, float volume = 1, float pitch = 1)
        {
            DataDictionary instance = _PlaySound(clip, position, maxDistance, volume, pitch);
            if (instance != null)
            {
                int index = instance[INDEX].Int;
                instance[IS_CHILDED] = true;
                TransformList[index].parent = parent;
            }
            return instance;
        }
        
        public DataDictionary _PlaySound(AudioClip[] clips, Vector3 position, float maxDistance = DEFAULT_MAX_DISTANCE, float volume = 1, float pitch = 1)
        {
            if (clips == null || clips.Length == 0)
            {
                _Log("Clip array length is 0, unable to play a new clip.");
                return null;
            }
            
            DataDictionary instance = _PlaySound(clips[UnityEngine.Random.Range(0, clips.Length)], position, maxDistance, volume, pitch);
            return instance;
        }

        public void _ReturnSource(DataDictionary instance)
        {
            instance[IS_PLAYING] = false;
            instance[START_TIME] = 0;
            instance[CLIP_LENGTH] = 0;

            int index = instance[INDEX].Int;

            if (instance[IS_CHILDED].Boolean)
            {
                instance[IS_CHILDED] = false;
                TransformList[index].SetParent(transform);
                TransformList[index].SetSiblingIndex(index);
            }

            GameObjectList[index].SetActive(false);
        }
        
        public DataDictionary _PlaySound(AudioClip clip, Vector3 position, float maxDistance = DEFAULT_MAX_DISTANCE, float volume = 1, float pitch = 1)
        {
            if (!initialized) _Init();
            
            DataDictionary instance = TryGetSource();
            
            if (clip == null)
            {
                _Log("Tried to play null clip.");
                return instance;
            }
            
            if (instance != null)
            {
                if (Vector3.Distance(localPlayer.GetPosition(), position) > maxDistance)
                {
                    _Log("Target position is out of range.");
                    return instance;
                }
                
                instance[IS_PLAYING] = true;
                instance[START_TIME] = Time.timeSinceLevelLoad;
                
                if (clip == null)
                {
                    _Log("Tried to play null clip.");
                }
                else
                {
                    instance[CLIP_LENGTH] = clip.length;
                }


                int index = instance[INDEX].Int;

                AudioSource source = AudioSourceList[index];
                source.clip = clip;
                source.maxDistance = maxDistance;
                source.volume = volume;
                source.pitch = pitch;
                
                GameObjectList[index].SetActive(true);
                TransformList[index].position = position;
            }

            return instance;
        }

        private DataDictionary TryGetSource()
        {
            for (int i = 0; i < SourceCount; i++)
            {
                lastPlayedIndex++;
                if (lastPlayedIndex >= SourceCount) lastPlayedIndex = 0;
                
                DataDictionary instance = AudioList[lastPlayedIndex].DataDictionary;
                if (instance[IS_PLAYING].Boolean) continue;

                return instance;
            }

            _Log("No available Audio Sources found.");
            return null;
        }
        
        public void _Log(string log)
        {
            Debug.Log($"<color=yellow>{this.name}:</color> {log}", this);
        }
    }
}
