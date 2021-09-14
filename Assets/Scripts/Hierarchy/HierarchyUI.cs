using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class HierarchyUI : MonoBehaviour
{
    public TMP_InputField searchInput;
    public GameObject HierarchyItemPrefab;
    public Transform scrollViewContent;
    public RectTransform scrollViewRect;
    public ScrollRect scrollRect;

    private List<HierarchyItem> hierarchyItems;
    private HierarchyManager _hierarchyManager;
    private PrefabManager _prefabManager;
    private PrefabGizmoManager _prefabGizmoManager;

    void Start()
    {
        _hierarchyManager = HierarchyManager.GetInstance();
        _prefabManager = PrefabManager.GetInstance();
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        hierarchyItems = new List<HierarchyItem>();
    }

    void Update()
    {
        
    }

    public void AddItem(GameObject reference)
    {
        HierarchyItem parentItem = CreateItem(reference);

        if(reference.transform.childCount > 0)
        {
            foreach(Transform child in reference.transform)
            {
                HierarchyItem childItem = CreateItem(child.gameObject);
                parentItem.AddChild(childItem);
            }
        }

        hierarchyItems.Add(parentItem);
    }

    private HierarchyItem CreateItem(GameObject reference)
    {
        GameObject go = Instantiate(HierarchyItemPrefab, scrollViewContent);
        HierarchyItem item = go.GetComponent<HierarchyItem>();
        item.Init(reference, _prefabManager.prefabContainer);
        item.ItemSelected += OnItemSelected;
        item.ItemExpanded += OnItemExpanded;
        item.ItemCollapsed += OnItemCollapsed;

        return item;
    }

    public void AddChild(GameObject parent)
    {
        GameObject go = Instantiate(HierarchyItemPrefab, scrollViewContent);
        HierarchyItem item = go.GetComponent<HierarchyItem>();
    }

    public void RemoveItem(GameObject reference)
    {
        List<HierarchyItem> itemsToRemove = hierarchyItems.Where(item => item.reference == reference).ToList();
        foreach(HierarchyItem item in itemsToRemove)
        {
            hierarchyItems.Remove(item);
            Destroy(item.gameObject);
        }
    }

    public void SelectItems(List<GameObject> references)
    {
        foreach(GameObject reference in references)
        {
            HierarchyItem item = hierarchyItems
                .FirstOrDefault(item => item.reference == reference);
            if (item)
            {
                RectTransform tranformToScrollTo = item.SelectFromScene();
                ScrollTo(tranformToScrollTo);
            }
        }
    }

    public void DeselectItems(List<GameObject> references)
    {
        foreach (GameObject reference in references)
        {
            HierarchyItem item = hierarchyItems
                .FirstOrDefault(item => item.reference == reference);
            if(item)
                item.Deselect();
        }
    }

    public void Clear()
    {
        foreach (HierarchyItem item in hierarchyItems)
        {
            Destroy(item.gameObject);
        }

        hierarchyItems.Clear();
    }

    private void OnItemSelected(GameObject reference)
    {
        _prefabGizmoManager.ForceSelectObject(reference);
    }

    private void OnItemExpanded(GameObject reference)
    {

    }

    private void OnItemCollapsed(GameObject reference)
    {

    }

    public void ScrollTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        Vector2 viewportLocalPosition = scrollRect.viewport.localPosition;
        Vector2 childLocalPosition = target.localPosition;
        Vector2 result = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
        scrollRect.content.localPosition = result;
    }
}
