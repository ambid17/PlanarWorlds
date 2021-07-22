using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : StaticMonoBehaviour<UIManager>
{
    public GameObject PrefabCanvas;
    public GameObject InspectorCanvas;
    public GameObject HierarchyCanvas;
    public GameObject CampaignCanvas;

    void Start()
    {
        CampaignCanvas.SetActive(true);
        PrefabCanvas.SetActive(false);
        InspectorCanvas.SetActive(false);
        HierarchyCanvas.SetActive(false);
    }

    void Update()
    {
        
    }
}
