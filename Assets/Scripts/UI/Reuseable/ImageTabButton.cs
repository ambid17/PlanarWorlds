using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageTabButton : ButtonBase, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image innerImage;

    private string _tooltipText;

    public override void Awake()
    {
        Button myButton = GetComponent<Button>();
        if (myButton)
        {
            button = myButton;
        }

        Image myImage = GetComponent<Image>();
        if (myImage)
        {
            buttonImage = myImage;
        }

        Image myInnerImage = transform.GetChild(0).GetComponent<Image>();
        if (myInnerImage)
        {
            innerImage = myInnerImage;
        }
    }

    public void Setup(Sprite sprite, Action callback)
    {
        innerImage.sprite = sprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback());
        button.onClick.AddListener(Select);
    }

    public void Setup(Action callback, string tooltipText)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback());
        button.onClick.AddListener(Select);
        _tooltipText = tooltipText;
    }

    public void Select()
    {
        isSelected = true;
        buttonImage.color = activeTextColor;
    }

    public void Unselect()
    {
        isSelected = false;
        buttonImage.color = inactiveTextColor;
    }

    public void Toggle(bool shouldBeSelected)
    {
        if (shouldBeSelected)
        {
            Select();
        }
        else
        {
            Unselect();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.tooltipUI.SetTooltip(_tooltipText);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.tooltipUI.DisableTooltip();
    }
}
