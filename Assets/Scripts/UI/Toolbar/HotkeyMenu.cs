using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public class HotkeyMenu : MonoBehaviour
{
    [SerializeField] private GameObject _hotkeyItemPrefab;
    [SerializeField] private Transform _listViewContainer;

    [SerializeField] private ImageTabButton _generalButton;
    [SerializeField] private ImageTabButton _cameraButton;
    [SerializeField] private ImageTabButton _terrainButton;
    [SerializeField] private ImageTabButton _prefabButton;
    [SerializeField] private ImageTabButton _encounterButton;

    private List<HotKeyItem> _hotKeyItems;
    
    void Start()
    {
        PopulateList();
        SetupButtons();
        
        // We will default to the general tab
        OnFilterClicked(HotkeyFilterType.General);
        UIManager.OnEditModeChanged += EditModeChanged;
    }

    void PopulateList()
    {
        _hotKeyItems = new List<HotKeyItem>();
        
        foreach (Hotkey hotkey in HotKeyManager.Instance.GetHotKeys())
        {
            GameObject hotkeyGO = Instantiate(_hotkeyItemPrefab, _listViewContainer);
            HotKeyItem hotKeyItem = hotkeyGO.GetComponent<HotKeyItem>();
            hotKeyItem.Init(hotkey);
            _hotKeyItems.Add(hotKeyItem);
        }
    }

    void SetupButtons()
    {
        _generalButton.Setup(() =>OnFilterClicked(HotkeyFilterType.General), "General Hotkeys");
        _cameraButton.Setup(() =>OnFilterClicked(HotkeyFilterType.Camera), "Camera Hotkeys");
        _terrainButton.Setup(() =>OnFilterClicked(HotkeyFilterType.Terrain), "Terrain Hotkeys");
        _prefabButton.Setup(() =>OnFilterClicked(HotkeyFilterType.Prefab), "Prefab Hotkeys");
        _encounterButton.Setup(() =>OnFilterClicked(HotkeyFilterType.Encounter), "Encounter Hotkeys");
    }

    void OnFilterClicked(HotkeyFilterType buttonType)
    {
        ToggleButtonHighlights(buttonType);
        FilterHotkeys(buttonType);
    }

    void ToggleButtonHighlights(HotkeyFilterType buttonType)
    {
        _generalButton.Toggle(buttonType == HotkeyFilterType.General);
        _cameraButton.Toggle(buttonType == HotkeyFilterType.Camera);
        _terrainButton.Toggle(buttonType == HotkeyFilterType.Terrain);
        _prefabButton.Toggle(buttonType == HotkeyFilterType.Prefab);
        _encounterButton.Toggle(buttonType == HotkeyFilterType.Encounter);
    }
    void FilterHotkeys(HotkeyFilterType buttonType)
    {
        foreach (var hotKeyItem in _hotKeyItems)
        {
            hotKeyItem.gameObject.SetActive(hotKeyItem.hotkey.filterType == buttonType);
        }
    }

    void EditModeChanged(EditMode editMode)
    {
        switch (editMode)
        {
            case EditMode.Terrain:
                OnFilterClicked(HotkeyFilterType.Terrain);
                break;
            case EditMode.Prefab:
                OnFilterClicked(HotkeyFilterType.Prefab);
                break;
            case EditMode.Encounter:
                OnFilterClicked(HotkeyFilterType.Encounter);
                break;
        }
    }
}
