using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading;
using System.IO;

// This editor script:
// 1. Gets all prefabs in the hard-coded folders
// 2. Generates object previews for each of those prefabs
// 2. Puts the prefabs and their previews in the PrefabList ScriptableObject
public class PrefabListGenerator : EditorWindow
{
    [SerializeField] private PrefabList prefabList;

    [MenuItem("Tools/GeneratePrefabList")]
    private static void GeneratePrefabList()
    {
        GetWindow<PrefabListGenerator>();
    }

    private void OnGUI()
    {
        prefabList = GetFirstInstance<PrefabList>();

        GUILayout.Label("This populates the prefab list");

        if (GUILayout.Button("Populate"))
        {
            string[] foldersToGetAssetsFrom = new string[]
            {
                "Assets/Models/TestAssets/Campfire",
                "Assets/Models/TestAssets/Maple Tree",
                "Assets/Models/TestAssets/Rocks",
                "Assets/Models/TestAssets/Wood Cart",
                "Assets/Models/TestAssets/House1"
            };

            GameObject[] prefabs = GetAllPrefabs(foldersToGetAssetsFrom);
            List<Texture2D> previews = GetPreviews(prefabs);
            PopulatePrefabs(prefabs, previews);
            EditorUtility.SetDirty(prefabList);
        }
    }

    private List<Texture2D> GetPreviews(GameObject[] prefabs)
    {
        List<Texture2D> previews = new List<Texture2D>();

        AssetPreview.SetPreviewTextureCacheSize(prefabs.Length + 1);

        foreach (GameObject go in prefabs)
        {
            Texture2D preview = null;
            do
            {
                preview = AssetPreview.GetAssetPreview(go);
            }
            while (AssetPreview.IsLoadingAssetPreview(go.GetInstanceID()));

            if(preview != null)
            {
                preview.Apply();
                byte[] data = preview.EncodeToPNG();
                string folder = Path.Combine(
                                        Path.Combine(Application.dataPath, "Textures"), 
                                        "GeneratedPreviews");
                string fileName = $"{go.name}.png";
                string fullPath = Path.Combine(folder, fileName);

                if (!File.Exists(fullPath))
                {
                    File.WriteAllBytes(fullPath, data);
                }

                Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Textures/GeneratedPreviews/{fileName}");
                previews.Add(savedTexture);
            }
        }

        return previews;
    }

    private void PopulatePrefabs(GameObject[] prefabs, List<Texture2D> previews)
    {
        Prefab[] generatedPrefabs = new Prefab[prefabs.Length];

        for (int i = 0; i < prefabs.Length; i++)
        {
            generatedPrefabs[i] = new Prefab()
            {
                gameObject = prefabs[i],
                previewTexture = previews[i],
                prefabId = i,
                prefabName = prefabs[i].name
            };
        }

        prefabList.prefabs = generatedPrefabs;
    }

    public static GameObject[] GetAllPrefabs(string[] searchInFolders)
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(GameObject).Name}", searchInFolders);  //FindAssets uses tags 
        GameObject[] toReturn = new GameObject[guids.Length];
        for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            toReturn[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        return toReturn;
    }

    public static T GetFirstInstance<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name); 
        T toReturn = null;
        
        if(guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            toReturn = AssetDatabase.LoadAssetAtPath<T>(path);
        }
        else
        {
            Debug.LogError($"No asset of type {typeof(T).Name} exists in the asset database");
        }

        return toReturn;
    }
}
