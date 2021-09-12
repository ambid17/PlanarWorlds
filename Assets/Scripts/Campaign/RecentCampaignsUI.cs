using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecentCampaignsUI : MonoBehaviour
{
    public GameObject recentCampaignUiPrefab;

    private CampaignManager _campaignManager;
    private void Awake()
    {
        _campaignManager = CampaignManager.GetInstance();
    }

    private void OnEnable()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        PopulateRecentCampaigns();
    }

    private void PopulateRecentCampaigns()
    {
        foreach(string filePath in _campaignManager.recentCampaigns.filePaths)
        {
            GameObject instance = Instantiate(recentCampaignUiPrefab, transform);
            TMP_Text text = instance.GetComponentInChildren<TMP_Text>();

            text.text = filePath;
        }
    }
}
