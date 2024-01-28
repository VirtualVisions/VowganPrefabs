
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ResetChessGame : UdonSharpBehaviour
    {
        public VRCObjectSync[] Pieces;


        public override void Interact()
        {
            for (int i = 0; i < Pieces.Length; i++)
            {
                Networking.SetOwner(Networking.LocalPlayer, Pieces[i].gameObject);
                Pieces[i].Respawn();
            }
        }
    }
}