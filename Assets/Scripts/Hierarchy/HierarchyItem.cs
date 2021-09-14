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

    public GameObject reference;
    private Transform prefabContainer;

    void Start()
    {
        backgroundImage.color = defaultColor;
        objectNameInput.onEndEdit.AddListener(delegate { UpdateObjectName(); });
    }

    void Update()
    {
        
    }

    public void Init(GameObject reference, Transform container)
    {
        this.reference = reference;
        prefabContainer = container;

        objectNameInput.text = reference.name;
        SetDepth();
    }

    public void SetDepth()
    {
        int hierarchyDepth = 0;

        while (reference.transform.parent != prefabContainer)
        {
            hierarchyDepth++;
        }
        
        content.offsetMin = new Vector2(hierarchyDepth * 10, 0);
    }

    private void UpdateObjectName()
    {
        reference.name = objectNameInput.text;
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
