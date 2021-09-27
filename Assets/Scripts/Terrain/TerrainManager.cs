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

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ChangeTerrainMode(TerrainMode newMode)
    {
        if (currentTerrainMode != newMode)
        {
            if(newMode == TerrainMode.Mesh)
            {
                tileMapEditor.Clear();
            }
            else
            {
                meshMapEditor.Clear();
            }
        }

        currentTerrainMode = newMode;
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
        if (currentTerrainMode == TerrainMode.TileMap)
        {
            tileMapEditor.LoadFromCampaign(campaign);
        }
        else
        {
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
