using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vowgan.Inventory
{
    public class SavePrefabThumbnail : Editor
    {
        
        [MenuItem("Assets/Save Prefab Thumbnail", true)]
        private static bool SaveThumbnailValidation()
        {
            Object obj = Selection.activeObject;
            if (obj == null) return false;
            return obj is GameObject;
        }

        [MenuItem("Assets/Save Prefab Thumbnail")]
        private static void SaveThumbnail()
        {
            UnityEngine.Object obj = Selection.activeObject;

            string assetPath = AssetDatabase.GetAssetPath(obj);
            string directoryName = Path.GetDirectoryName(assetPath);
            string fileName = Path.GetFileNameWithoutExtension(assetPath) + " Icon.png";
            string filePath = Path.Combine(directoryName, fileName);

            Texture2D thumbnail = AssetPreview.GetAssetPreview(obj);
            if (thumbnail == null)
            {
                Debug.LogError("Saving Prefab thumbnail failed: Preview is null.");
                return;
            }

            byte[] file = thumbnail.EncodeToPNG();
            File.WriteAllBytes(filePath, file);


            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            }
            
            
            AssetDatabase.Refresh();
            Debug.Log($"Saved thumbnail: {fileName}", AssetDatabase.LoadAssetAtPath<Texture2D>(filePath));

        }
        
    }
}