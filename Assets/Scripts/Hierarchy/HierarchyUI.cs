using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class HierarchyUI : MonoBehaviour
{
    public TMP_InputField searchInput;
    public GameObject HierarchyItemPrefab;
    public Transform scrollViewContent;

    private List<HierarchyItem> hierarchyItems;
    private HierarchyManager _hierarchyManager;
    private PrefabManager _prefabManager;

    void Start()
    {
        _hierarchyManager = HierarchyManager.GetInstance();
        _prefabManager = PrefabManager.GetInstance();
        hierarchyItems = new List<HierarchyItem>();
    }

    void Update()
    {
        
    }

    public void ManuallyLoad()
    {
        //hierarchyItems.Clear();

        //foreach(Transform child in _prefabManager.prefabContainer)
        //{
        //    GameObject go = Instantiate(HierarchyItemPrefab, scrollViewContent);
        //    HierarchyItem item = go.GetComponent<HierarchyItem>();
        //    item.Init(child.name);
        //    hierarchyItems.Add(item);
        //}
    }

    public void AddItem(GameObject reference)
    {
        GameObject go = Instantiate(HierarchyItemPrefab, scrollViewContent);
        HierarchyItem item = go.GetComponent<HierarchyItem>();
        item.Init(reference, _prefabManager.prefabContainer);
        hierarchyItems.Add(item);
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
                item.Select();
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
}
