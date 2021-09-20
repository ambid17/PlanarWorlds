using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

[Serializable]
public class RecentCampaigns
{
    [SerializeField]
    public List<CampaignAccess> accesses;

    public RecentCampaigns()
    {
        accesses = new List<CampaignAccess>();
    }

    public void Add(string path)
    {
        if (path == string.Empty)
            return;

        path = path.Replace("\\", "/");

        bool pathExists = accesses.Where(access => access.filePath == path).Any();
        if (!pathExists)
        {
            CampaignAccess newAccess = new CampaignAccess()
            {
                filePath = path,
                lastAccessed = DateTime.Now.Ticks
            };

            if (accesses.Count > 5)
            {
                var lastAccess = accesses.Min(a => a.lastAccessed);
                var accessToRemove = accesses.First(a => a.lastAccessed == lastAccess);
                accesses.Remove(accessToRemove);
            }

            accesses.Add(newAccess);
        }
        else
        {
            var accessToUpdate = accesses.First(a => a.filePath == path);
            accessToUpdate.lastAccessed = DateTime.Now.Ticks;
        }

        SaveRecentCampaigns();
    }

    private void SaveRecentCampaigns()
    {
        string filePath = Path.Combine(Application.persistentDataPath, Constants.recentCampainsFileName);
        string fileContents = JsonUtility.ToJson(this);
        try
        {
            File.WriteAllText(filePath, fileContents);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n{e.StackTrace}");
        }
    }
}

[Serializable]
public class CampaignAccess
{
    [SerializeField]
    public string filePath;
    [SerializeField]
    public long lastAccessed;
}
