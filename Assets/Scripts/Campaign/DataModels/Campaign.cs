using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Campaign
{
    [SerializeField]
    public string campaignName;
    [SerializeField]
    public List<PrefabModel> prefabs;
    [SerializeField]
    public List<TileModel> tiles;
    [SerializeField]
    public string filePath;

    public Campaign()
    {
        campaignName = "testCampaign";
        prefabs = new List<PrefabModel>();
        tiles = new List<TileModel>();
    }

    public void UpdateName(string newName)
    {
        // If the file already exists, make sure to rename it
        string currentFilePath = FilePathUtil.GetSaveFilePath(campaignName);
        string newFilePath = FilePathUtil.GetSaveFilePath(newName);
        if (File.Exists(currentFilePath))
        {
            try
            {
                File.Move(currentFilePath, newFilePath);

            }catch(IOException e)
            {
                Debug.LogWarning($"Cannot rename file \n{e.Message}");
            }
        }

        campaignName = newName;
        Save();
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
        Debug.Log($"Finished Saving campaign {campaignName} to {filePath}");
    }

    public void SaveAs(string filePath)
    {
        this.filePath = filePath;

        Save();
    }

    public static Campaign LoadFromPath(string filePath)
    {
        Campaign campaign = new Campaign();

        string qualifiedFilePath = filePath.Replace("/", "\\\\");
        File.OpenRead(qualifiedFilePath);
        if (File.Exists(qualifiedFilePath))
        {
            try
            {
                string fileContents = File.ReadAllText(qualifiedFilePath);
                campaign = JsonUtility.FromJson<Campaign>(fileContents);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"Campaign.LoadFromName(): No file exists at: {qualifiedFilePath}");
        }

        return campaign;
    }
    #endregion
}
