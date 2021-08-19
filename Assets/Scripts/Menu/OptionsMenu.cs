using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTG;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public Transform scrollViewContent;
    public GameObject hotKeyPrefab;

    string keyToRebind = null;
    Dictionary<string, TMP_Text> buttonKeyCodeTexts;

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
        if (keyToRebind != null)
        {
            if (Input.anyKeyDown)
            {
                // Loop through all possible keys and see if it was pressed down
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        _hotKeyManager.SetButtonForKey(keyToRebind, keyCode);
                        buttonKeyCodeTexts[keyToRebind].text = keyCode.ToString();
                        keyToRebind = null;
                        break;
                    }
                }
            }
        }
    }

    void ReloadUI()
    {
        CleanScrollView();
        PopulateHotkeys();
    }

    void StartRebindFor(string keyName)
    {
        Debug.Log("StartRebindFor: " + keyName);

        keyToRebind = keyName;
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
        Dictionary<string, KeyCode> keys = _hotKeyManager.GetHotKeys();
        Dictionary<string, string> tooltips = _hotKeyManager.GetTooltips();

        buttonKeyCodeTexts = new Dictionary<string, TMP_Text>();
        foreach (string hotkey in keys.Keys)
        {
            GameObject newItem = Instantiate(hotKeyPrefab);
            newItem.transform.SetParent(scrollViewContent, false);

            HotKeyItem item = newItem.GetComponentInChildren<HotKeyItem>();
            item.SetItemText(hotkey);
            item.SetButtonText(keys[hotkey].ToString());
            item.tooltip.SetTooltipText(tooltips[hotkey]);
            item.itemButton.onClick.AddListener(() => StartRebindFor(hotkey.ToString()));

            buttonKeyCodeTexts.Add(hotkey, item.GetButtonText());
        }
    }

    public void SetDefaults()
    {
        Debug.Log("set hotkey defaults");
        _hotKeyManager.SetDefaults();
        onSetDefaults?.Invoke();
        ReloadUI();
    }
}
