using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HierarchyUI : MonoBehaviour
{
    public GameObject HierarchyItemPrefab;
    public Transform scrollViewContent;
    public ScrollRect scrollRect;

    private List<HierarchyItem> hierarchyItems;
    private PrefabGizmoManager _prefabGizmoManager;

    void Awake()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        hierarchyItems = new List<HierarchyItem>();
    }

    public void AddItem(GameObject reference)
    {
        HierarchyItem parentItem = CreateItem(reference);
        hierarchyItems.Add(parentItem);
    }

    private HierarchyItem CreateItem(GameObject reference)
    {
        GameObject go = Instantiate(HierarchyItemPrefab, scrollViewContent);
        HierarchyItem item = go.GetComponent<HierarchyItem>();
        item.Init(reference);
        item.ItemSelected += OnItemSelected;

        return item;
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

    // Scroll the Hierarchy to the selected item
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
