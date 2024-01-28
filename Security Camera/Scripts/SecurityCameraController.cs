
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SecurityCameraController : UdonSharpBehaviour
    {
        
        public Transform RenderCamera;
        public GameObject CameraScreen;
        public Slider FOVSlider;
        public TextMeshProUGUI CameraNameText;
        public SecurityCamera[] Cameras;
        [UdonSynced, FieldChangeCallback(nameof(CurrentCameraCallback))] public int CurrentCamera = -1;
        public int CurrentCameraCallback
        {
            get => CurrentCamera;
            set
            {
                CurrentCamera = value;
                LoadCamera();
            }
        }
        
        private VRCPlayerApi playerLocal;
        private Camera renderCam;
        
        
        private void Start()
        {
            playerLocal = Networking.LocalPlayer;
            renderCam = RenderCamera.GetComponent<Camera>();
            LoadCamera();
        }
        
        public void _NextCamera()
        {
            Networking.SetOwner(playerLocal, gameObject);
            CurrentCameraCallback++;
            RequestSerialization();
        }
        
        public void _PreviousCamera()
        {
            Networking.SetOwner(playerLocal, gameObject);
            CurrentCameraCallback--;
            RequestSerialization();
        }

        public void _SetFOV()
        {
            renderCam.fieldOfView = FOVSlider.value;
        }
        
        private void LoadCamera()
        {
            if (CurrentCamera == -1)
            {
                CameraScreen.SetActive(false);
                renderCam.enabled = false;
            }
            else if (CurrentCamera < -1)
            {
                CurrentCamera = Cameras.Length - 1;
                CameraScreen.SetActive(true);
                renderCam.enabled = true;
                CameraNameText.text = Cameras[CurrentCamera].transform.name;
            }
            else if (CurrentCamera >= Cameras.Length)
            {
                CurrentCamera = -1;
                CameraScreen.SetActive(false);
                renderCam.enabled = false;
            }
            else
            {
                CameraScreen.SetActive(true);
                renderCam.enabled = true;
                CameraNameText.text = Cameras[CurrentCamera].transform.name;
            }
            
            for (int i = 0; i < Cameras.Length; i++)
            {
                if (CurrentCamera == i)
                {
                    RenderCamera.parent = Cameras[i].RenderPoint;
                    RenderCamera.localPosition = Vector3.zero;
                    RenderCamera.localRotation = Quaternion.identity;
                }
            }
        }
        
    }
}