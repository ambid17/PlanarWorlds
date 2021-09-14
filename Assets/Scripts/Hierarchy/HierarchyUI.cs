using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        hierarchyItems.Clear();

        foreach(Transform child in _prefabManager.prefabContainer)
        {
            GameObject go = Instantiate(HierarchyItemPrefab, scrollViewContent);
            HierarchyItem item = go.GetComponent<HierarchyItem>();
            item.Init(child.name);
            hierarchyItems.Add(item);
        }
    }
}
