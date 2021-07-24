using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampaignUiHeader : MonoBehaviour
{
    [SerializeField]
    TMP_InputField campaignNameInputField;
    [SerializeField]
    Button loadDocumentButton;
    [SerializeField]
    Button saveCampaignButton;

    CampaignManager campaignManager;


    void Start()
    {
        campaignManager = CampaignManager.GetInstance();

        campaignNameInputField.onEndEdit.AddListener(OnCampaignNameChanged);
        loadDocumentButton.onClick.AddListener(OnLoadButtonClicked);
        saveCampaignButton.onClick.AddListener(SaveCampaign);
    }

    public void OnCampaignNameChanged(string newName)
    {
        campaignManager.UpdateCampaignName(newName);
    }

    public void OnLoadButtonClicked()
    {
        //string filePath = FileBrowser.Instance.OpenSingleFile("pdf");
        //campaignManager.ImportDocument(filePath);
    }

    void SaveCampaign()
    {
        campaignManager.SaveCampaign();
    }

    public void SetCampaignNameInput(string campaignName)
    {
        campaignNameInputField.text = campaignName;
    }
}
