
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ModularFood
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(CapsuleCollider))]
    public class ModularFoodPlate : UdonSharpBehaviour
    {

        public ModularFoodList FoodList;
        [UdonSynced] public int[] PartIndices;
        [SerializeField] private int[] PartIndicesLocal;
        [SerializeField] private int PartCount;
        [SerializeField] private GameObject[] PartObjects;
        [SerializeField] private float PlacementRange = 0.005f;
        [SerializeField] private float PlaceHeight;

        [SerializeField] private ModularFoodPlateProxy Proxy;
        [SerializeField] private AudioSource Source;

        private CapsuleCollider capsule;


        private void Start()
        {
            capsule = GetComponent<CapsuleCollider>();
        }

        public override void OnDeserialization()
        {
            if (PartIndicesLocal.Length != PartIndices.Length)
            {
                Source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                Source.volume = UnityEngine.Random.Range(0.8f, 1f);
                Source.PlayOneShot(Source.clip);
                PartCount = PartIndicesLocal.Length;
                GameObject[] parts = new GameObject[PartIndices.Length];
                Array.Copy(PartObjects, parts, Mathf.Min(parts.Length, PartObjects.Length));


                if (PartIndicesLocal.Length < PartIndices.Length)
                {
                    while (PartCount < PartIndices.Length)
                    {
                        GameObject part = FoodList.FoodParts[PartIndices[PartCount]];
                        parts[PartCount] = Instantiate(part, transform);
                        parts[PartCount].transform.localPosition = new Vector3(
                            UnityEngine.Random.Range(-PlacementRange, PlacementRange),
                            PlaceHeight,
                            UnityEngine.Random.Range(-PlacementRange, PlacementRange));
                        parts[PartCount].transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                        PlaceHeight += FoodList.PartHeights[PartIndices[PartCount]];

                        PartCount++;
                    }
                }
                else if (PartIndices.Length == 0)
                {
                    foreach (Transform obj in transform) Destroy(obj.gameObject);
                    PartObjects = new GameObject[0];
                    PlaceHeight = 0;
                    PartCount = 0;
                }

                PartObjects = parts;
                PartIndicesLocal = PartIndices;
            }

            capsule.height = PlaceHeight;
            capsule.center = Vector3.up * (PlaceHeight / 2);

        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) return;

            ModularFoodPart part = other.GetComponent<ModularFoodPart>();
            if (part)
            {
                if (!Networking.IsOwner(part.gameObject)) return;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                int[] parts = new int[PartCount + 1];
                Array.Copy(PartIndices, parts, Mathf.Min(parts.Length, PartIndices.Length));
                parts[PartCount] = part.PartIndex;
                PartIndices = parts;
                RequestSerialization();
                OnDeserialization();
                part._Respawn();
            }

            ModularFoodTrash trash = other.GetComponent<ModularFoodTrash>();
            if (trash)
            {
                if (!Networking.IsOwner(gameObject)) return;
                PartIndices = new int[0];
                RequestSerialization();
                OnDeserialization();
                Proxy._Respawn();
            }
        }

        public void _OnPickupUseDown()
        {

        }
    }
}