
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SnapOnDrop : UdonSharpBehaviour
    {
        
        public Vector3 SnapPositionScale = new Vector3(0.1f, 1000, 0.1f);
        public float SnapRotationScale = 45f;
        public bool LocalSpace = true;
        public bool HoldRotationVector = true;
        public Vector3 RotationVector = Vector3.up;


        public override void OnDrop()
        {
            Vector3 droppedPosition;
            Vector3 droppedRotation;
            if (LocalSpace)
            {
                droppedPosition = transform.localPosition;
                droppedRotation = transform.localRotation.eulerAngles;
            }
            else
            {
                droppedPosition = transform.position;
                droppedRotation = transform.rotation.eulerAngles;
            }
            
            droppedPosition.x = Mathf.Round(droppedPosition.x / SnapPositionScale.x) * SnapPositionScale.x;
            droppedPosition.y = Mathf.Round(droppedPosition.y / SnapPositionScale.y) * SnapPositionScale.y;
            droppedPosition.z = Mathf.Round(droppedPosition.z / SnapPositionScale.z) * SnapPositionScale.z;
            
            Vector3 heldRotation = Vector3.one;
            
            if (HoldRotationVector)
            {
                heldRotation = RotationVector;
            }
            
            droppedRotation.x = Mathf.Round(droppedRotation.x / SnapRotationScale) * heldRotation.x * SnapRotationScale;
            droppedRotation.y = Mathf.Round(droppedRotation.y / SnapRotationScale) * heldRotation.y * SnapRotationScale;
            droppedRotation.z = Mathf.Round(droppedRotation.z / SnapRotationScale) * heldRotation.z * SnapRotationScale;
            
            if (LocalSpace)
            {
                transform.localPosition = droppedPosition;
                transform.localRotation = Quaternion.Euler(droppedRotation);
            }
            else
            {
                transform.position = droppedPosition;
                transform.rotation = Quaternion.Euler(droppedRotation);
            }
            
        }
    }
}
