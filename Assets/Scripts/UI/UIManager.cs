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
    public TooltipUI tooltipUI;
    public GameObject PrefabCanvas;
    public GameObject InspectorCanvas;
    public GameObject TerrainInspectorCanvas;
    public GameObject EncounterCanvas;

    public bool isEditingValues;
    public bool isFileBrowserOpen;
    public bool isPaused;

    public bool UserCantInput { get => isEditingValues || isFileBrowserOpen || isPaused; }

    private bool _inspectorWindowShouldBeActive;
    private bool _prefabWindowShouldBeActive;
    private bool _terrainInspectorWindowShouldBeActive;
    private bool _encounterWindowShouldBeActive;

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

        _inspectorWindowShouldBeActive = editMode == EditMode.Prefab;
        _prefabWindowShouldBeActive = editMode == EditMode.Prefab;
        _terrainInspectorWindowShouldBeActive = editMode == EditMode.Terrain;
        _encounterWindowShouldBeActive = editMode == EditMode.Encounter;

        //tell the UI scripts that are listening to clean their UI and affects before their scripts are toggled off
        OnEditModeChanged?.Invoke(editMode);

        UpdateActiveWindows();
    }

    private void UpdateActiveWindows()
    {
        InspectorCanvas.SetActive(_inspectorWindowShouldBeActive);
        PrefabCanvas.SetActive(_prefabWindowShouldBeActive);
        TerrainInspectorCanvas.SetActive(_terrainInspectorWindowShouldBeActive);
        EncounterCanvas.SetActive(_encounterWindowShouldBeActive);
    }
}
