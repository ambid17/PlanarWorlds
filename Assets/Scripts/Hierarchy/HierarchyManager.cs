using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyManager : StaticMonoBehaviour<HierarchyManager>
{
    public HierarchyUI hierarchyUI;

    public event Action objectAddedToScene;

    private void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        
    }

    public void ManuallyLoad()
    {
        hierarchyUI.ManuallyLoad();
    }

    public void AddItem(GameObject reference)
    {
        hierarchyUI.AddItem(reference);
    }

    public void RemoveItem(GameObject reference)
    {
        hierarchyUI.RemoveItem(reference);
    }

    public void SelectItems(List<GameObject> references)
    {
        hierarchyUI.SelectItems(references);
    }

    public void DeselectItems(List<GameObject> references)
    {
        hierarchyUI.DeselectItems(references);
    }
}
