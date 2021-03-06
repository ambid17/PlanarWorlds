using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using System.Diagnostics;

public enum OverwriteType
{
    Save, New
}

public class FileMenu : MonoBehaviour
{
    public Button newButton;
    public Button openButton;
    public TMP_Dropdown recentCampaignsDropdown;
    public Button saveButton;
    public Button saveAsButton;
    public Button openFolderButton;
    public TempSaveModal tempSaveModal;
    public OverwriteSaveModal overwriteSaveModal;

    public event Action<string> CampaignNameUpdated;

    private CampaignManager _campaignManager;
    private UIManager _uiManager;

    private string _defaultPath;

    private RectTransform _myRectTransform;



    void Start()
    {
        _campaignManager = CampaignManager.GetInstance();
        _uiManager = UIManager.GetInstance();
        _myRectTransform = GetComponent<RectTransform>();

        newButton.onClick.AddListener(OnNew);
        openButton.onClick.AddListener(OnOpen);
        saveButton.onClick.AddListener(OnSave);
        saveAsButton.onClick.AddListener(OnSaveAs);
        openFolderButton.onClick.AddListener(OnOpenFolder);

        FileBrowser.SetFilters(false, new FileBrowser.Filter("Campaigns", ".plane"));

        _defaultPath = Path.Combine(Application.persistentDataPath, "Campaigns");

        PopulateRecentCampaigns();
        _campaignManager.OnRecentCampaignsUpdated += PopulateRecentCampaigns;
        overwriteSaveModal.WillOverwrite += Overwrite;
        overwriteSaveModal.CancelOverwrite += DontOverwrite;
        recentCampaignsDropdown.onValueChanged.AddListener(OnOpenRecent);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = _myRectTransform.InverseTransformPoint(Input.mousePosition);
            bool mouseIsOnMenu = _myRectTransform.rect.Contains(mousePosition);

            if (!mouseIsOnMenu)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void PopulateRecentCampaigns()
    {
        recentCampaignsDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add(string.Empty);

        foreach (CampaignAccess access in _campaignManager.recentCampaigns.accesses)
        {
            string fileName = Path.GetFileNameWithoutExtension(access.filePath);

            string itemText = $"{fileName} \n{access.filePath}";
            options.Add(itemText);
        }

        recentCampaignsDropdown.AddOptions(options);
    }

    public void OnNew()
    {
        _uiManager.isFileBrowserOpen = true;
        FileBrowser.ShowSaveDialog((paths) => { New(paths); }, null, FileBrowser.PickMode.Files, false, _defaultPath, GetTempFileName(), "Save New", "Save New");
    }

    public void OnOpen()
    {
        _uiManager.isFileBrowserOpen = true;
        FileBrowser.ShowLoadDialog((paths) => { Load(paths); }, null, FileBrowser.PickMode.Files, false, _defaultPath, string.Empty, "Open", "Open");
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

        string fileName = Path.GetFileNameWithoutExtension(filePath);
        CampaignNameUpdated.Invoke(fileName);

        gameObject.SetActive(false);
    }

    public void OnSave()
    {
        if (_campaignManager.CurrentCampaign != null)
            _campaignManager.SaveCampaign();
        else
            OnSaveAs();
    }

    public void OnSaveAs()
    {
        _uiManager.isFileBrowserOpen = true;
        FileBrowser.ShowSaveDialog((paths) => { SaveAs(paths); }, null, FileBrowser.PickMode.Files, false, _defaultPath, GetTempFileName(), "Save As", "Save As");
    }

    public void OnOpenFolder()
    {
#if UNITY_STANDALONE_WIN
        string fileBrowserLocation = FilePathUtil.GetSaveFolder();
        fileBrowserLocation = fileBrowserLocation.Replace("/","\\");
        try
        {
            Process fileBrowserProcess = new Process();
            fileBrowserProcess.StartInfo = new ProcessStartInfo("explorer.exe", fileBrowserLocation);
            fileBrowserProcess.Start();
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogError($"Cannot open folder: \n{fileBrowserLocation} \nException\n{e.Message}");
        }
        #endif
    }

    private void InformOfSave()
    {
        string filePath = _campaignManager.SaveTempCampaign();
        tempSaveModal.Show(filePath);
    }

    private void Load(string[] paths)
    {
        if (!_campaignManager.CurrentDataIsSaved())
        {
            InformOfSave();
        }

        _campaignManager.LoadCampaign(paths[0]);

        string fileName = Path.GetFileNameWithoutExtension(paths[0]);
        CampaignNameUpdated.Invoke(fileName);

        _uiManager.isFileBrowserOpen = false;
        gameObject.SetActive(false);
    }

    

    private void SaveAs(string[] paths)
    {
        string path = paths[0];

        if (File.Exists(path))
        {
            overwriteSaveModal.Show(path, OverwriteType.Save);
        }
        else
        {
            _campaignManager.SaveCampaignAs(paths[0]);

            string fileName = Path.GetFileNameWithoutExtension(paths[0]);
            CampaignNameUpdated.Invoke(fileName);
        }

        _uiManager.isFileBrowserOpen = false;
        gameObject.SetActive(false);
    }

    private void New(string[] paths)
    {
        if (!_campaignManager.CurrentDataIsSaved())
        {
            InformOfSave();
        }

        string path = paths[0];

        if (File.Exists(path))
        {
            overwriteSaveModal.Show(path, OverwriteType.New);
        }
        else
        {
            _campaignManager.NewCampaign(path);

            string fileName = Path.GetFileNameWithoutExtension(paths[0]);
            CampaignNameUpdated.Invoke(fileName);
        }

        _uiManager.isFileBrowserOpen = false;
        gameObject.SetActive(false);
    }

    

    private void Overwrite(string path, OverwriteType type)
    {
        if(type == OverwriteType.New)
        {
            _campaignManager.NewCampaign(path);
        }
        else if(type == OverwriteType.Save)
        {
            _campaignManager.SaveCampaignAs(path);
        }

        string fileName = Path.GetFileNameWithoutExtension(path);
        CampaignNameUpdated.Invoke(fileName);
    }

    private void DontOverwrite(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        CampaignNameUpdated.Invoke(fileName);
    }


    public string GetTempFileName()
    {
        string baseFilePath = Path.Combine(Application.persistentDataPath, "Campaigns");

        int fileCounter = 1;
        string fullFilePath = Path.Combine(baseFilePath, $"Campaign{fileCounter}.plane");

        while (File.Exists(fullFilePath))
        {
            fileCounter++;
            fullFilePath = Path.Combine(baseFilePath, $"Campaign{fileCounter}.plane");
        }

        return $"Campaign{fileCounter}.plane";
    }
}
