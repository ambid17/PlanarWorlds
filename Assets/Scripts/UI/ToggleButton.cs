using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color activeTextColor = new Color(0.4f, 0.4f, 1, 1);
    public Color hoverTextColor = new Color(0.4f, 0.4f, 1, 1);
    public Color inactiveTextColor = Color.white;

    private TMP_Text buttonText;
    private Button button;
    private Image buttonImage;

    private bool isSelected;


    void Start()
    {
        buttonText = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    public void SetupAction(Action callback)
    {
        button.onClick.AddListener(() => callback());
        button.onClick.AddListener(Select);
    }

    public void Select()
    {
        isSelected = true;
        buttonImage.color = activeTextColor;
        buttonText.color = activeTextColor;
    }

    public void Unselect()
    {
        isSelected = false;
        buttonImage.color = inactiveTextColor;
        buttonText.color = inactiveTextColor;
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
