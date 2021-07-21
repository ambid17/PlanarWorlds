using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class WorkspaceSelector : MonoBehaviour
{
    [SerializeField]
    CustomDropdown workspaceDropdown;
    [SerializeField]
    Button newWorkspaceButton;
    [SerializeField]
    GameObject workspaceEditor;
    [SerializeField]
    GameObject workspaceLoader;
    [SerializeField]
    Sprite dropdownItemIcon;

    WorkspaceManager workspaceManager;



    void Start()
    {
        workspaceManager = WorkspaceManager.GetInstance();
        newWorkspaceButton.onClick.AddListener(OnNewWorkspaceClicked);
        workspaceDropdown.dropdownEvent.AddListener(OnOldWorkspaceSelected);
        SetupDropdown();
    }

    public void SetupDropdown()
    {
        List<string> workspaceNames = FilePathUtil.GetWorkspaceNames();

        foreach (string workspace in workspaceNames)
        {
            workspaceDropdown.CreateNewItemFast(workspace, dropdownItemIcon);
        }
        workspaceDropdown.SetupDropdown();
    }


    public void OnNewWorkspaceClicked()
    {
        workspaceEditor.SetActive(true);
        workspaceLoader.SetActive(false);
        workspaceManager.CreateWorkspace();
    }

    public void OnOldWorkspaceSelected(int index)
    {
        workspaceEditor.SetActive(true);
        workspaceLoader.SetActive(false);
        workspaceManager.LoadWorkspace(workspaceDropdown.selectedText.text);
    }

}
