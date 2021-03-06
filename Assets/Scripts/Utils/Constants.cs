using UnityEngine;

public class Constants
{
    public static int UILayer = 5;
    public static int PrefabParentLayer = 6;
    public static int PrefabChildLayer = 7;
    public static int TerrainLayer = 8;
    public static int PrefabPlacementLayer = 9;
    public static KeyCode positionHotkey = KeyCode.Z;
    public static KeyCode rotationHotkey = KeyCode.X;
    public static KeyCode scaleHotkey = KeyCode.C;

    public static int defaultMapSize = 50;
    public static int defaultBrushSize = 1;
    public static float rotationSpeed = 15;


    public static Color ShadowTileColor = new Color(1, 1, 1, 0.6f);

    public static string UnsignedIntegerPattern = @"^[0-9]*$";

    public static string TileGridDir = "Assets/ScriptableObjects/TileGrids";

    public static string recentCampainsFileName = "recentCampaigns.json";

}
