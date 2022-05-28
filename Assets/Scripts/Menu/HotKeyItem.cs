using System;
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
        string readableKeyCode = GetHotkeyText();
        buttonText.text = readableKeyCode;
    }

    /// <summary>
    /// If the tooltip is being used for hotkeys, we want to rename them from Unity's "KeyCode" enum to a better readable name 
    /// </summary>
    private string GetHotkeyText()
    {
        string text = String.Empty;

        if (hotkey.modifier != KeyCode.None)
        {
            text = $"{hotkey.modifier}+";
        }

        if (hotkey.secondModifier != KeyCode.None)
        {
            text += $"{hotkey.secondModifier}+";
        }

        text += $"{hotkey.savedKeyCode}";
        
        if (text.Contains("Mouse0"))
        {
            text = text.Replace("Mouse0", "LMB");
        }
        
        if (text.Contains("Mouse1"))
        {
            text = text.Replace("Mouse1", "RMB");
        }
        
        if (text.Contains("Mouse2"))
        {
            text = text.Replace("Mouse2", "MMB");
        }
        
        if (text.Contains("LeftControl"))
        {
            text = text.Replace("LeftControl", "CTRL");
        }
        
        if (text.Contains("LeftShift"))
        {
            text = text.Replace("LeftShift", "SHFT");
        }
        
        if (text.Contains("Backspace"))
        {
            text = text.Replace("Backspace", "DELETE");
        }

        return text;
    }
}
