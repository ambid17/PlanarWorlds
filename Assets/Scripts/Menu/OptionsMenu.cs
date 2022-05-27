using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;
using RTG;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public Transform scrollViewContent;
    public GameObject hotKeyPrefab;
    public TooltipUI tooltip;

    HotKeyName keyToRebind;
    private bool isBindingKey = false;
    Dictionary<HotKeyName, TMP_Text> buttonKeyCodeTexts;

    public static event Action onSetDefaults;

    private HotKeyManager _hotKeyManager;

    private void Awake()
    {
        _hotKeyManager = HotKeyManager.GetInstance();
    }

    void Start()
    {
        ReloadUI();
    }

    private void Update()
    {
        // If we aren't binding a key, and there's nothing pressed, don't try to bind
        if (!isBindingKey || !Input.anyKeyDown)
        {
            return;
        }

        TryBindKey();
    }

    void TryBindKey()
    {
        // Loop through all possible keys and see if it was pressed down
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                _hotKeyManager.SetButtonForKey(keyToRebind, keyCode);
                buttonKeyCodeTexts[keyToRebind].text = keyCode.ToString();
                isBindingKey = false;
                break;
            }
        }
    }

    void ReloadUI()
    {
        CleanScrollView();
        PopulateHotkeys();
    }

    private void CleanScrollView()
    {
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateHotkeys()
    {
        buttonKeyCodeTexts = new Dictionary<HotKeyName, TMP_Text>();

        Hotkey[] keys = _hotKeyManager.GetHotKeys();
        foreach (Hotkey hotkey in keys)
        {
            if (hotkey.hotkeyName == HotKeyName.ControlModifier)
                return;

            GameObject newItem = Instantiate(hotKeyPrefab, scrollViewContent);
            HotKeyItem item = newItem.GetComponent<HotKeyItem>();
            item.Init(hotkey, tooltip, StartRebindFor);
            buttonKeyCodeTexts.Add(hotkey.hotkeyName, item.GetButtonText());
        }
    }
    
    void StartRebindFor(HotKeyName keyName)
    {
        Debug.Log($"StartRebindFor: {keyName}");
        isBindingKey = true;
        keyToRebind = keyName;
    }

    public void SetDefaults()
    {
        Debug.Log("Setting hotkeys to defaults");
        _hotKeyManager.SetDefaults();
        ReloadUI();
    }
}
