using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMapEditor : MonoBehaviour
{
    private bool _isDirty;

    void Start()
    {
        _isDirty = false;    
    }

    void Update()
    {
        
    }

    public bool IsDirty()
    {
        return _isDirty;
    }

    public void SaveIntoCampaign(Campaign campaign)
    {

    }

    public void LoadFromCampaign(Campaign campaign)
    {

    }

    public void Clear()
    {

    }
}
