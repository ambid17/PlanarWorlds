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

    void Start()
    {
        fileButton.onClick.AddListener(FileButtonClicked);
        fileMenu.gameObject.SetActive(false);
        fileMenu.CampaignNameUpdated += UpdateCampaignName;
    }

    void Update()
    {
        
    }

    private void FileButtonClicked()
    {
        fileMenu.gameObject.SetActive(true);
    }

    public void UpdateCampaignName(string newName)
    {
        campaignNameText.text = newName;
    }
}
