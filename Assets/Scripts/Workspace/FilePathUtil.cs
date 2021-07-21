using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class FilePathUtil : MonoBehaviour
{
    public static string workspaceFolderName = "Workspaces";

    public static string GetWorkspaceFolder(string workspaceName)
    {
        string workspaceParentFolder = Path.Combine(Application.persistentDataPath, workspaceFolderName);
        EnsureDirectoryExists(workspaceParentFolder);
        return workspaceParentFolder;
    }

    public static string GetSaveFilePath(string workspaceName)
    {
        string workspaceParentFolder = Path.Combine(Application.persistentDataPath, workspaceFolderName);
        EnsureDirectoryExists(workspaceParentFolder);
        string saveFilePath = Path.Combine(workspaceParentFolder, $"{workspaceName}.json");
        return saveFilePath;
    }

    public static List<string> GetWorkspaceNames()
    {
        string workspaceParentFolder = Path.Combine(Application.persistentDataPath, workspaceFolderName);
        EnsureDirectoryExists(workspaceParentFolder);

        List<string> filePaths = Directory.EnumerateFiles(workspaceParentFolder, "*.json", SearchOption.AllDirectories).ToList();

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
