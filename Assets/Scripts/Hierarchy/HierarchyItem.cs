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

    public Image backgroundImage;
    public TMP_InputField objectNameInput;
    public RectTransform content;
    public PointerEventListener clickListener;

    public TMP_Text inputText;
    private TMP_SelectionCaret inputSelectionCaret;


    public static Color defaultColor = new Color(0.17f, 0.25f, 0.33f, 0);
    public static Color selectedColor = new Color(0.17f, 0.25f, 0.33f, 1);

    public GameObject reference;

    private RectTransform myRect;
    private UIManager _uiManager;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        inputText.raycastTarget = false;
        _uiManager = UIManager.GetInstance();
    }

    void Start()
    {
        backgroundImage.color = defaultColor;
        objectNameInput.onEndEdit.AddListener(delegate { UpdateObjectName(); });
        objectNameInput.onSelect.AddListener(delegate { _uiManager.isEditingValues = true; });
        objectNameInput.onDeselect.AddListener(delegate { _uiManager.isEditingValues = false; });

        clickListener.PointerClick += (eventData) => ManualSelect();

        inputSelectionCaret = objectNameInput.GetComponentInChildren<TMP_SelectionCaret>(true);
        inputSelectionCaret.raycastTarget = false;
    }
    
    public void Init(GameObject reference)
    {
        this.reference = reference;
        objectNameInput.text = reference.name;
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
}
