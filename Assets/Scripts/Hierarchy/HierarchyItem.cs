using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

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
    public PointerEventListener clickListener;

    public TMP_Text inputText;
    private TMP_SelectionCaret inputSelectionCaret;


    public static Color defaultColor = new Color(0.17f, 0.25f, 0.33f, 0);
    public static Color selectedColor = new Color(0.17f, 0.25f, 0.33f, 1);
    public static float ItemHeight = 40;

    public GameObject reference;
    private Transform prefabContainer;

    private bool isExpanded;

    private List<HierarchyItem> children;

    private RectTransform myRect;

    private Transform dragItemContainer;
    private RectTransform scrollViewRect;
    private Vector3 startPosition;
    private Vector3 offset;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        inputText.raycastTarget = false;
    }

    void Start()
    {
        backgroundImage.color = defaultColor;
        objectNameInput.onEndEdit.AddListener(delegate { UpdateObjectName(); });
        expandButton.onClick.AddListener(ToggleExpand);

        isExpanded = false;
        children = new List<HierarchyItem>();

        clickListener.PointerClick += (eventData) => ManualSelect();
        clickListener.BeginDrag += (eventData) => OnBeginDrag(eventData);
        clickListener.Drag += (eventData) => OnDrag(eventData);
        clickListener.EndDrag += (eventData) => OnEndDrag(eventData );
        //clickListener.PointerUp +=

        inputSelectionCaret = objectNameInput.GetComponentInChildren<TMP_SelectionCaret>(true);
        inputSelectionCaret.raycastTarget = false;
    }
    
    public void Init(GameObject reference, Transform prefabContainer, Transform dragItemContainer, RectTransform scrollViewRect)
    {
        this.reference = reference;
        this.prefabContainer = prefabContainer;
        this.dragItemContainer = dragItemContainer;
        this.scrollViewRect = scrollViewRect;

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

        Transform parent = reference.transform.parent;
        while (parent != prefabContainer)
        {
            hierarchyDepth++;
            parent = parent.parent;
        }
        
        content.offsetMin = new Vector2(hierarchyDepth * 10, 0);
    }

    private void UpdateObjectName()
    {
        if(reference != null)
            reference.name = objectNameInput.text;
    }

    private void ManualSelect()
    {
        ItemSelected?.Invoke(reference);
        backgroundImage.color = selectedColor;
        objectNameInput.interactable = true;
        inputText.raycastTarget = true;
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
        inputText.raycastTarget = false;
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

    private void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        offset = Input.mousePosition - startPosition;
        EventSystem.current.SetSelectedGameObject(gameObject);
        EventSystem.current.currentSelectedGameObject.transform.SetParent(dragItemContainer);
        EventSystem.current.currentSelectedGameObject.transform.SetAsFirstSibling();
        Debug.Log("start drag " + gameObject.name);
    }

    private void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition - offset;
    }

    private void OnEndDrag(PointerEventData eventData)
    {
        transform.parent = scrollViewRect;

        int index = (int)((((transform.position.y + (ItemHeight / 2)) - Input.mousePosition.y)) / 35) + 1;

        if (index <= 0)
        {
            index = 1;
        }

        transform.SetSiblingIndex(index);

        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRect);
    }
}
