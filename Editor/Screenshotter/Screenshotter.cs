
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace VowganVR
{
    public class Screenshotter : EditorWindow
    {
        
        public static Camera SavedCamera;
        
        public Camera CaptureCamera;
        public bool Transparent;
        public bool OpenImageAfterCapture = true;
        public string SaveLocation = "";
        public string LastImage = "";
        public ResolutionPresets ResolutionPresetValue;
        public int ImageWidth = 1920;
        public int ImageHeight = 1080;
        public NamingFormats NamingFormatValue;
        public string CustomName = "Screenshot";
        
        public enum NamingFormats
        {
            SceneNameHhMmSs,
            SceneNameMonthDateYearHhMmSs,
            MonthDateYearHhMmSs,
            CustomNameHhMmSs
        };

        public enum ResolutionPresets
        {
            GameView,
            Custom,
        };
        
        private const string PREF_TRANSPARENT = "Vowgan/Screenshotter/Transparent";
        private const string PREF_AUTO_OPEN = "Vowgan/Screenshotter/AutoOpen";
        private const string PREF_SAVE_LOCATION = "Vowgan/Screenshotter/SaveLocation";
        private const string PREF_LAST_IMAGE = "Vowgan/Screenshotter/LastImage";
        private const string PREF_IMAGE_WIDTH = "Vowgan/Screenshotter/ImageWidth";
        private const string PREF_IMAGE_HEIGHT = "Vowgan/Screenshotter/ImageHeight";
        private const string PREF_SAVE_FORMAT = "Vowgan/Screenshotter/SFormatIndex";
        private const string PREF_PRESETS_INDEX = "Vowgan/Screenshotter/PresetsIndex";

        private Button CaptureButton;
        private Button ScreenshotButton;
        private Button FolderButton;
        private ObjectField CameraField;
        private Toggle TransparentToggle;
        private Toggle OpenImageToggle;
        private EnumField ResolutionField;
        private IntegerField WidthField;
        private IntegerField HeightField;
        private TextField SaveLocationField;
        private Button BrowseButton;
        private EnumField NamingFormatField;
        private TextField CustomNameField;

        private SerializedObject serializedObject;
        private SerializedProperty propCaptureCamera;
        private SerializedProperty propTransparent;
        private SerializedProperty propOpenImageAfterCapture;
        private SerializedProperty propSaveLocation;
        private SerializedProperty propLastImage;
        private SerializedProperty propResolutionPresetIndex;
        private SerializedProperty propImageWidth;
        private SerializedProperty propImageHeight;
        private SerializedProperty propCustomName;
        private SerializedProperty propNameFormatIndex;


        [MenuItem("Tools/Vowgan/Screenshotter")]
        public static void ShowWindow()
        {
            EditorWindow win = GetWindow<Screenshotter>();
            win.minSize = new Vector2(150, 100);
            win.titleContent.image = EditorGUIUtility.ObjectContent(null, typeof(Camera)).image;
            win.titleContent.text = "Screenshotter";
            win.Show();
        }

        private void OnEnable()
        {
            CaptureCamera = SavedCamera;
            if (CaptureCamera == null)
            {
                CaptureCamera = Camera.main;
                SavedCamera = CaptureCamera;
            }
            
            EditorSceneManager.sceneOpened += (scene, mode) =>
            {
                CaptureCamera = SavedCamera;
                if (CaptureCamera == null)
                {
                    CaptureCamera = Camera.main;
                    SavedCamera = CaptureCamera;
                }
            };
            
            if (EditorPrefs.HasKey(PREF_TRANSPARENT)) Transparent = EditorPrefs.GetBool(PREF_TRANSPARENT);
            if (EditorPrefs.HasKey(PREF_AUTO_OPEN)) OpenImageAfterCapture = EditorPrefs.GetBool(PREF_AUTO_OPEN);
            if (EditorPrefs.HasKey(PREF_SAVE_LOCATION)) SaveLocation = EditorPrefs.GetString(PREF_SAVE_LOCATION);
            if (EditorPrefs.HasKey(PREF_LAST_IMAGE)) LastImage = EditorPrefs.GetString(PREF_LAST_IMAGE);
            if (EditorPrefs.HasKey(PREF_IMAGE_WIDTH)) ImageWidth = EditorPrefs.GetInt(PREF_IMAGE_WIDTH);
            if (EditorPrefs.HasKey(PREF_IMAGE_HEIGHT)) ImageHeight = EditorPrefs.GetInt(PREF_IMAGE_HEIGHT);
            if (EditorPrefs.HasKey(PREF_SAVE_FORMAT)) NamingFormatValue = (NamingFormats)EditorPrefs.GetInt(PREF_SAVE_FORMAT);
            if (EditorPrefs.HasKey(PREF_PRESETS_INDEX)) ResolutionPresetValue = (ResolutionPresets)EditorPrefs.GetInt(PREF_PRESETS_INDEX);
            
            serializedObject = new(this);
            propCaptureCamera = serializedObject.FindProperty(nameof(CaptureCamera));
            propTransparent = serializedObject.FindProperty(nameof(Transparent));
            propOpenImageAfterCapture = serializedObject.FindProperty(nameof(OpenImageAfterCapture));
            propSaveLocation = serializedObject.FindProperty(nameof(SaveLocation));
            propLastImage = serializedObject.FindProperty(nameof(LastImage));
            propResolutionPresetIndex = serializedObject.FindProperty(nameof(ResolutionPresetValue));
            propImageWidth = serializedObject.FindProperty(nameof(ImageWidth));
            propImageHeight = serializedObject.FindProperty(nameof(ImageHeight));
            propCustomName = serializedObject.FindProperty(nameof(CustomName));
            propNameFormatIndex = serializedObject.FindProperty(nameof(NamingFormatValue));
        }

        private void CreateGUI()
        {
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("ScreenshotterUXML");
            uxml.CloneTree(rootVisualElement);

            rootVisualElement.Bind(new(this));
            
            CaptureButton = rootVisualElement.Query<Button>("CaptureButton");
            CaptureButton.clicked += CaptureButtonOnClicked;
            
            ScreenshotButton = rootVisualElement.Query<Button>("ScreenshotButton");
            ScreenshotButton.clicked += () => Application.OpenURL("file://" + LastImage);
            
            FolderButton = rootVisualElement.Query<Button>("FolderButton");
            FolderButton.clicked += () => EditorUtility.RevealInFinder(SaveLocation);
            
            CameraField = rootVisualElement.Query<ObjectField>("CameraField");
            CameraField.RegisterValueChangedCallback(evt =>
            {
                SavedCamera = (Camera)evt.newValue;
                SetEditorPrefs();
            });
            
            TransparentToggle = rootVisualElement.Query<Toggle>("TransparentToggle");
            TransparentToggle.RegisterValueChangedCallback(evt => SetEditorPrefs());
            
            OpenImageToggle = rootVisualElement.Query<Toggle>("OpenImageToggle");
            OpenImageToggle.RegisterValueChangedCallback(evt => SetEditorPrefs());
            
            ResolutionField = rootVisualElement.Query<EnumField>("ResolutionField");
            ResolutionField.RegisterValueChangedCallback(evt =>
            {
                WidthField.SetEnabled(ResolutionPresetValue == ResolutionPresets.Custom);
                HeightField.SetEnabled(ResolutionPresetValue == ResolutionPresets.Custom);
                SetEditorPrefs();
            });
            ResolutionField.SetValueWithoutNotify(ResolutionPresetValue);
            
            WidthField = rootVisualElement.Query<IntegerField>("WidthField");
            WidthField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue <= 0) WidthField.value = 1;
                SetEditorPrefs();
            });
            
            HeightField = rootVisualElement.Query<IntegerField>("HeightField");
            HeightField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue <= 0) HeightField.value = 1;
                SetEditorPrefs();
            });
            
            SaveLocationField = rootVisualElement.Query<TextField>("SaveLocationField");
            SaveLocationField.RegisterValueChangedCallback(evt => SetEditorPrefs());
            
            BrowseButton = rootVisualElement.Query<Button>("BrowseButton");
            BrowseButton.clicked += () => SaveLocation = EditorUtility.SaveFolderPanel(
                "Path to Save Images", SaveLocation, Application.dataPath);
            
            NamingFormatField = rootVisualElement.Query<EnumField>("NamingFormatField");
            NamingFormatField.RegisterValueChangedCallback(evt =>
            {
                CustomNameField.SetEnabled(NamingFormatValue == NamingFormats.CustomNameHhMmSs);
                SetEditorPrefs();
            });

            CustomNameField = rootVisualElement.Query<TextField>("CustomNameField");
            CustomNameField.RegisterValueChangedCallback(evt => SetEditorPrefs());
            
            
            WidthField.SetEnabled(ResolutionPresetValue == ResolutionPresets.Custom);
            HeightField.SetEnabled(ResolutionPresetValue == ResolutionPresets.Custom);
            CustomNameField.SetEnabled(NamingFormatValue == NamingFormats.CustomNameHhMmSs);
        }

        private void CaptureButtonOnClicked()
        {
            if (SaveLocation == "")
            {
                SaveLocation = EditorUtility.SaveFolderPanel("Image Save Location", SaveLocation, Application.dataPath);
                RenderImage();
            }
            else
            {
                RenderImage();
            }
        }
        
        private void SetEditorPrefs()
        {
            EditorPrefs.SetBool(PREF_TRANSPARENT, Transparent);
            EditorPrefs.SetBool(PREF_AUTO_OPEN, OpenImageAfterCapture);
            EditorPrefs.SetString(PREF_SAVE_LOCATION, SaveLocation);
            EditorPrefs.SetString(PREF_LAST_IMAGE, LastImage);
            EditorPrefs.SetInt(PREF_IMAGE_WIDTH, ImageWidth);
            EditorPrefs.SetInt(PREF_IMAGE_HEIGHT, ImageHeight);
            EditorPrefs.SetInt(PREF_SAVE_FORMAT, (int)NamingFormatValue);
            EditorPrefs.SetInt(PREF_PRESETS_INDEX, (int)ResolutionPresetValue);
        }

        private void RenderImage()
        {
            Vector2 resolution;
            switch (ResolutionPresetValue)
            {
                default:
                case ResolutionPresets.GameView:
                    resolution = Handles.GetMainGameViewSize();
                    break;
                case ResolutionPresets.Custom:
                    resolution = new(ImageWidth, ImageHeight);
                    break;
            }

            RenderTexture rt = new((int)resolution.x, (int)resolution.y, 24);
            CaptureCamera.targetTexture = rt;

            TextureFormat textureFormat = Transparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;

            Texture2D screenShot = new((int)resolution.x, (int)resolution.y, textureFormat, false);

            if (Transparent)
            {
                CameraClearFlags nativeFlags = CaptureCamera.clearFlags;
                CaptureCamera.clearFlags = CameraClearFlags.Nothing;
                CaptureCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new(0, 0, resolution.x, resolution.y), 0, 0);
                CaptureCamera.targetTexture = null;
                RenderTexture.active = null;
                CaptureCamera.clearFlags = nativeFlags;
            }
            else
            {
                CaptureCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new(0, 0, resolution.x, resolution.y), 0, 0);
                CaptureCamera.targetTexture = null;
                RenderTexture.active = null;
            }

            byte[] bytes = screenShot.EncodeToPNG();
            string filename = NameImage();

            System.IO.File.WriteAllBytes(filename, bytes);
            if (OpenImageAfterCapture) Application.OpenURL(filename);
        }

        private string NameImage()
        {
            string newName = "Screenshot";
            Scene scene = SceneManager.GetActiveScene();
            
            switch (NamingFormatValue)
            {
                case NamingFormats.SceneNameHhMmSs:
                    newName = $"{SaveLocation}/{scene.name}_{DateTime.Now:HH-mm-ss}.png";
                    break;
                case NamingFormats.SceneNameMonthDateYearHhMmSs:
                    newName = $"{SaveLocation}/{scene.name}_{DateTime.Now:MM-dd-yyyy'_'HH-mm-ss}.png";
                    break;
                case NamingFormats.MonthDateYearHhMmSs:
                    newName = $"{SaveLocation}/{DateTime.Now:MM-dd-yyyy'_'HH-mm-ss}.png";
                    break;
                case NamingFormats.CustomNameHhMmSs:
                    newName = $"{SaveLocation}/{CustomName}_{DateTime.Now:HH-mm-ss}.png";
                    break;
            }
            
            LastImage = newName;
            
            return newName;
        }
    }
}