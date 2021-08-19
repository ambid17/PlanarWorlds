using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotKeyItem : MonoBehaviour
{
    public TMP_Text itemText;
    public Button itemButton;
    public TMP_Text buttonText;
    public TooltipItem tooltip;

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

    public void SetItemText(string text)
    {
        itemText.text = text;
    }

    public void SetButtonText(string text)
    {
        buttonText.text = text;
    }

    public TMP_Text GetButtonText()
    {
        return buttonText;
    }
}
