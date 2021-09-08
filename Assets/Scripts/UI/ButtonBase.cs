﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color activeTextColor = new Color(0.4f, 0.4f, 1, 1);
    public Color hoverTextColor = new Color(0.4f, 0.4f, 1, 1);
    public Color inactiveTextColor = Color.white;

    public Button button;
    public Image buttonImage;
    public TMP_Text buttonText;

    public bool isSelected;

    void Awake()
    {
        buttonText = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    public virtual void SetupAction(Action callback)
    {
        button.onClick.AddListener(() => callback());
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