using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HierarchyItem : MonoBehaviour
{
    public event Action<GameObject> ItemSelected;
    public event Action<GameObject> ItemExpanded;
    public event Action<GameObject> ItemCollapsed;

    public Image backgroundImage;
    public TMP_InputField objectNameInput;
    public RectTransform content;
    public GameObject toggleArrow;
    public Button expandButton;
    public Button selectButton;

    private Color defaultColor = new Color(0.17f, 0.25f, 0.33f, 0);
    private Color selectedColor = new Color(0.17f, 0.25f, 0.33f, 1);

    public GameObject reference;
    private Transform prefabContainer;

    private bool isExpanded;

    private List<HierarchyItem> children;

    private RectTransform myRect;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
    }

    void Start()
    {
        backgroundImage.color = defaultColor;
        objectNameInput.onEndEdit.AddListener(delegate { UpdateObjectName(); });
        selectButton.onClick.AddListener(ManualSelect);
        expandButton.onClick.AddListener(ToggleExpand);

        isExpanded = false;
        children = new List<HierarchyItem>();
    }
    
    public void Init(GameObject reference, Transform prefabContainer)
    {
        this.reference = reference;
        this.prefabContainer = prefabContainer;

        objectNameInput.text = reference.name;
        SetDepth();
    }

    public void AddChild(HierarchyItem childItem)
    {
        children.Add(childItem);
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

    private void ManualSelect()
    {
        ItemSelected?.Invoke(reference);
        backgroundImage.color = selectedColor;
        objectNameInput.interactable = true;
    }

    public RectTransform SelectFromScene()
    {
        backgroundImage.color = selectedColor;
        objectNameInput.interactable = true;
        return myRect;
    }

    public void Deselect()
    {
        backgroundImage.color = defaultColor;
        objectNameInput.interactable = false;
    }

    public void ToggleExpand()
    {
        isExpanded = !isExpanded;

        if (isExpanded)
        {
            ItemExpanded?.Invoke(reference);
            toggleArrow.transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else
        {
            ItemCollapsed?.Invoke(reference);
            toggleArrow.transform.rotation = Quaternion.identity;
        }
    }
}
