using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class FileMenu : MonoBehaviour
{
    public Button newButton;
    public Button openButton;
    public TMP_Dropdown recentCampaignsDropdown;
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
        _campaignManager.OnRecentCampaignsUpdated += PopulateRecentCampaigns;
        recentCampaignsDropdown.onValueChanged.AddListener(OnOpenRecent);
    }

    private void PopulateRecentCampaigns()
    {
        recentCampaignsDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add(string.Empty);

        foreach (string filePath in _campaignManager.recentCampaigns.filePaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            string itemText = $"{fileName} \n{filePath}";
            options.Add(itemText);
        }

        recentCampaignsDropdown.AddOptions(options);
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
        string filePath = recentCampaignsDropdown.options[index].text.Split('\n').Last();
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

    IEnumerator ShowNewDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, _defaultPath, _defaultFileName, "Save", "Save");

        Debug.Log($"FileBrowser succes: {FileBrowser.Success}");

        if (FileBrowser.Success)
        {
            _campaignManager.NewCampaign(FileBrowser.Result[0]);
        }
    }
}
