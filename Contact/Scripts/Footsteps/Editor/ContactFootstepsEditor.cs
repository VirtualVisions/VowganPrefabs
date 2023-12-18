
using UnityEditor;
using UnityEngine.UIElements;

namespace Vowgan.Contact.Footsteps
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ContactFootsteps))]
    public class ContactFootstepsEditor : Editor
    {
        public VisualTreeAsset InspectorTree;
        
        private SerializedProperty propPresets;
        private SerializedProperty propGroundLayers;


        private void OnEnable()
        {
            propGroundLayers = serializedObject.FindProperty(nameof(ContactFootsteps.GroundLayers));
            propPresets = serializedObject.FindProperty(nameof(ContactFootsteps.Presets));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            InspectorTree.CloneTree(root);

            IMGUIContainer groundLayersContainer = root.Query<IMGUIContainer>("GroundLayersContainer");
            groundLayersContainer.onGUIHandler += GroundLayersContainerGUI;

            IMGUIContainer presetsContainer = root.Query<IMGUIContainer>("PresetsContainer");
            presetsContainer.onGUIHandler += PresetsContainerGUI;

            Button findAllPresetsButton = root.Query<Button>("FindAllPresetsButton");
            findAllPresetsButton.clicked += FindAllPresetsButtonClicked;
            
            return root;
        }

        private void FindAllPresetsButtonClicked()
        {
            string[] guids = AssetDatabase.FindAssets("t:ContactFootstepPreset");
            propPresets.arraySize = guids.Length;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ContactFootstepPreset preset = AssetDatabase.LoadAssetAtPath<ContactFootstepPreset>(path);

                propPresets.GetArrayElementAtIndex(i).objectReferenceValue = preset;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GroundLayersContainerGUI()
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(propGroundLayers);
                if (changed.changed) serializedObject.ApplyModifiedProperties();
            }
        }

        private void PresetsContainerGUI()
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(propPresets);
                if (changed.changed) serializedObject.ApplyModifiedProperties();
            }
        }
    }
}