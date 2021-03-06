using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolbarUI : MonoBehaviour
{
    public Button fileButton;
    public FileMenu fileMenu;
    public TMP_Text campaignNameText;

    public Button terrainButton;
    public TerrainMenu terrainMenu;
    
    public Button hotkeyButton;
    public HotkeyMenu hotkeyMenu;
    
    private UIManager _uiManager;

    void Start()
    {
        _uiManager = UIManager.GetInstance();
        UIManager.OnEditModeChanged += EditModeChanged;

        fileButton.onClick.AddListener(FileButtonClicked);
        fileMenu.gameObject.SetActive(false);
        fileMenu.CampaignNameUpdated += UpdateCampaignName;

        terrainButton.onClick.AddListener(TerrainButtonClicked);
        terrainMenu.gameObject.SetActive(false);
        
        hotkeyButton.onClick.AddListener(HotkeyButtonClicked);
        hotkeyMenu.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_uiManager.isPaused || _uiManager.isFileBrowserOpen)
            return;

        CheckFileMenuHotkeys();
    }

    private void FileButtonClicked()
    {
        fileMenu.gameObject.SetActive(true);
    }

    private void TerrainButtonClicked()
    {
        terrainMenu.gameObject.SetActive(true);
    }
    
    private void HotkeyButtonClicked()
    {
        hotkeyMenu.gameObject.SetActive(true);
    }

    public void UpdateCampaignName(string newName)
    {
        campaignNameText.text = newName;
    }

    private void CheckFileMenuHotkeys()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                fileMenu.OnSave();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                fileMenu.OnNew();
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                fileMenu.OnOpen();
            }

            if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
            {
                fileMenu.OnSaveAs();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                fileMenu.OnOpenFolder();
            }
        }
        
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        terrainButton.gameObject.SetActive(newEditMode == EditMode.Terrain);
    }
}
