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

public class CampaignManager : StaticMonoBehaviour<CampaignManager>
{
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
    }

    #region Campaign Utils
    public void NewCampaign()
    {
        if (_currentCampaign != null)
        {
            SaveCampaign();
        }
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
        LoadPrefabs();
        LoadTiles();
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

    public void LoadPrefabFromSave(PrefabModel prefab)
    {
        //PrefabItem prefabItem = GetPrefabItem(prefab);
        //GameObject instance = Instantiate(prefabItem.prefab, prefabParent);
        //instance.layer = Constants.PrefabLayer;
        //instance.transform.position = prefab.position;
        //instance.transform.rotation = Quaternion.Euler(prefab.rotation);
        //instance.transform.localScale = prefab.scale;
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
        // Delete all of the current prefabs
        foreach(Transform child in _prefabManager.prefabContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (PrefabModel campaignPrefab in _currentCampaign.prefabs)
        {
            _prefabManager.LoadPrefabFromSave(campaignPrefab);
        }
    }

    private void LoadTiles()
    {
        // Clear all of the current tiles
        _terrainManager.tileMap.ClearAllTiles();

        foreach(TileModel tile in _currentCampaign.tiles)
        {
            Tile tileToSet = _terrainManager.tiles.Where(t => t.name == tile.tileName).FirstOrDefault();
            _terrainManager.tileMap.SetTile(tile.tilePosition, tileToSet);
        }
    }
    #endregion
}
