﻿using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HotKeyItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private Button itemButton;
    [SerializeField] private TMP_Text buttonText;

    public Hotkey hotkey;

    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.tooltipUI.SetTooltip(hotkey.tooltip);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.tooltipUI.DisableTooltip();
    }

    public TMP_Text GetButtonText()
    {
        return buttonText;
    }

    /// <summary>
    /// Used by OptionsMenu to send a callback for binding the hotkey
    /// </summary>
    public void Init(Hotkey hotkey, Action<HotKeyName> action)
    {
        Init(hotkey);
        
        itemButton.onClick.AddListener(() =>
        {
            action(hotkey.hotkeyName);
        });
    }
    
    public void Init(Hotkey hotkey)
    {
        this.hotkey = hotkey;
        itemText.text = hotkey.readableName;
        buttonText.text = hotkey.savedKeyCode.ToString();
    }

}
