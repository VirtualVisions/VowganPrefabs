
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Contact.Gunshots
{
    public class ContactGunshots : ContactBehaviour
    {
        
        [ContactSurfacePreset] public UnityEngine.Object[] Presets;
        
        public Material[][] Materials;
        public int[][] MaterialIds;
        public AudioClip[][] Clips;
        
        public Transform FirePoint;
        public LayerMask HitLayers;

        private int currentId;


        protected override void _Init()
        {
            base._Init();
            
            MaterialIds = new int[Materials.Length][];
            for (int x = 0; x < MaterialIds.Length; x++)
            {
                MaterialIds[x] = new int[Materials[x].Length];
                for (int y = 0; y < Materials[x].Length; y++)
                {
                    MaterialIds[x][y] = Materials[x][y].GetInstanceID();
                }
            }
        }
        
        public override void OnPickupUseDown()
        {
            if (UnityEngine.Physics.Raycast(FirePoint.position, FirePoint.forward, out RaycastHit hit, 100, HitLayers))
            {
                MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();
                if (!meshRenderer) return;
                
                int instanceId = meshRenderer.sharedMaterial.GetInstanceID();
                
                CheckMaterialId(instanceId);
                ContactAudio._PlaySound(Clips[currentId], hit.point);
            }
        }
        
        private void CheckMaterialId(int id)
        {
            for (int i = 0; i < MaterialIds.Length; i++)
            {
                if (Array.IndexOf(MaterialIds[i], id) != -1)
                {
                    currentId = i;
                }
            }
        }
    }
}