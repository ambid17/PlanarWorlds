using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;
using RTG;

// This class manages the workspace and it's data
public class WorkspaceManager : StaticMonoBehaviour<WorkspaceManager>
{
    public GameObject documentPrefab;
    public Transform documentContainer;

    private Workspace _workspace;
    private WorkspaceHeader _workspaceHeader;
    private PrefabManager _prefabManager;

    protected override void Awake()
    {
        base.Awake();
        _workspaceHeader = GetComponentInChildren<WorkspaceHeader>(true);
    }

    private void Start()
    {
        _prefabManager = PrefabManager.GetInstance();
    }


    #region Workspace Utils
    public void CreateWorkspace()
    {
        _workspace = new Workspace();
    }

    public void UpdateWorkspaceName(string name)
    {
        _workspace.UpdateName(name);
    }

    public void LoadWorkspace(string name)
    {
        _workspace = Workspace.LoadFromName(name);

        _workspaceHeader.SetWorkspaceNameInput(_workspace.workspaceName);

        InitializePrefabs();
    }

    public void SaveWorkspace()
    {
        PopulatePrefabs();
        _workspace.Save();
    }
    #endregion


    #region Prefab Utils
    private void InitializePrefabs()
    {
        foreach (WorkspacePrefab workspacePrefab in _workspace.prefabs)
        {
            _prefabManager.LoadPrefabFromSave(workspacePrefab);
        }
    }

    public void PopulatePrefabs()
    {
        _workspace.prefabs = new List<WorkspacePrefab>();
        foreach (Transform child in _prefabManager.prefabParent)
        {
            WorkspacePrefab newPrefab = new WorkspacePrefab();

            PrefabItemInstance prefabInstance = child.GetComponent<PrefabItemInstance>();

            newPrefab.color = prefabInstance.color;
            newPrefab.position = child.position;
            newPrefab.rotation = child.rotation.eulerAngles;
            newPrefab.scale = child.localScale;
            newPrefab.prefabType = prefabInstance.prefabItem.prefabType;
            _workspace.prefabs.Add(newPrefab);
        }
    }
    #endregion


}
