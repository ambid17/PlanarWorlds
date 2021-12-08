using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageTabButton : ButtonBase
{
    [SerializeField]
    private Image innerImage;

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
