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
    public TempSaveModal tempSaveModal;

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
        if (!_campaignManager.CurrentDataIsSaved())
        {
            InformOfSave();
        }

        StartCoroutine(ShowNewDialogCoroutine());
    }

    private void OnOpen()
    {
        if (!_campaignManager.CurrentDataIsSaved())
        {
            InformOfSave();
        }

        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private void OnOpenRecent(int index)
    {
        if (index == 0)
            return; // This is the empty item, it is only here as a placeholder

        if (!_campaignManager.CurrentDataIsSaved())
        {
            InformOfSave();
        }

        // This is a shitty hack. I was too lazy to learn how to attach a script to the dropdown items
        // Instead, I truncate the text field, and hide the file path on the next line
        // It.... works i guess
        string filePath = recentCampaignsDropdown.options[index].text.Split('\n').Last();
        _campaignManager.LoadCampaign(filePath);
    }

    private void OnSave()
    {
        if (_campaignManager.CurrentCampaign != null)
            _campaignManager.SaveCampaign();
        else
            OnSaveAs();
    }

    private void OnSaveAs()
    {
        StartCoroutine(ShowSaveDialogCoroutine());
    }

    private void InformOfSave()
    {
        string filePath = _campaignManager.SaveTempCampaign();
        tempSaveModal.Show(filePath);
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
