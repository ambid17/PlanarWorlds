using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContextualHotKeysUI : MonoBehaviour
{
    public Transform scrollViewContent;
    public GameObject hotKeyPrefab;

    Dictionary<string, TMP_Text> buttonKeyCodeTexts;

    private HotKeyManager _hotKeyManager;

    private HashSet<string> terrainHotKeys = new HashSet<string> { HotkeyConstants.SelectPosition, HotkeyConstants.SelectRotation, HotkeyConstants.SelectScale };

    private HashSet<string> propHotKeys = new HashSet<string> { HotkeyConstants.DeletePrefab, HotkeyConstants.Focus, HotkeyConstants.Duplicate };


    private void Awake()
    {
        _hotKeyManager = HotKeyManager.GetInstance();

        UIManager.OnEditModeChanged += PopulateHotkeysBasedOnGameMode;
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


        buttonKeyCodeTexts = new Dictionary<string, TMP_Text>();
        foreach (string hotkey in keys.Keys)
        {
            if (hotkey == HotkeyConstants.ModifierKey)
                return;

            if (editModeKeys.Contains(hotkey) == false)
                continue;

            GameObject newItem = Instantiate(hotKeyPrefab);
            newItem.transform.SetParent(scrollViewContent, false);

            HotKeyItem item = newItem.GetComponentInChildren<HotKeyItem>();
            item.SetItemText(hotkey);
            item.SetButtonText(keys[hotkey].ToString());
            item.tooltip.SetTooltipText(tooltips[hotkey]);

            buttonKeyCodeTexts.Add(hotkey, item.GetButtonText());
        }
    }

    private void CleanScrollView()
    {
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
    }
}
