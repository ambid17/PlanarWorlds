using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class CampaignSelector : MonoBehaviour
{
    [SerializeField]
    CustomDropdown campaignDropdown;
    [SerializeField]
    Button newCampaignButton;
    [SerializeField]
    GameObject campaignEditor;
    [SerializeField]
    GameObject campaignLoader;
    [SerializeField]
    Sprite dropdownItemIcon;

    CampaignManager campaignManager;

    void Start()
    {
        campaignManager = CampaignManager.GetInstance();
        newCampaignButton.onClick.AddListener(OnNewcampaignClicked);
        campaignDropdown.dropdownEvent.AddListener(OnOldcampaignSelected);
        SetupDropdown();
    }

    public void SetupDropdown()
    {
        List<string> campaignNames = FilePathUtil.GetCampaignNames();

        foreach (string campaignName in campaignNames)
        {
            campaignDropdown.CreateNewItemFast(campaignName, dropdownItemIcon);
        }
        campaignDropdown.SetupDropdown();
    }


    public void OnNewcampaignClicked()
    {
        campaignEditor.SetActive(true);
        campaignLoader.SetActive(false);
        campaignManager.CreateCampaign();
    }

    public void OnOldcampaignSelected(int index)
    {
        campaignEditor.SetActive(true);
        campaignLoader.SetActive(false);
        campaignManager.LoadCampaign(campaignDropdown.selectedText.text);
    }

}
