
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VowganVR
{
    public class Screenshotter : EditorWindow
    {
        
        // Date  9-13-2021
        // Build v0.4
        
        private bool transparent;
        private bool autoOpen = true;
        private string folderPath = "";
        private string lastImage = "";
        
        private string[] sFormat =
        {
            "SceneName Hour-Minute-Second",
            "SceneName Month-Date-Year Hour-Minute-Second",
            "Month-Date-Year Hour-Minute-Second",
            "CustomName Hour-Minute-Second"
        };
        
        private readonly Vector2[] presets =
        {
            new Vector2(1920, 1080),
            new Vector2(1920, 1080),
            new Vector2(1920, 1080),
            new Vector2(2560, 1440),
            new Vector2(3840, 2160),
            new Vector2(7680, 4320),
            new Vector2(320, 240),
            new Vector2(640, 480),
            new Vector2(800, 600),
            new Vector2(1024, 768),
            new Vector2(1280, 960),
            new Vector2(1536, 1180),
            new Vector2(1600, 1200),
            new Vector2(2048, 1536),
            new Vector2(2240, 1680),
            new Vector2(2560, 1920),
            new Vector2(3032, 2008),
            new Vector2(3072, 2304),
            new Vector2(3264, 2448),
        };

        private readonly string[] presetsString =
        {
            "Game Resolution",
            "Custom",
            "1920, 1080",
            "2560, 1440",
            "3840, 2160",
            "7680, 4320",
            "320, 240",
            "640, 480",
            "800, 600",
            "1024, 768",
            "1280, 960",
            "1536, 1180",
            "1600, 1200",
            "2048, 1536",
            "2240, 1680",
            "2560, 1920",
            "3032, 2008",
            "3072, 2304",
            "3264, 2448"
        };
        
        private string customName = "Screenshot";
        private int sFormatIndex;
        private int presetsIndex;

        private Camera captureCamera;
        private int imageWidth = 1920;
        private int imageHeight = 1080;

        private bool showCameraSelection = true;
        private bool showImageResolution = true;
        private bool showSaveImage = true;

        private GUIStyle iconStyle;
        
        private const string PrefTransparent = "Vowgan/ScreenshotterTransparent";
        private const string PrefAutoOpen = "Vowgan/ScreenshotterAutoOpen";
        private const string PrefFolderPath = "Vowgan/ScreenshotterFolderPath";
        private const string PrefLastImage = "Vowgan/ScreenshotterLastImage";
        private const string PrefImageWidth = "Vowgan/ScreenshotterImageWidth";
        private const string PrefImageHeight = "Vowgan/ScreenshotterImageHeight";
        private const string PrefShowCameraSelection = "Vowgan/ScreenshotterShowCameraSelection";
        private const string PrefShowImageResolution = "Vowgan/ScreenshotterShowImageResolution";
        private const string PrefShowSaveImage = "Vowgan/ScreenshotterShowSaveImage";
        private const string PrefSFormatIndex = "Vowgan/ScreenshotterSFormatIndex";
        private const string PrefPresetsIndex = "Vowgan/ScreenshotterPresetsIndex";
        
        
        [MenuItem("Tools/Vowgan/Screenshotter")]
        public static void ShowWindow()
        {
            EditorWindow win = GetWindow<Screenshotter>();
            win.minSize = new Vector2(230, 300);
            win.titleContent.image = EditorGUIUtility.ObjectContent(null, typeof(Camera)).image;
            win.titleContent.text = "Screenshotter";
            win.Show();
        }

        private void OnEnable()
        {
            iconStyle = new GUIStyle
            {
                normal =
                {
                    background = Resources.Load("VV Icon") as Texture2D,
                },
                fixedHeight = 64,
                fixedWidth = 64
            };
            
            if (EditorPrefs.HasKey(PrefTransparent)) transparent = EditorPrefs.GetBool(PrefTransparent); 
            if (EditorPrefs.HasKey(PrefAutoOpen)) autoOpen = EditorPrefs.GetBool(PrefAutoOpen);
            if (EditorPrefs.HasKey(PrefFolderPath)) folderPath = EditorPrefs.GetString(PrefFolderPath);
            if (EditorPrefs.HasKey(PrefLastImage)) lastImage = EditorPrefs.GetString(PrefLastImage);
            if (EditorPrefs.HasKey(PrefImageWidth)) imageWidth = EditorPrefs.GetInt(PrefImageWidth);
            if (EditorPrefs.HasKey(PrefImageHeight)) imageHeight = EditorPrefs.GetInt(PrefImageHeight);
            if (EditorPrefs.HasKey(PrefShowCameraSelection)) showCameraSelection = EditorPrefs.GetBool(PrefShowCameraSelection);
            if (EditorPrefs.HasKey(PrefShowImageResolution)) showImageResolution = EditorPrefs.GetBool(PrefShowImageResolution);
            if (EditorPrefs.HasKey(PrefShowSaveImage)) showSaveImage = EditorPrefs.GetBool(PrefShowSaveImage);
            if (EditorPrefs.HasKey(PrefSFormatIndex)) sFormatIndex = EditorPrefs.GetInt(PrefSFormatIndex);
            if (EditorPrefs.HasKey(PrefPresetsIndex)) presetsIndex = EditorPrefs.GetInt(PrefPresetsIndex);
        }

        private void OnGUI()
        {
            DrawTitle();

            GUILayout.Space(5);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            CameraSelection();

            GUILayout.Space(5);

            ImageResolution();

            GUILayout.Space(5);

            SaveImage();
            GUILayout.Space(5);
            GUILayout.EndVertical();

            SetEditorPrefs();
        }
        
        private void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();
            GUILayout.Box("", iconStyle);
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            
            GUILayout.Label("Screenshotter Settings", EditorStyles.boldLabel);
            GUILayout.Label("VowganVR");
            
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Capture Image", GUILayout.MinHeight(60)))
            {
                if (folderPath == "")
                {
                    folderPath = EditorUtility.SaveFolderPanel("Image Save Location", folderPath, Application.dataPath);
                    RenderImage();
                }
                else
                {
                    RenderImage();
                }
            }

            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(lastImage == string.Empty);
            if (GUILayout.Button("Open Screenshot", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
            {
                Application.OpenURL("file://" + lastImage);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Open Folder", EditorStyles.miniButtonRight, GUILayout.Height(20)) && !string.IsNullOrEmpty(folderPath))
            {
                EditorUtility.RevealInFinder(folderPath);
            }

            GUILayout.EndHorizontal();
            
            GUILayout.Space(2);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);
            
        }
        
        private void CameraSelection()
        {
            showCameraSelection = EditorGUILayout.Foldout(showCameraSelection, "Select Camera", true);
            if (showCameraSelection)
            {
                GUILayout.BeginVertical("box");
                
                captureCamera = (Camera) EditorGUILayout.ObjectField(captureCamera, typeof(Camera), true);
    
                if (captureCamera == null) captureCamera = Camera.main;
    
                transparent = EditorGUILayout.ToggleLeft("Transparent", transparent);
                autoOpen = EditorGUILayout.ToggleLeft("Open Image", autoOpen);
                GUILayout.Space(2);
                GUILayout.EndVertical();
            }
        }

        private void ImageResolution()
        {
            showImageResolution = EditorGUILayout.Foldout(showImageResolution, "Resolution", true);
            if (showImageResolution)
            {
                GUILayout.BeginVertical("box");
                
                EditorGUI.BeginChangeCheck();
                presetsIndex = EditorGUILayout.Popup("Options", presetsIndex, presetsString);
                if (EditorGUI.EndChangeCheck())
                {
                    imageWidth = (int) presets[presetsIndex].x;
                    imageHeight = (int) presets[presetsIndex].y;
                }
                if (presetsIndex == 1)
                {
                    imageWidth = EditorGUILayout.IntField("Width", imageWidth);
                    imageHeight = EditorGUILayout.IntField("Height", imageHeight);
                    if (imageHeight <= 0) imageHeight = 1;
                    if (imageWidth <= 0) imageWidth = 1;
                }

                GUILayout.Space(4);
                GUILayout.EndVertical();
            }
        }

        private void SaveImage()
        {
            showSaveImage = EditorGUILayout.Foldout(showSaveImage, "Save Image", true);
            if (showSaveImage)
            {
                GUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(folderPath);
                if (GUILayout.Button("Browse", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
                {
                    folderPath = EditorUtility.SaveFolderPanel("Path to Save Images", folderPath, Application.dataPath);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                sFormatIndex = EditorGUILayout.Popup("Naming Format", sFormatIndex, sFormat);

                if (sFormatIndex == 3 ^ sFormatIndex == 4)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Custom Name");
                    customName = GUILayout.TextField(customName);
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(8);
                GUILayout.EndVertical();
            }
        }

        private void SetEditorPrefs()
        {
            EditorPrefs.SetBool(PrefTransparent, transparent);
            EditorPrefs.SetBool(PrefAutoOpen, autoOpen);
            EditorPrefs.SetString(PrefFolderPath, folderPath);
            EditorPrefs.SetString(PrefLastImage, lastImage);
            EditorPrefs.SetInt(PrefImageWidth, imageWidth);
            EditorPrefs.SetInt(PrefImageHeight, imageHeight);
            EditorPrefs.SetBool(PrefShowImageResolution, showImageResolution);
            EditorPrefs.SetBool(PrefShowCameraSelection, showCameraSelection);
            EditorPrefs.SetBool(PrefShowSaveImage, showSaveImage);
            EditorPrefs.SetInt(PrefSFormatIndex, sFormatIndex);
            EditorPrefs.SetInt(PrefPresetsIndex, presetsIndex);
        }

        private void RenderImage()
        {
            var rt = new RenderTexture(imageWidth, imageHeight, 24);
            captureCamera.targetTexture = rt;

            TextureFormat tFormat;

            if (transparent)
            {
                tFormat = TextureFormat.ARGB32;
            }
            else
            {
                tFormat = TextureFormat.RGB24;
            }

            Vector2 resolution;
            switch (presetsIndex)
            {
                case 0: // Game Resolution
                    resolution = new Vector2(Handles.GetMainGameViewSize().x, Handles.GetMainGameViewSize().y);
                    break;
                case 1: // Custom
                    resolution = new Vector2(imageWidth, imageHeight);
                    break;
                default: // Use Preset
                    resolution = presets[presetsIndex];
                    break;
            }
            
            Texture2D screenShot = new Texture2D((int)resolution.x, (int)resolution.y, tFormat, false);

            if (transparent)
            {
                var nativeFlags = captureCamera.clearFlags;
                captureCamera.clearFlags = CameraClearFlags.Nothing;
                captureCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
                captureCamera.targetTexture = null;
                RenderTexture.active = null;
                captureCamera.clearFlags = nativeFlags;
            }
            else
            {
                captureCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
                captureCamera.targetTexture = null;
                RenderTexture.active = null;
            }

            byte[] bytes = screenShot.EncodeToPNG();
            var filename = NameImage();

            System.IO.File.WriteAllBytes(filename, bytes);
            if (autoOpen) Application.OpenURL(filename);
        }
        
        private string NameImage()
        {
            string newName = "Screenshot";
            var scene = SceneManager.GetActiveScene();

            switch (sFormatIndex)
            {
                case 0:
                    newName = folderPath + "/" + scene.name + "_" + DateTime.Now.ToString("HH-mm-ss") + ".png";
                    break;
                case 1:
                    newName = folderPath + "/" + scene.name + "_" + DateTime.Now.ToString("MM-dd-yyyy'_'HH-mm-ss") + ".png";
                    break;
                case 2:
                    newName = folderPath + "/" + DateTime.Now.ToString("MM-dd-yyyy'_'HH-mm-ss") + ".png";
                    break;
                case 3:
                    newName = folderPath + "/" + customName + "_" + DateTime.Now.ToString("HH-mm-ss") + ".png";
                    break;
            }

            lastImage = newName;

            return newName;
        }

    }
}