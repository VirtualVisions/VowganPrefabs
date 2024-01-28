
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VowganVR
{
    public class ObjSequenceC : MonoBehaviour
    {
    
        [Tooltip("Determine whether or not the Obj Sequence should be running.")]
        public bool Animating;
        [Tooltip("Whether or not the sequence should continue to animate past the last value.")]
        public bool Loop = true;
        [Tooltip("Whether to play the sequence in reverse.")]
        public bool Reversing;
        [Tooltip("Determine what framerate the sequence will be animating at.")]
        public int Framerate = 60;
        [Tooltip("Whether to swap materials for each object.")]
        public bool UseCustomMaterials;
        [Tooltip("All objects (in order) which will have their meshes cannibalized for the sequence.")]
        public GameObject[] SequenceObjects;
        [Tooltip("Materials used when swapping between sequences.")]
        public Material[] Materials;
        
        private Mesh[] meshes;
        private MeshFilter filter;
        private MeshRenderer rend;
        private int index;
        private float timer;
    
        private void Start()
        {
            filter = GetComponent<MeshFilter>();
            rend = GetComponent<MeshRenderer>();
            
            meshes = new Mesh[SequenceObjects.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = SequenceObjects[i].GetComponentInChildren<MeshFilter>().sharedMesh;
            }
            ApplyIndex();
        }
        
        
        private void Update()
        {
            if (!Animating) return;
            
            timer += Time.deltaTime;
            
            if (timer > 1f / Framerate)
            {
                // Check how many frames should have played since the last check
                // This functions to catch up people with lower framerate
                int iteration = (int)Mathf.Floor(timer / (1f / Framerate));
                
                // Remove new frame count from timer and keep the float overflow for accuracy
                timer -= iteration * 1f / Framerate;

                if (!Reversing)
                {
                    if (index + iteration < meshes.Length)
                    {
                        index += iteration;
                        ApplyIndex();
                    }
                    else
                    {
                        if (Loop)
                        {
                            index = 0;
                            filter.sharedMesh = meshes[index];
                        }
                        else
                        {
                            Animating = false;
                            filter.sharedMesh = meshes[index];
                            index = 0;
                        }
                    }
                }
                else
                {
                    if (index - iteration > -1)
                    {
                        index -= iteration;
                        ApplyIndex();
                    }
                    else
                    {
                        if (Loop)
                        {
                            index = meshes.Length - 1;
                            ApplyIndex();
                        }
                        else
                        {
                            Animating = false;
                            ApplyIndex();
                            index = meshes.Length - 1;
                        }
                    }
                }
                
                // Check if the animation needs to be reset
            }
        }
        
        private void ApplyIndex()
        {
            filter.sharedMesh = meshes[index];
            if (UseCustomMaterials)
            {
                rend.sharedMaterial = Materials[index];
            }
        }
        
        #region Public Events
    
        /// <summary>
        /// Begin playback of the Sequence. If paused, resume from current position.
        /// </summary>
        public void Play()
        {
            Animating = true;
        }
        
        /// <summary>
        /// Pause playback of the sequence.
        /// </summary>
        public void Pause()
        {
            Animating = false;
        }
        
        /// <summary>
        /// Halt playback of the sequence and reset the Sequence.
        /// </summary>
        public void Stop()
        {
            Pause();
            ResetState();
        }
        
        /// <summary>
        /// Toggle whether or not to reverse the playback direction of the Sequence.
        /// </summary>
        public void Reverse()
        {
            Reversing = !Reversing;
            if (Animating) return;
            if (Reversing)
            {
                index = meshes.Length - 1;
            }
            else
            {
                index = 0;
            }
        }
        
        /// <summary>
        /// Reset Sequence to the original frame.
        /// </summary>
        public void ResetState()
        {
            index = 0;
            filter.sharedMesh = meshes[0];
        }    
    
        #endregion
    }
}