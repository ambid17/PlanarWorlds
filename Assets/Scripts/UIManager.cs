using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditMode
{
    Terrain, Prefab, Encounter
}

public class UIManager : StaticMonoBehaviour<UIManager>
{
    public GameObject PrefabCanvas;
    public GameObject InspectorCanvas;
    public GameObject HierarchyCanvas;
    public GameObject CampaignCanvas;
    public GameObject TerrainInspectorCanvas;

    public bool isEditingValues;

    public bool campaignWindowShouldBeActive;
    public bool hierarchyWindowShouldBeActive;
    public bool inspectorWindowShouldBeActive;
    public bool prefabWindowShouldBeActive;
    public bool terrainInspectorWindowShouldBeActive;

    private EditMode _currentEditmode;
    public EditMode EditMode
    {
        get => _currentEditmode;
    }

    void Start()
    {
        isEditingValues = false;

        SetEditMode(EditMode.Terrain);
    }

    void Update()
    {
        
    }

    public void SetEditMode(EditMode editMode)
    {
        _currentEditmode = editMode;

        campaignWindowShouldBeActive = editMode == EditMode.Terrain || editMode == EditMode.Prefab;
        hierarchyWindowShouldBeActive = editMode == EditMode.Prefab;
        inspectorWindowShouldBeActive = editMode == EditMode.Prefab;
        prefabWindowShouldBeActive = editMode == EditMode.Prefab;
        terrainInspectorWindowShouldBeActive = editMode == EditMode.Terrain;

        // Here is where you have to tell the UI scripts that are listening to clean their UI and affects before their scripts are toggled off

        UpdateActiveWindows();
    }

    private void UpdateActiveWindows()
    {
        CampaignCanvas.SetActive(campaignWindowShouldBeActive);
        HierarchyCanvas.SetActive(hierarchyWindowShouldBeActive);
        InspectorCanvas.SetActive(inspectorWindowShouldBeActive);
        PrefabCanvas.SetActive(prefabWindowShouldBeActive);
        TerrainInspectorCanvas.SetActive(terrainInspectorWindowShouldBeActive);
    }
}
