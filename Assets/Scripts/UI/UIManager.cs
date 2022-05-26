using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;

public enum EditMode
{
    Terrain, Prefab, Encounter
}

public class UIManager : StaticMonoBehaviour<UIManager>
{
    public GameObject PrefabCanvas;
    public GameObject InspectorCanvas;
    public GameObject TerrainInspectorCanvas;
    public GameObject EncounterCanvas;

    public bool isEditingValues;
    public bool isFileBrowserOpen;
    public bool isPaused;

    public bool UserCantInput { get => isEditingValues || isFileBrowserOpen || isPaused; }

    public bool inspectorWindowShouldBeActive;
    public bool prefabWindowShouldBeActive;
    public bool terrainInspectorWindowShouldBeActive;
    public bool encounterWindowShouldBeActive;

    public static event Action<EditMode> OnEditModeChanged;

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

        inspectorWindowShouldBeActive = editMode == EditMode.Prefab;
        prefabWindowShouldBeActive = editMode == EditMode.Prefab;
        terrainInspectorWindowShouldBeActive = editMode == EditMode.Terrain;
        encounterWindowShouldBeActive = editMode == EditMode.Encounter;

        //tell the UI scripts that are listening to clean their UI and affects before their scripts are toggled off
        OnEditModeChanged.Invoke(editMode);

        UpdateActiveWindows();
    }

    private void UpdateActiveWindows()
    {
        InspectorCanvas.SetActive(inspectorWindowShouldBeActive);
        PrefabCanvas.SetActive(prefabWindowShouldBeActive);
        TerrainInspectorCanvas.SetActive(terrainInspectorWindowShouldBeActive);
        EncounterCanvas.SetActive(encounterWindowShouldBeActive);
    }
}
