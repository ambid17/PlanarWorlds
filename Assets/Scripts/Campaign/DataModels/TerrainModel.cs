using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TerrainModel
{
    [SerializeField]
    public float[,] heightMap;
    [SerializeField]
    public float[,,] alphaMap;
    [SerializeField] 
    public int[][,] detailMap;

}
