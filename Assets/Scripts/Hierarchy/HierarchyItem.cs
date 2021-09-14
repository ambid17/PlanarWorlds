using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HierarchyItem : MonoBehaviour
{
    public Image backgroundImage;
    public TMP_InputField objectNameInput;
    public RectTransform content;
    public GameObject toggleArrow;

    private Color defaultColor = new Color(0.17f, 0.25f, 0.33f, 0);
    private Color selectedColor = new Color(0.17f, 0.25f, 0.33f, 1);

    void Start()
    {
        backgroundImage.color = defaultColor;
    }

    void Update()
    {
        
    }

    public void Init(string name)
    {
        objectNameInput.text = name;
    }

    public void SetDepth(int hierarchyDepth)
    {
        content.offsetMin = new Vector2(hierarchyDepth * 10, 0);
    }

    public void Select()
    {
        backgroundImage.color = selectedColor;
    }

    public void Deselect()
    {
        backgroundImage.color = defaultColor;
    }

    public void Expand()
    {
        toggleArrow.transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    public void Collapse()
    {
        toggleArrow.transform.rotation = Quaternion.identity;
    }
}
