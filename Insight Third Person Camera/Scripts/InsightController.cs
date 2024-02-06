
using System;
using Cinemachine;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InsightController : UdonSharpBehaviour
    {
        
        [Header("Controls")]
        [Tooltip("Input used for toggling the entire camera system.")]
        [SerializeField] private KeyCode InputActiveToggle = KeyCode.T;
        [Tooltip("Input used for moving the camera behind the player.")]
        [SerializeField] private KeyCode InputPrimaryView = KeyCode.Alpha1;
        [Tooltip("Input used for moving the camera over the left shoulder.")]
        [SerializeField] private KeyCode InputLeftShoulderView = KeyCode.Alpha2;
        [Tooltip("Input used for moving the camera over the right shoulder.")]
        [SerializeField] private KeyCode InputRightShoulderView = KeyCode.Alpha3;
        [Tooltip("Input used for moving the camera to face backwards towards the player.")]
        [SerializeField] private KeyCode InputPrimaryRear = KeyCode.Alpha4;
        [Tooltip("Input used for toggling the camera between the front or back view.")]
        [SerializeField] private KeyCode InputForwardViewToggle = KeyCode.F;
        [Tooltip("Input used for toggling the camera between the right or left right shoulder view.")]
        [SerializeField] private KeyCode InputShoulderViewToggle = KeyCode.Q;
        [Tooltip("Input used for toggling the visibility of the controls.")]
        [SerializeField] private KeyCode InputControlsToggle = KeyCode.Tab;
        [Tooltip("Input used for toggling camera tracking.")]
        [SerializeField] private KeyCode InputRoamingToggle = KeyCode.G;
        [Tooltip("Input used for toggling camera smoothing.")]
        [SerializeField] private KeyCode InputBlending = KeyCode.B;
        
        [Header("Field of View")]
        [Tooltip("Input used for toggling the Fov zoom.")]
        [SerializeField] private KeyCode InputFovToggle = KeyCode.Mouse2;
        [SerializeField] private float FovZoomIn = 30;
        [SerializeField] private float FovZoomOut = 60;
        
        [Header("Audio")] 
        [SerializeField] private AudioSource Source;
        [SerializeField] private AudioClip ClipZoomIn;
        [SerializeField] private AudioClip ClipZoomOut;
        [SerializeField] private AudioClip ClipClick;
        
        [Header("References")]
        [SerializeField] private Transform HeadTracker;
        [SerializeField] private CinemachineVirtualCamera PrimaryCamera;
        [SerializeField] private CinemachineVirtualCamera LeftShoulderCamera;
        [SerializeField] private CinemachineVirtualCamera RightShoulderCamera;
        [SerializeField] private CinemachineVirtualCamera RearCamera;
        [SerializeField] private CinemachineVirtualCamera DirectedCamera;
        [SerializeField] private CinemachineVirtualCamera RoamingCamera;
        [SerializeField] private GameObject Controls;
        [SerializeField] private Camera Cam;
        [SerializeField] private Camera CamUi;
        [SerializeField] private TextMeshProUGUI ControlHint;
        [SerializeField] private TextMeshProUGUI TextDebug;
        
        [HideInInspector] public float Range = 1.5f;
        [HideInInspector] public float RangeFadeSpeed = 2;
        
        private VRCPlayerApi playerLocal;
        private Animator anim;
        private Transform camTransform;
        private Transform roamingTransform;
        private byte cameraType;
        private float fov;
        private float walkSpeed;
        private float runSpeed;
        private float strafeSpeed;
        private float jumpImpulse;
        private bool roaming;
        private bool debugVisible;
        private bool blending = true;
        private bool directed;
        private bool playerLocked;
        private bool active;
        
        private const float FOV_ZOOM_SPEED = 8;
        
        private const byte CAMERA_PRIMARY = 0;
        private const byte CAMERA_LEFT_SHOULDER = 1;
        private const byte CAMERA_RIGHT_SHOULDER = 2;
        private const byte CAMERA_REAR_VIEW = 3;
        private const byte CAMERA_ROAMING = 4;
        
        
        private int hashFov;
        private int hashBlending;
        private int hashRange;
        
        
        private void Start()
        {
            playerLocal = Networking.LocalPlayer;
            if (!Utilities.IsValid(playerLocal)) gameObject.SetActive(false);
            anim = GetComponent<Animator>();
            fov = FovZoomOut;
            
            camTransform = Cam.transform;
            roamingTransform = RoamingCamera.transform;
            
            Cam.enabled = false;
            CamUi.enabled = false;
            SetCamera(CAMERA_PRIMARY);
            
            hashFov = Animator.StringToHash("Fov");
            hashBlending = Animator.StringToHash("Blending");
            hashRange = Animator.StringToHash("Range");
        }
        
        public override void PostLateUpdate()
        {
            if (Input.GetKeyDown(InputActiveToggle))
            {
                Source.clip = ClipClick;
                Source.Play();
                
                active = !active;
                Cam.enabled = active;
                CamUi.enabled = active;
                
                if (roaming)
                {
                    SetRoaming(true);
                }
                else
                {
                    if (!directed) SetCamera(cameraType);
                }
            }
            if (Input.GetKeyDown(InputControlsToggle))
            {
                Source.clip = ClipClick;
                Source.Play();
                ToggleMenu();
            }
            
            if (debugVisible)
            {
                Vector3 playerPos = playerLocal.GetPosition();
                int levelTimeRaw = (int)Time.timeSinceLevelLoad;
                int levelTimeHour = Mathf.FloorToInt(levelTimeRaw / 3600f);
                int levelTimeMinute = Mathf.FloorToInt(levelTimeRaw / 60f);
                int levelTimeSecond = levelTimeRaw % 60;
                
                TextDebug.text = $"Xyz: {playerPos.x:f1}, {playerPos.y:f1}, {playerPos.z:f1}\n" +
                                 $"Player Count: {VRCPlayerApi.GetPlayerCount()} \n" +
                                 $"Local Time: {DateTime.Now:h:mm:ss}\n" +
                                 $"World Time: {levelTimeHour}:{levelTimeMinute}:{levelTimeSecond}";
            }
            
            if (!active) return;
            HandleInputs();
            anim.SetFloat(hashFov, Mathf.Lerp(anim.GetFloat(hashFov), fov, FOV_ZOOM_SPEED * Time.deltaTime));
            anim.SetFloat(hashRange, Mathf.Lerp(anim.GetFloat(hashRange), Range, RangeFadeSpeed * Time.deltaTime));
            
            VRCPlayerApi.TrackingData headData = playerLocal.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            HeadTracker.SetPositionAndRotation(headData.position, headData.rotation);
        }

        private void HandleInputs()
        {
            if (!directed)
            {
                if (roaming)
                {
                    if (Input.GetKey(KeyCode.Keypad8))
                    {
                        roamingTransform.position += roamingTransform.forward * (5 * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.Keypad4))
                    {
                        roamingTransform.position -= roamingTransform.right * (5 * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.Keypad5))
                    {
                        roamingTransform.position -= roamingTransform.forward * (5 * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.Keypad6))
                    {
                        roamingTransform.position += roamingTransform.right * (5 * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.KeypadPlus))
                    {
                        roamingTransform.position += Vector3.up * (5 * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.KeypadMinus))
                    {
                        roamingTransform.position -= Vector3.up * (5 * Time.deltaTime);
                    }
                    if (Input.GetKey(KeyCode.Keypad9))
                    {
                        roamingTransform.Rotate(Vector3.up, Space.World);
                    }
                    if (Input.GetKey(KeyCode.Keypad7))
                    {
                        roamingTransform.Rotate(Vector3.down, Space.World);
                    }
                    if (Input.GetKey(KeyCode.Keypad9))
                    {
                        roamingTransform.Rotate(Vector3.up, Space.World);
                    }
                    if (Input.GetKey(KeyCode.Keypad7))
                    {
                        roamingTransform.Rotate(Vector3.down, Space.World);
                    }
                    if (Input.GetKey(KeyCode.Keypad3))
                    {
                        roamingTransform.Rotate(Vector3.left, Space.Self);
                    }
                    if (Input.GetKey(KeyCode.Keypad1))
                    {
                        roamingTransform.Rotate(Vector3.right, Space.Self);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(InputPrimaryView) && cameraType != CAMERA_PRIMARY)
                    {
                        Source.clip = ClipClick;
                        Source.Play();
                        SetCamera(CAMERA_PRIMARY);
                    }
                    if (Input.GetKeyDown(InputLeftShoulderView) && cameraType != CAMERA_LEFT_SHOULDER)
                    {
                        Source.clip = ClipClick;
                        Source.Play();
                        SetCamera(CAMERA_LEFT_SHOULDER);
                    }
                    if (Input.GetKeyDown(InputRightShoulderView) && cameraType != CAMERA_RIGHT_SHOULDER)
                    {
                        Source.clip = ClipClick;
                        Source.Play();
                        SetCamera(CAMERA_RIGHT_SHOULDER);
                    }
                    if (Input.GetKeyDown(InputPrimaryRear) && cameraType != CAMERA_REAR_VIEW)
                    {
                        Source.clip = ClipClick;
                        Source.Play();
                        SetCamera(CAMERA_REAR_VIEW);
                    }
                    if (Input.GetKeyDown(InputShoulderViewToggle))
                    {
                        Source.clip = ClipClick;
                        Source.Play();
                        if (cameraType == CAMERA_RIGHT_SHOULDER)
                        {
                            SetCamera(CAMERA_LEFT_SHOULDER);
                        }
                        else
                        {
                            SetCamera(CAMERA_RIGHT_SHOULDER);
                        }
                    }
                    if (Input.GetKeyDown(InputForwardViewToggle))
                    {
                        Source.clip = ClipClick;
                        Source.Play();
                        if (cameraType == CAMERA_PRIMARY)
                        {
                            SetCamera(CAMERA_REAR_VIEW);
                        }
                        else
                        {
                            SetCamera(CAMERA_PRIMARY);
                        }
                    }
                }
                if (Input.GetKeyDown(InputRoamingToggle))
                {
                    Source.clip = ClipClick;
                    Source.Play();
                    SetRoaming(!roaming);
                }
                if (Input.GetKeyDown(InputBlending))
                {
                    Source.clip = ClipClick;
                    Source.Play();
                    blending = !blending;
                    anim.SetBool(hashBlending, blending);
                }
            }
            
            if (Input.GetKeyDown(InputFovToggle))
            {
                ToggleZoom();
            }
        }
        
        private void SetCamera(byte camType)
        {
            DirectedCamera.enabled = false;
            cameraType = camType;
            
            PrimaryCamera.enabled = cameraType == CAMERA_PRIMARY;
            LeftShoulderCamera.enabled = cameraType == CAMERA_LEFT_SHOULDER;
            RightShoulderCamera.enabled = cameraType == CAMERA_RIGHT_SHOULDER;
            RearCamera.enabled = cameraType == CAMERA_REAR_VIEW;
            RoamingCamera.enabled = cameraType == CAMERA_ROAMING;
        }
        
        private void ToggleMenu()
        {
            debugVisible ^= true;
            Controls.SetActive(debugVisible);
            ControlHint.gameObject.SetActive(false);
        }
        
        private void SetRoaming(bool value)
        {
            roaming = value;
            if (roaming)
            {
                byte camTypeCache = cameraType;
                bool blendingCache = blending;
                
                roamingTransform.SetPositionAndRotation(camTransform.position, camTransform.rotation);
                anim.SetBool(hashBlending, false);
                
                SetCamera(CAMERA_ROAMING);
                
                cameraType = camTypeCache;
                blending = blendingCache;
            }
            else
            {
                SetCamera(cameraType);
                anim.SetBool(hashBlending, blending);
            }
        }
        
        private void ToggleZoom()
        {
            if (fov == FovZoomIn)
            {
                Source.clip = ClipZoomOut;
                Source.Play();
                fov = FovZoomOut;
            }
            else
            {
                Source.clip = ClipZoomIn;
                Source.Play();
                fov = FovZoomIn;
            }
        }
        
        private void LockPlayer()
        {
            walkSpeed = playerLocal.GetWalkSpeed();
            runSpeed = playerLocal.GetRunSpeed();
            strafeSpeed = playerLocal.GetStrafeSpeed();
            jumpImpulse = playerLocal.GetJumpImpulse();
            
            playerLocal.SetWalkSpeed(0);
            playerLocal.SetRunSpeed(0);
            playerLocal.SetStrafeSpeed(0);
            playerLocal.SetJumpImpulse(0);
        }
        
        private void UnlockPlayer()
        {
            playerLocal.SetWalkSpeed(walkSpeed);
            playerLocal.SetRunSpeed(runSpeed);
            playerLocal.SetStrafeSpeed(strafeSpeed);
            playerLocal.SetJumpImpulse(jumpImpulse);
        }
        
        public void _StartDirection(Transform target, Transform lookAt, bool blendCamera, bool lockPlayer)
        {
            anim.SetBool(hashBlending, blendCamera);
            
            roaming = false;
            
            DirectedCamera.Follow = target;
            DirectedCamera.LookAt = lookAt;
            
            if (lockPlayer) LockPlayer();
            SendCustomEventDelayedFrames(nameof(InitializeDirection), 1);
        }
        
        public void InitializeDirection()
        {
            directed = true;
            PrimaryCamera.enabled = false;
            RightShoulderCamera.enabled = false;
            LeftShoulderCamera.enabled = false;
            RearCamera.enabled = false;
            DirectedCamera.enabled = true;
            RoamingCamera.enabled = false;
        }
        
        public void _EndDirection(bool lockPlayer)
        {
            directed = false;
            
            if (cameraType == CAMERA_ROAMING)
            {
                cameraType = CAMERA_PRIMARY;
            }
            
            anim.SetBool(hashBlending, blending);
            PrimaryCamera.enabled = cameraType == CAMERA_PRIMARY;
            LeftShoulderCamera.enabled = cameraType == CAMERA_LEFT_SHOULDER;
            RightShoulderCamera.enabled = cameraType == CAMERA_RIGHT_SHOULDER;
            RearCamera.enabled = cameraType == CAMERA_REAR_VIEW;
            DirectedCamera.enabled = false;
            RoamingCamera.enabled = false;
            
            if (lockPlayer) UnlockPlayer();
        }
    }
}