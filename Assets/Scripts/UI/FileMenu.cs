using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileMenu : MonoBehaviour
{
    public Button newButton;
    public Button openButton;
    public RectTransform openRecentTransform;
    public Button saveButton;
    public Button saveAsButton;
    public GameObject openRecentMenu;

    private CampaignManager _campaignManager;

    void Start()
    {
        _campaignManager = CampaignManager.GetInstance();

        newButton.onClick.AddListener(NewButtonClicked);
        openButton.onClick.AddListener(OpenButtonClicked);
        saveButton.onClick.AddListener(SaveButtonClicked);
        saveAsButton.onClick.AddListener(SaveAsButtonClicked);
    }

    private void Update()
    {
        Vector2 mousePosition = openRecentTransform.InverseTransformPoint(Input.mousePosition);
        bool mouseIsOnToolip = openRecentTransform.rect.Contains(mousePosition);
        openRecentMenu.SetActive(mouseIsOnToolip);
    }

    private void NewButtonClicked()
    {
        // TODO: open file browser
        _campaignManager.NewCampaign();
    }

    private void OpenButtonClicked()
    {
        // TODO: open file browser
        _campaignManager.LoadCampaign("");
    }

    private void SaveButtonClicked()
    {
        _campaignManager.SaveCampaign();
    }

    private void SaveAsButtonClicked()
    {
        // TODO: open file browser
        _campaignManager.SaveCampaign();
    }

}
