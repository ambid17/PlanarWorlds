using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class FilePathUtil : MonoBehaviour
{
    public static string campaignFolderName = "Campaigns";
    public static string hotkeyFileName = "hotkey.json";
    
    public static string GetSaveFolder()
    {
        return Path.Combine(Application.persistentDataPath, "Campaigns");
    }

    public static string GetHotkeyFilePath()
    {
        return Path.Combine(Application.persistentDataPath, hotkeyFileName);
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
}
