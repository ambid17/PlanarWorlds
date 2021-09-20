using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolbarUI : MonoBehaviour
{
    public Button fileButton;
    public FileMenu fileMenu;
    public TMP_Text campaignNameText;

    private UIManager _uiManager;

    void Start()
    {
        _uiManager = UIManager.GetInstance();

        fileButton.onClick.AddListener(FileButtonClicked);
        fileMenu.gameObject.SetActive(false);
        fileMenu.CampaignNameUpdated += UpdateCampaignName;
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
        }
        
    }
}
