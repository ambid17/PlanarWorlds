using System;
using System.IO;
using UnityEngine;

public class CampaignManager : StaticMonoBehaviour<CampaignManager>
{
    public RecentCampaigns recentCampaigns;
    public event Action OnRecentCampaignsUpdated;

    private Campaign _currentCampaign;
    public Campaign CurrentCampaign { get => _currentCampaign; }

    private PrefabManager _prefabManager;
    private TerrainManager _terrainManager;

    private void Start()
    {
        _prefabManager = PrefabManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();

        LoadRecentCampaigns();
    }

    private void OnApplicationQuit()
    {
        // if (!CurrentDataIsSaved())
        // {
        //     SaveTempCampaign();
        // }
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

        AddToRecentCampaigns(path);
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
            AddToRecentCampaigns(path);
        }
    }

    public void SaveCampaign()
    {
        _prefabManager.PopulateCampaign(_currentCampaign);
        _terrainManager.PopulateCampaign(_currentCampaign);
        _currentCampaign.Save();
        AddToRecentCampaigns(_currentCampaign.filePath);
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

    public bool CurrentDataIsSaved()
    {
        bool isSaved = true;

        if(_currentCampaign == null)
        {
            if(_prefabManager.prefabContainer.childCount > 0)
            {
                isSaved = false;
            }

            if(_terrainManager.TerrainNeedsSaved())
            {
                isSaved = false;
            }
        }
        else
        {
            SaveCampaign();
        }

        return isSaved;
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
                OnRecentCampaignsUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            recentCampaigns = new RecentCampaigns();
        }
    }

    private void AddToRecentCampaigns(string path)
    {
        recentCampaigns.Add(path);
        OnRecentCampaignsUpdated?.Invoke();
    }
    #endregion
}
