using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PreviewImageGenerator : EditorWindow
{
    [MenuItem("Tools/GeneratePreview")]
    private static void GeneratePreview()
    {
        GetWindow<PreviewImageGenerator>();
    }

    private void OnGUI()
    {
        GUILayout.Label("This creates a preview image");
        if (GUILayout.Button("Create"))
        {
            GetPreviews(Selection.activeObject as GameObject);
        }
    }

    private void GetPreviews(GameObject prefab)
    {
        AssetPreview.SetPreviewTextureCacheSize(2);

        Texture2D preview = null;
        do
        {
            preview = AssetPreview.GetAssetPreview(prefab);
        }
        while (AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()));

        if (preview != null)
        {
            preview.Apply();
            byte[] data = preview.EncodeToPNG();
            string texturesFolder = Path.Combine(Application.dataPath, "Textures");
            string previewsFolder = Path.Combine(texturesFolder, "GeneratedPreviews");
            string fileName = $"{prefab.name}.png";
            string fullPath = Path.Combine(previewsFolder, fileName);

            if (!File.Exists(fullPath))
            {
                File.WriteAllBytes(fullPath, data);
            }
        }
    }
}
