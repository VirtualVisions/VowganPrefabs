
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ModularFood
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModularFoodList : UdonSharpBehaviour
    {

        public GameObject[] FoodParts;
        public float[] PartHeights;

    }
}