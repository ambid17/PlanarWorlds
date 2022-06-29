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
        openButton.onClick.AddListener(OnLoad);
        saveButton.onClick.AddListener(OnSave);
        saveAsButton.onClick.AddListener(OnSaveAs);
        openFolderButton.onClick.AddListener(OnOpenFolder);

        FileBrowser.SetFilters(false, new FileBrowser.Filter("Campaigns", ".plane"));

        _defaultPath = Path.Combine(Application.persistentDataPath, "Campaigns");

        PopulateRecentCampaigns();
        recentCampaignsDropdown.onValueChanged.AddListener(OnOpenRecent);
    }

    private void OnEnable()
    {
        PopulateRecentCampaigns();
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

        var dirInfo = new DirectoryInfo(FilePathUtil.GetSaveFolder());
        var latestFiles = dirInfo.GetFiles("*.plane").OrderByDescending(file => file.LastWriteTime).Take(5);
        foreach (FileInfo file in latestFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FullName);

            string itemText = $"{fileName} \n{file.FullName}";
            options.Add(itemText);
        }

        recentCampaignsDropdown.AddOptions(options);
    }

    #region New
    public void OnNew()
    {
        _uiManager.isFileBrowserOpen = true;
        
        // This could be done a few ways:
        // X - Callbacks/events when yes/no is clicked
        // - Using coroutines to wait for a result value on the modal to be set
        // - Using async
        // - Moving the logic to a NewButton that handles all of this there
        // - State Machine
        if (_campaignManager.CampaignNeedsSave())
        {
            tempSaveModal.Show((isYes) =>
            {
                if (isYes)
                {
                    FileBrowser.ShowSaveDialog((paths) => { New(paths); }, null, FileBrowser.PickMode.Files, false, _defaultPath, GetTempFileName(), "Save New", "Save New");
                }
            });
        }
        
        
    }
    
    private void New(string[] paths)
    {
        string path = paths[0];
        
        if (File.Exists(path))
        {
            overwriteSaveModal.Show(path);
        }
        else
        {
            _campaignManager.NewCampaign(path);
            UpdateCampaignNameText(path);
        }

        _uiManager.isFileBrowserOpen = false;
        gameObject.SetActive(false);
    }
    #endregion

    #region Load
    public void OnLoad()
    {
        _uiManager.isFileBrowserOpen = true;
        FileBrowser.ShowLoadDialog((paths) => { Load(paths); }, null, FileBrowser.PickMode.Files, false, _defaultPath, string.Empty, "Open", "Open");
    }
    
    private void Load(string[] paths)
    {
        string filePath = paths[0];

        AskToSave();
        
        _campaignManager.LoadCampaign(filePath);
        UpdateCampaignNameText(filePath);
        _uiManager.isFileBrowserOpen = false;
        gameObject.SetActive(false);
    }

    private void AskToSave()
    {
        if (_campaignManager.CampaignNeedsSave())
        {
            tempSaveModal.Show();
        }
    }
    #endregion

    #region Save
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
    
    private void SaveAs(string[] paths)
    {
        string path = paths[0];

        if (File.Exists(path))
        {
            overwriteSaveModal.Show(path);
        }
        else
        {
            _campaignManager.SaveCampaignAs(path);
        }

        UpdateCampaignNameText(path);
        _uiManager.isFileBrowserOpen = false;
        gameObject.SetActive(false);
    }
    #endregion

    #region Open Data Folder
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
    #endregion

    #region Open Recent
    private void OnOpenRecent(int index)
    {
        if (index == 0)
            return; // This is the empty item, it is only here as a placeholder

        // This is a shitty hack. I was too lazy to learn how to attach a script to the dropdown items
        // Instead, I truncate the text field, and hide the file path on the next line
        // It.... works i guess
        string filePath = recentCampaignsDropdown.options[index].text.Split('\n').Last();
        
        Load(new []{filePath});
    }
    #endregion

    #region Modals
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
    }

    private void UpdateCampaignNameText(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        CampaignNameUpdated?.Invoke(fileName);
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
    #endregion
}
