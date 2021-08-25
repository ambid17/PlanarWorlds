using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGridGenerator : MonoBehaviour
{
    [MenuItem("Tools/GenerateTileGrid")]
    private static void GenerateTileGrid()
    {
        try
        {
            TileGrid newTileGrid = ScriptableObject.CreateInstance<TileGrid>();

            FieldInfo[] tileFields = typeof(TileGrid)
                .GetFields()
                .Where(x => x.FieldType == typeof(Tile))
                .ToArray();

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (i > tileFields.Length - 1)
                    break;

                if (tileFields[i].FieldType == typeof(Tile))
                    tileFields[i].SetValue(newTileGrid, Selection.objects[i]);
            }

            EditorUtility.SetDirty(newTileGrid);

            string assetName = $"{Constants.tileGridDir}/TileGrid_{GUID.Generate()}";

            // Associate the new SO with an asset so it can persist 
            AssetDatabase.CreateAsset(newTileGrid, $"{assetName}.asset");
            AssetDatabase.SaveAssets();

            Selection.activeObject = newTileGrid;

            Debug.Log($"TileGrid '{newTileGrid.name}' created");
        }
        catch (Exception ex)
        {
            if (Selection.objects.Length <= 0)
                Debug.LogError("No items selected");
            else
                Debug.LogException(ex);
        }
    }
}
