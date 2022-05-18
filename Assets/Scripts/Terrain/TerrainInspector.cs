using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainInspector : MonoBehaviour
{
    public GameObject meshMapInspector;

    void Start()
    {
        
    }

    public void SetTerrainMode()
    {
        meshMapInspector.SetActive(true);
    }
}
