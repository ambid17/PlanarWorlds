using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageTabButton : ButtonBase
{
    [SerializeField]
    private Image innerImage;

    public override void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        innerImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void Setup(Sprite sprite, Action callback)
    {
        innerImage.sprite = sprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback());
        button.onClick.AddListener(Select);
    }

    public void Setup(Action callback)
    {
        button.onClick.RemoveAllListeners();
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
}
