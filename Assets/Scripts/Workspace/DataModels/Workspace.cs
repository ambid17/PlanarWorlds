using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Workspace
{
    [SerializeField]
    public string workspaceName;
    [SerializeField]
    public List<WorkspacePrefab> prefabs;

    public Workspace()
    {
        workspaceName = "testWorkspace";
        prefabs = new List<WorkspacePrefab>();
    }

    public void UpdateName(string newName)
    {
        // If the file already exists, make sure to rename it
        string currentFilePath = FilePathUtil.GetSaveFilePath(workspaceName);
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

        workspaceName = newName;
        Save();
    }

    #region Serialization
    public void Save()
    {
        string filePath = FilePathUtil.GetSaveFilePath(workspaceName);
        string fileContents = JsonUtility.ToJson(this);
        try
        {
            File.WriteAllText(filePath, fileContents);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n{e.StackTrace}");
        }
        Debug.Log($"Finished Saving workspace {workspaceName} to {filePath}");
    }

    public static Workspace LoadFromName(string workspaceName)
    {
        Workspace workspace = new Workspace();

        string filePath = FilePathUtil.GetSaveFilePath(workspaceName);
        if (File.Exists(filePath))
        {
            try
            {
                string fileContents = File.ReadAllText(filePath);
                workspace = JsonUtility.FromJson<Workspace>(fileContents);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }

        return workspace;
    }
    #endregion
}
