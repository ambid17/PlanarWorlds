using System;

public class ToggleButton : ButtonBase
{
    public override void SetupAction(Action callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback());
        button.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        isSelected = !isSelected;
        buttonImage.color = isSelected ? activeTextColor : inactiveTextColor;
        buttonText.color = isSelected ? activeTextColor : inactiveTextColor;
    }
}
