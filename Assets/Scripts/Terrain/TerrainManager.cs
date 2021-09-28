using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainMode
{
    TileMap, Mesh
}

public class TerrainManager : StaticMonoBehaviour<TerrainManager>
{
    public TileMapEditor tileMapEditor;
    public MeshMapEditor meshMapEditor;
    public TerrainMode currentTerrainMode;
    public TerrainInspector terrainInspector;

    void Start()
    {
        SetTerrainMode(TerrainMode.Mesh);
    }

    public void SetTerrainMode(TerrainMode newMode)
    {
        if(newMode == TerrainMode.Mesh)
        {
            tileMapEditor.Disable();
            meshMapEditor.Enable();
        }
        else
        {
            tileMapEditor.Enable();
            meshMapEditor.Disable();
        }

        currentTerrainMode = newMode;

        terrainInspector.SetTerrainMode(currentTerrainMode);
    }

    public void PopulateCampaign(Campaign campaign)
    {
        if(currentTerrainMode == TerrainMode.TileMap)
        {
            tileMapEditor.SaveIntoCampaign(campaign);
        }
        else
        {
            meshMapEditor.SaveIntoCampaign(campaign);
        }
    }

    public void LoadCampaign(Campaign campaign)
    {
        if(campaign.tiles.Count > 0)
        {
            SetTerrainMode(TerrainMode.TileMap);
            tileMapEditor.LoadFromCampaign(campaign);

        }
        else if (campaign.terrainData != null && campaign.terrainData.heightMap != null)
        {
            SetTerrainMode(TerrainMode.Mesh);
            meshMapEditor.LoadFromCampaign(campaign);

        }
    }

    public bool TerrainNeedsSaved()
    {
        if (currentTerrainMode == TerrainMode.TileMap)
        {
            return tileMapEditor.IsDirty();
        }
        else
        {
            return meshMapEditor.IsDirty();
        }
    }

    public void ClearAllTerrain()
    {
        tileMapEditor.Clear();
        meshMapEditor.Clear();
    }
}
