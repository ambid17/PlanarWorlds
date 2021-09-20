using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Campaign
{
    [SerializeField]
    public List<PrefabModel> prefabs;
    [SerializeField]
    public List<TileModel> tiles;
    [SerializeField]
    public string filePath;

    public Campaign()
    {
        prefabs = new List<PrefabModel>();
        tiles = new List<TileModel>();
    }

    #region Serialization
    public void Save()
    {
        string fileContents = JsonUtility.ToJson(this);
        try
        {
            File.WriteAllText(filePath, fileContents);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n{e.StackTrace}");
        }
        Debug.Log($"Finished Saving campaign to {filePath}");
    }

    public static Campaign LoadFromPath(string filePath)
    {
        Campaign campaign = new Campaign();

        Debug.Log($"Loading campaign from: {filePath}");
        if (File.Exists(filePath))
        {
            try
            {
                string fileContents = File.ReadAllText(filePath);
                campaign = JsonUtility.FromJson<Campaign>(fileContents);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                campaign = null;
            }
        }
        else
        {
            Debug.LogError($"Campaign.LoadFromName(): No file exists at: {filePath}");
            campaign = null;
        }

        return campaign;
    }

    // Find the next unused campaign name
    // i.e. : tempCampaign0, tempCampaign1, etc
    public string SetTempFilePath()
    {
        string baseFilePath = Path.Combine(Application.persistentDataPath, "Campaigns");

        int fileCounter = 1;
        string fullFilePath = Path.Combine(baseFilePath, $"tempCampaign{fileCounter}.plane");

        while (File.Exists(fullFilePath))
        {
            fileCounter++;
            fullFilePath = Path.Combine(baseFilePath, $"tempCampaign{fileCounter}.plane");
        }

        filePath = fullFilePath;
        return fullFilePath;
    }
    #endregion
}
