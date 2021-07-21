using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkspaceHeader : MonoBehaviour
{
    [SerializeField]
    TMP_InputField workspaceNameInputField;
    [SerializeField]
    Button loadDocumentButton;
    [SerializeField]
    Button saveWorkspaceButton;

    WorkspaceManager workspaceManager;


    void Start()
    {
        workspaceManager = WorkspaceManager.GetInstance();

        workspaceNameInputField.onEndEdit.AddListener(OnWorkspaceNameChanged);
        loadDocumentButton.onClick.AddListener(OnLoadButtonClicked);
        saveWorkspaceButton.onClick.AddListener(SaveWorkspace);
    }

    public void OnWorkspaceNameChanged(string newName)
    {
        workspaceManager.UpdateWorkspaceName(newName);
    }

    public void OnLoadButtonClicked()
    {
        //string filePath = FileBrowser.Instance.OpenSingleFile("pdf");
        //workspaceManager.ImportDocument(filePath);
    }

    void SaveWorkspace()
    {
        workspaceManager.SaveWorkspace();
    }

    public void SetWorkspaceNameInput(string workspaceName)
    {
        workspaceNameInputField.text = workspaceName;
    }
}
