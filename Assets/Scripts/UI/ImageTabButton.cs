using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ImageTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color activeTextColor = new Color(0.4f, 0.4f, 1, 1);
    public Color hoverTextColor = new Color(0.4f, 0.4f, 1, 1);
    public Color inactiveTextColor = Color.white;

    private Button button;
    private Image buttonImage;
    private Image innerImage;

    private bool isSelected;


    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        innerImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void Setup(Sprite sprite, Action callback)
    {
        innerImage.sprite = sprite;
        button.onClick.AddListener(() => callback());
        button.onClick.AddListener(Select);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            buttonImage.color = hoverTextColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            buttonImage.color = inactiveTextColor;
        }
    }
}
