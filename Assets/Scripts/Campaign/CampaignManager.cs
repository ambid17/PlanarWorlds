using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;
using RTG;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine.Events;

public class CampaignManager : StaticMonoBehaviour<CampaignManager>
{
    public RecentCampaigns recentCampaigns;
    public event Action OnRecentCampaignsUpdated;

    private Campaign _currentCampaign;

    private PrefabManager _prefabManager;
    private TerrainManager _terrainManager;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _prefabManager = PrefabManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();

        LoadRecentCampaigns();
    }

    #region Campaign Utils
    public void NewCampaign(string path)
    {
        if (_currentCampaign != null)
        {
            SaveCampaign();
        }

        ClearOldData();
        _currentCampaign = new Campaign();
    }

    public void UpdateCampaignName(string name)
    {
        _currentCampaign.UpdateName(name);
    }

    public void LoadCampaign(string path)
    {
        if (_currentCampaign != null)
        {
            SaveCampaign();
        }

        _currentCampaign = Campaign.LoadFromPath(path);
        ClearOldData();
        LoadPrefabs();
        LoadTiles();
        AddToRecentCampaigns(path);
    }

    public void SaveCampaign()
    {
        if (_currentCampaign == null)
        {
            Debug.Log("CampaignManager.SaveCampaign(): Saving empty campaign");
            _currentCampaign = new Campaign();
        }

        SavePrefabs();
        SaveTiles();
        _currentCampaign.Save();
    }

    public void SaveCampaignAs(string filePath)
    {
        if (_currentCampaign == null)
        {
            Debug.Log("CampaignManager.SaveCampaign(): Saving empty campaign");
            _currentCampaign = new Campaign();
        }

        SavePrefabs();
        SaveTiles();
        _currentCampaign.SaveAs(filePath);
    }
    #endregion

    #region Save Utils
    public void SavePrefabs()
    {
        _currentCampaign.prefabs = new List<PrefabModel>();
        foreach (Transform child in _prefabManager.prefabContainer)
        {
            PrefabModel newPrefab = new PrefabModel();

            newPrefab.position = child.position;
            newPrefab.rotation = child.rotation.eulerAngles;
            newPrefab.scale = child.localScale;

            _currentCampaign.prefabs.Add(newPrefab);
        }
    }

    private void SaveTiles()
    {
        _currentCampaign.tiles = new List<TileModel>();

        foreach(Vector3Int position in _terrainManager.tileMap.cellBounds.allPositionsWithin)
        {
            if (!_terrainManager.tileMap.HasTile(position))
                continue;

            TileModel tile = new TileModel();
            tile.tileName = _terrainManager.tileMap.GetTile(position).name;
            tile.tilePosition = position;

            _currentCampaign.tiles.Add(tile);
        }
    }
    #endregion

    #region Load Utils
    private void LoadPrefabs()
    {
        foreach (PrefabModel campaignPrefab in _currentCampaign.prefabs)
        {
            _prefabManager.LoadPrefabFromSave(campaignPrefab);
        }
    }

    private void LoadTiles()
    {
        foreach(TileModel tile in _currentCampaign.tiles)
        {
            Tile tileToSet = _terrainManager.tiles.Where(t => t.name == tile.tileName).FirstOrDefault();
            _terrainManager.tileMap.SetTile(tile.tilePosition, tileToSet);
        }
    }

    private void ClearOldData()
    {
        // Clear all of the current tiles
        _terrainManager.tileMap.ClearAllTiles();

        // Delete all of the current prefabs
        foreach (Transform child in _prefabManager.prefabContainer)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region RecentCampaigns
    private void LoadRecentCampaigns()
    {
        string filePath = Path.Combine(Application.persistentDataPath, Constants.recentCampainsFileName);
        if (File.Exists(filePath))
        {
            try
            {
                string fileContents = File.ReadAllText(filePath);
                recentCampaigns = JsonUtility.FromJson<RecentCampaigns>(fileContents);
                OnRecentCampaignsUpdated.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            recentCampaigns = new RecentCampaigns();
            recentCampaigns.filePaths = new List<string>();
        }
    }

    private void AddToRecentCampaigns(string path)
    {
        if (!recentCampaigns.filePaths.Contains(path))
        {
            recentCampaigns.filePaths.Add(path);
            OnRecentCampaignsUpdated.Invoke();
        }

        SaveRecentCampaigns();
    }

    private void SaveRecentCampaigns()
    {
        string filePath = Path.Combine(Application.persistentDataPath, Constants.recentCampainsFileName);
        string fileContents = JsonUtility.ToJson(recentCampaigns);
        try
        {
            File.WriteAllText(filePath, fileContents);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n{e.StackTrace}");
        }
    }
    #endregion
}
