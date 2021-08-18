using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditMode
{
    Map, Prop, Encounter
}

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

    private EditMode _currentEditmode;

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

    public void SetEditMode(EditMode editMode)
    {
        _currentEditmode = editMode;

        campaignWindowShouldBeActive = editMode == EditMode.Map || editMode == EditMode.Prop;
        hierarchyWindowShouldBeActive = editMode == EditMode.Prop;
        inspectorWindowShouldBeActive = true;
        prefabWindowShouldBeActive = editMode == EditMode.Prop;

        UpdateActiveWindows();
    }

    private void UpdateActiveWindows()
    {
        CampaignCanvas.SetActive(campaignWindowShouldBeActive);
        HierarchyCanvas.SetActive(hierarchyWindowShouldBeActive);
        InspectorCanvas.SetActive(inspectorWindowShouldBeActive);
        PrefabCanvas.SetActive(prefabWindowShouldBeActive);
    }
}
