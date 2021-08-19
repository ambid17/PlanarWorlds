using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;
using RTG;

public class CampaignManager : StaticMonoBehaviour<CampaignManager>
{
    public GameObject documentPrefab;
    public Transform documentContainer;

    private Campaign _campaign;
    private CampaignUiHeader _campaignHeader;

    protected override void Awake()
    {
        base.Awake();
        _campaignHeader = GetComponentInChildren<CampaignUiHeader>(true);
    }

    private void Start()
    {
    }


    #region Campaign Utils
    public void CreateCampaign()
    {
        _campaign = new Campaign();
    }

    public void UpdateCampaignName(string name)
    {
        _campaign.UpdateName(name);
    }

    public void LoadCampaign(string name)
    {
        _campaign = Campaign.LoadFromName(name);

        _campaignHeader.SetCampaignNameInput(_campaign.campaignName);

        InitializePrefabs();
    }

    public void SaveCampaign()
    {
        PopulatePrefabs();
        _campaign.Save();
    }

    public void LoadPrefabFromSave(SerializedPrefab prefab)
    {
        //PrefabItem prefabItem = GetPrefabItem(prefab);
        //GameObject instance = Instantiate(prefabItem.prefab, prefabParent);
        //instance.layer = Constants.PrefabLayer;
        //instance.transform.position = prefab.position;
        //instance.transform.rotation = Quaternion.Euler(prefab.rotation);
        //instance.transform.localScale = prefab.scale;
    }
    #endregion


    #region Prefab Utils
    private void InitializePrefabs()
    {
        foreach (SerializedPrefab campaignPrefab in _campaign.prefabs)
        {
            //_prefabManager.LoadPrefabFromSave(campaignPrefab);
        }
    }

    public void PopulatePrefabs()
    {
        //_campaign.prefabs = new List<SerializedPrefab>();
        //foreach (Transform child in _prefabManager.prefabParent)
        //{
        //    SerializedPrefab newPrefab = new SerializedPrefab();

        //    newPrefab.position = child.position;
        //    newPrefab.rotation = child.rotation.eulerAngles;
        //    newPrefab.scale = child.localScale;
        //    _campaign.prefabs.Add(newPrefab);
        //}
    }
    #endregion


}
