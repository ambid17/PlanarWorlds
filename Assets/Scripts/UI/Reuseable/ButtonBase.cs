using System;
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

    public virtual void Awake()
    {
        GetComponents();
    }

    public void GetComponents()
    {
        buttonText = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    public virtual void SetupAction(Action callback)
    {
        // So we can call this before the object is enabled (before Awake)
        if (button == null 
            || buttonImage == null 
            || buttonText == null)
        {
            GetComponents();
        }

        button.onClick.RemoveAllListeners();
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