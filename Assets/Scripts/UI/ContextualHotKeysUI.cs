using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Based on the game mode, we show the corresponding hotkeys in the hotkey window
// When the player changes game mode, the hotkey are updated accordingly, even when the keys are rebound

public class ContextualHotKeysUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    [SerializeField] private OptionsMenu optionsMenu;

    public Transform scrollViewContent;
    public GameObject hotKeyPrefab;

    private HotKeyManager _hotKeyManager;

    private HashSet<string> terrainHotKeys = new HashSet<string> { HotkeyConstants.SelectPosition, HotkeyConstants.SelectRotation, HotkeyConstants.SelectScale };

    private HashSet<string> propHotKeys = new HashSet<string> { HotkeyConstants.DeletePrefab, HotkeyConstants.Focus, HotkeyConstants.Duplicate };

    private Dictionary<string, HotKeyItem> displayedHotkeys = new Dictionary<string, HotKeyItem>();


    private void Awake()
    {
        _hotKeyManager = HotKeyManager.GetInstance();

        UIManager.OnEditModeChanged += PopulateHotkeysBasedOnGameMode;
        optionsMenu.RebindKeyEvent += UpdateHotKeyItem;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ToggleUI);
    }

    private void PopulateHotkeysBasedOnGameMode(EditMode editMode)
    {
        CleanScrollView();

        Dictionary<string, KeyCode> keys = _hotKeyManager.GetHotKeys();
        Dictionary<string, string> tooltips = _hotKeyManager.GetTooltips();

        HashSet<string> editModeKeys;

        if (editMode == EditMode.Terrain)
            editModeKeys = terrainHotKeys;
        else
            editModeKeys = propHotKeys;

        foreach (string hotkeyName in keys.Keys)
        {
            if (!editModeKeys.Contains(hotkeyName))
                continue;

            GameObject newItem = Instantiate(hotKeyPrefab, scrollViewContent);

            HotKeyItem item = newItem.GetComponentInChildren<HotKeyItem>();
            item.SetItemText(hotkeyName);
            item.SetButtonText(keys[hotkeyName].ToString());
            item.tooltip.SetTooltipText(tooltips[hotkeyName]);

            displayedHotkeys.Add(hotkeyName,item);
        }
    }

    private void UpdateHotKeyItem(string hotkeyName)
    {
        HotKeyItem hotKey = displayedHotkeys[hotkeyName];

        displayedHotkeys[hotkeyName].SetButtonText(hotKey.ToString());
    }

    private void CleanScrollView()
    {
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        displayedHotkeys.Clear();
    }

    public void ToggleUI()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
