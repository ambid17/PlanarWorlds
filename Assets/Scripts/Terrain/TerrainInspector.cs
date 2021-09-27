using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainInspector : MonoBehaviour
{
    public GameObject tileMapInspector;
    public GameObject meshMapInspector;

    void Start()
    {
        
    }

    public void SetTerrainMode(TerrainMode newMode)
    {
        tileMapInspector.SetActive(newMode == TerrainMode.TileMap);
        meshMapInspector.SetActive(newMode == TerrainMode.Mesh);
    }
}
