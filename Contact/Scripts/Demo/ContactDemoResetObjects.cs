
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Contact
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactDemoResetObjects : ContactBehaviour
    {
        public Rigidbody[] Props;
        
        private DataList propData;

        private const string PropRigidbody = "PropRigidbody";
        private const string PropPosition = "PropPosition";
        private const string PropRotation = "PropRotation";
        
        protected override void _Init()
        {
            base._Init();
            
            propData = new DataList();
            
            foreach (Rigidbody prop in Props)
            {
                DataDictionary dict = new DataDictionary();
                dict.Add(PropRigidbody, prop);
                dict.Add(PropPosition, new DataToken(prop.position));
                dict.Add(PropRotation, new DataToken(prop.rotation));
                propData.Add(dict);
            }
        }

        public override void Interact()
        {
            for (int i = 0; i < propData.Count; i++)
            {
                Rigidbody rb = (Rigidbody)propData[i].DataDictionary[PropRigidbody].Reference;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = (Vector3)propData[i].DataDictionary[PropPosition].Reference;
                rb.rotation = (Quaternion)propData[i].DataDictionary[PropRotation].Reference;
            }
        }
    }
}