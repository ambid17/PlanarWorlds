using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotKeyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private Button itemButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TooltipItem tooltip;

    public delegate void ButtonPressDelegate();

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        tooltip.gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector2 mousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
        bool mouseIsOnToolip = rectTransform.rect.Contains(mousePosition);
        tooltip.gameObject.SetActive(mouseIsOnToolip);
    }

    public TMP_Text GetButtonText()
    {
        return buttonText;
    }

    public void Init(Hotkey hotkey, Action<HotKeyName> action)
    {
        itemText.text = hotkey.readableName;
        buttonText.text = hotkey.savedKeyCode.ToString();
        tooltip.SetTooltipText(hotkey.tooltip);
        
        itemButton.onClick.AddListener(() =>
        {
            action(hotkey.hotkeyName);
        });
    }
    
    public void Init(Hotkey hotkey)
    {
        itemText.text = hotkey.readableName;
        buttonText.text = hotkey.savedKeyCode.ToString();
        tooltip.SetTooltipText(hotkey.tooltip);
    }

}
