using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private Vector3 offset;

    private RectTransform _myRectTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        _myRectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Input.mousePosition + new Vector3(_myRectTransform.rect.width/2, _myRectTransform.rect.height/2);
    }

    public void SetTooltip(string text)
    {
        tooltipText.text = text;
    }
}
