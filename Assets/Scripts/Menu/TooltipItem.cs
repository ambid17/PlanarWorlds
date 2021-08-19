using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text tooltipText;

    public void SetTooltipText(string text)
    {
        tooltipText.text = text;
    }
}
