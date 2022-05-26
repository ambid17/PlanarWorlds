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
    [SerializeField]
    public SerializedTree[] treeMap;
}

[Serializable]
public class SerializedTree
{
    [SerializeField]
    public float x;
    [SerializeField]
    public float y;
    [SerializeField]
    public float z;
    [SerializeField]
    public int prefabIndex;
}