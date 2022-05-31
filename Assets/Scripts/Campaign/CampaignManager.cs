using System;
using System.IO;
using UnityEngine;

public class CampaignManager : StaticMonoBehaviour<CampaignManager>
{
    private Campaign _currentCampaign;
    public Campaign CurrentCampaign { get => _currentCampaign; }

    private PrefabManager _prefabManager;
    private TerrainManager _terrainManager;
    
    
    private bool _isSaved;
    /// <summary>
    /// This is defaulted to true so that we don't save a temp campaign every time we exit.
    /// Everything that edits the campaign is responsible for calling CampaignModified() to inform we need to save
    /// </summary>
    public bool IsSaved => _isSaved;

    private void Start()
    {
        _prefabManager = PrefabManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();
        _isSaved = true;
    }

    private void OnApplicationQuit()
    {
        if(_currentCampaign == null && !_isSaved)
        {
            SaveTempCampaign();
        }
        else
        {
            SaveCampaign();
        }
    }

    #region Campaign Utils
    public void NewCampaign(string path)
    {
        if (_currentCampaign != null)
        {
            SaveCampaign();
        }

        ClearOldData();
        _currentCampaign = new Campaign()
        {
            filePath = path
        };
    }

    public void LoadCampaign(string path)
    {
        if (_currentCampaign != null)
        {
            SaveCampaign();
        }

        _currentCampaign = Campaign.LoadFromPath(path);
        if(_currentCampaign != null)
        {
            ClearOldData();
            _prefabManager.LoadCampaign(_currentCampaign);
            _terrainManager.LoadCampaign(_currentCampaign);
        }
    }

    public void CampaignModified()
    {
        _isSaved = false;
    }
    
    public void SaveCampaign()
    {
        _prefabManager.PopulateCampaign(_currentCampaign);
        _terrainManager.PopulateCampaign(_currentCampaign);
        _currentCampaign.Save();

        _isSaved = true;
    }

    public void SaveCampaignAs(string filePath)
    {
        if (_currentCampaign == null)
        {
            _currentCampaign = new Campaign()
            {
                filePath = filePath
            };
        }
        else
        {
            _currentCampaign.filePath = filePath;
        }
            

        SaveCampaign();
    }

    public string SaveTempCampaign()
    {
        _currentCampaign = new Campaign();
        string filePath = _currentCampaign.SetTempFilePath();
        SaveCampaign();

        return filePath;
    }
    #endregion

    #region Load Utils
    private void ClearOldData()
    {
        _terrainManager.ClearAllTerrain();
        _prefabManager.Clear();
    }
    #endregion
}
