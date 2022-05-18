using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainManager : StaticMonoBehaviour<TerrainManager>
{
    public TerrainEditor terrainEditor;

    void Start()
    {
        terrainEditor.Enable();
    }

    public void PopulateCampaign(Campaign campaign)
    {
        terrainEditor.SaveIntoCampaign(campaign);
    }

    public void LoadCampaign(Campaign campaign)
    {
        if (campaign.terrainData != null && campaign.terrainData.heightMap != null)
        {
            terrainEditor.LoadFromCampaign(campaign);
        }
    }

    public bool TerrainNeedsSaved()
    {
        return terrainEditor.IsDirty();
    }

    public void ClearAllTerrain()
    {
        terrainEditor.Clear();
    }
}
