using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PrefabModel
{
    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public Vector3 rotation;
    [SerializeField]
    public Vector3 scale;
    [SerializeField]
    public int prefabId;
}