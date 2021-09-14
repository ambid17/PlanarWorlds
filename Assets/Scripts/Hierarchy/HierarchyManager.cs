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

}
