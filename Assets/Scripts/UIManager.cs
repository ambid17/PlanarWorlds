using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : StaticMonoBehaviour<UIManager>
{
    public GameObject PrefabCanvas;
    public GameObject InspectorCanvas;
    public GameObject HierarchyCanvas;
    public GameObject CampaignCanvas;

    public bool isEditingValues;

    public bool campaignWindowShouldBeActive;
    public bool hierarchyWindowShouldBeActive;
    public bool inspectorWindowShouldBeActive;
    public bool prefabWindowShouldBeActive;

    void Start()
    {
        isEditingValues = false;

        campaignWindowShouldBeActive = false;
        hierarchyWindowShouldBeActive = false;
        inspectorWindowShouldBeActive = true;
        prefabWindowShouldBeActive = true;

        UpdateActiveWindows();
    }

    void Update()
    {
        
    }

    private void UpdateActiveWindows()
    {
        CampaignCanvas.SetActive(campaignWindowShouldBeActive);
        HierarchyCanvas.SetActive(hierarchyWindowShouldBeActive);
        InspectorCanvas.SetActive(inspectorWindowShouldBeActive);
        PrefabCanvas.SetActive(prefabWindowShouldBeActive);
    }
}
