using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class FilePathUtil : MonoBehaviour
{
    public static string campaignFolderName = "Campaigns";

    public static string GetCampaignFolder(string campaignName)
    {
        string campaignParentFolder = Path.Combine(Application.persistentDataPath, campaignFolderName);
        EnsureDirectoryExists(campaignParentFolder);
        return campaignParentFolder;
    }

    public static string GetSaveFilePath(string campaignName)
    {
        string campaignParentFolder = Path.Combine(Application.persistentDataPath, campaignFolderName);
        EnsureDirectoryExists(campaignParentFolder);
        string saveFilePath = Path.Combine(campaignParentFolder, $"{campaignName}.plane");
        return saveFilePath;
    }

    public static string GetSaveFolder()
    {
        return Path.Combine(Application.persistentDataPath, "Campaigns");
    }

    public static List<string> GetCampaignNames()
    {
        string campaignParentFolder = Path.Combine(Application.persistentDataPath, campaignFolderName);
        EnsureDirectoryExists(campaignParentFolder);

        List<string> filePaths = Directory.EnumerateFiles(campaignParentFolder, "*.plane", SearchOption.AllDirectories).ToList();

        List<string> fileNames = new List<string>();
        foreach(string path in filePaths)
        {
            fileNames.Add(Path.GetFileNameWithoutExtension(path));
        }
        
        return fileNames;
    }

    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not create directory:\n{e.Message}\n{e.StackTrace}");
            }
        }
    }

    public static string GetFileNameFromPath(string filePath)
    {
        int indexOfLastSlash = filePath.LastIndexOf('\\') + 1;
        string fileName = filePath.Substring(indexOfLastSlash).Replace(".pdf", "");
        return fileName;
    }
}
