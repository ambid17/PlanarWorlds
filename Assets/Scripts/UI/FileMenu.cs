using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;
using Michsky.UI.ModernUIPack;
using System.Linq;

public class FileMenu : MonoBehaviour
{
    public Button newButton;
    public Button openButton;
    public CustomDropdown recentCampaignsDropdown;
    public Button saveButton;
    public Button saveAsButton;

    private CampaignManager _campaignManager;

    private string _defaultPath;
    private string _defaultFileName;


    void Start()
    {
        _campaignManager = CampaignManager.GetInstance();

        newButton.onClick.AddListener(OnNew);
        openButton.onClick.AddListener(OnOpen);
        saveButton.onClick.AddListener(OnSave);
        saveAsButton.onClick.AddListener(OnSaveAs);

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Campaigns", ".json"));

        _defaultPath = Path.Combine(Application.persistentDataPath, "Campaigns");
        _defaultFileName = "campaign.json";

        PopulateRecentCampaigns();
    }

    private void PopulateRecentCampaigns()
    {
        foreach (string filePath in _campaignManager.recentCampaigns.filePaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            string itemText = $"{fileName} \n {filePath}";
            recentCampaignsDropdown.CreateNewItemFast(fileName, null);
        }

        recentCampaignsDropdown.SetupDropdown();
        recentCampaignsDropdown.dropdownEvent.AddListener(OnOpenRecent);
    }

    private void OnNew()
    {
        StartCoroutine(ShowSaveDialogCoroutine());
    }

    private void OnOpen()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private void OnOpenRecent(int index)
    {
        string filePath = recentCampaignsDropdown.selectedText.text.Split('\n').Last();
        _campaignManager.LoadCampaign(filePath);
    }

    private void OnSave()
    {
        _campaignManager.SaveCampaign();
    }

    private void OnSaveAs()
    {
        StartCoroutine(ShowSaveDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, _defaultPath, _defaultFileName, "Load", "Load");

        Debug.Log($"FileBrowser succes: {FileBrowser.Success}");

        if (FileBrowser.Success)
        {
            _campaignManager.LoadCampaign(FileBrowser.Result[0]);
        }
    }

    IEnumerator ShowSaveDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, _defaultPath, _defaultFileName, "Save", "Save");

        Debug.Log($"FileBrowser succes: {FileBrowser.Success}");

        if (FileBrowser.Success)
        {
            _campaignManager.SaveCampaignAs(FileBrowser.Result[0]);
        }
    }
}
