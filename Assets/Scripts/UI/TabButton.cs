using System;
using TMPro;
using UnityEngine.UI;

public class TabButton : ButtonBase
{
    public override void SetupAction(Action callback)
    {
        base.SetupAction(callback);
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
}
