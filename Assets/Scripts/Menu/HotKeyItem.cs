using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HotKeyItem : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private Button itemButton;
    [SerializeField] private TMP_Text buttonText;
    
    private TooltipContent _tooltip;
    private string _tooltipDescription;

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
        _tooltip.description = _tooltipDescription;
    }

    public TMP_Text GetButtonText()
    {
        return buttonText;
    }

    /// <summary>
    /// Used by OptionsMenu to send a callback for binding the hotkey
    /// </summary>
    public void Init(Hotkey hotkey, TooltipContent tooltip, Action<HotKeyName> action)
    {
        Init(hotkey, tooltip);
        
        itemButton.onClick.AddListener(() =>
        {
            action(hotkey.hotkeyName);
        });
    }
    
    public void Init(Hotkey hotkey, TooltipContent tooltip)
    {
        itemText.text = hotkey.readableName;
        buttonText.text = hotkey.savedKeyCode.ToString();
        _tooltipDescription = hotkey.tooltip;
        _tooltip = tooltip;
    }

}
