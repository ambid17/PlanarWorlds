using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private GameObject content;
    private RectTransform _myRectTransform;
    
    void Start()
    {
        _myRectTransform = GetComponent<RectTransform>();
        DisableTooltip();
    }

    void Update()
    {
        float halfWidth = _myRectTransform.rect.width / 2;
        float halfHeight = _myRectTransform.rect.height / 2;
        
        Vector3 followPosition = Input.mousePosition + new Vector3(halfWidth, halfHeight);
        
        followPosition.x = Mathf.Clamp(followPosition.x, halfWidth, Screen.width - halfWidth);
        followPosition.y = Mathf.Clamp(followPosition.y, halfHeight, Screen.height - halfHeight);
        
        transform.position = followPosition;
    }

    public void SetTooltip(string text)
    {
        content.SetActive(true);
        tooltipText.text = text;
    }

    public void DisableTooltip()
    {
        content.SetActive(false);
    }
}
