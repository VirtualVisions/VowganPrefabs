using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
namespace Vowgan
{
    public class InsightCollisionLayers : MonoBehaviour
    {

        [Tooltip("The Layers that the follow cameras will treat as walls.")]
        public LayerMask CollisionLayers;
        public CinemachineCollider[] Colliders;

    }
}
#endif