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
    public List<SerializedPrefab> prefabs;

    public Campaign()
    {
        campaignName = "testCampaign";
        prefabs = new List<SerializedPrefab>();
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
        string filePath = FilePathUtil.GetSaveFilePath(campaignName);
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

    public static Campaign LoadFromName(string campaignName)
    {
        Campaign campaign = new Campaign();

        string filePath = FilePathUtil.GetSaveFilePath(campaignName);
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
            }
        }

        return campaign;
    }
    #endregion
}
