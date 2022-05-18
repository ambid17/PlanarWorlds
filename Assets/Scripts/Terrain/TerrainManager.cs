using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainManager : StaticMonoBehaviour<TerrainManager>
{
    public MeshMapEditor meshMapEditor;
    public TerrainInspector terrainInspector;

    void Start()
    {
        meshMapEditor.Enable();
        terrainInspector.SetTerrainMode();
    }

    public void PopulateCampaign(Campaign campaign)
    {
        meshMapEditor.SaveIntoCampaign(campaign);
    }

    public void LoadCampaign(Campaign campaign)
    {
        if (campaign.terrainData != null && campaign.terrainData.heightMap != null)
        {
            meshMapEditor.LoadFromCampaign(campaign);
        }
    }

    public bool TerrainNeedsSaved()
    {
        return meshMapEditor.IsDirty();
    }

    public void ClearAllTerrain()
    {
        meshMapEditor.Clear();
    }
}
