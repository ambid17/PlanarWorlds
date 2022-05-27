using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;

public class HotkeyMenu : MonoBehaviour
{
    public TooltipUI tooltip;

    [SerializeField] private GameObject _hotkeyItemPrefab;
    [SerializeField] private Transform _listViewContainer;


    void Start()
    {
        PopulateList();
    }



    void PopulateList()
    {
        foreach (Hotkey hotkey in HotKeyManager.Instance.GetHotKeys())
        {
            GameObject hotkeyGO = Instantiate(_hotkeyItemPrefab, _listViewContainer);
            HotKeyItem hotKeyItem = hotkeyGO.GetComponent<HotKeyItem>();
            hotKeyItem.Init(hotkey, tooltip);
        }
    }
    
}
