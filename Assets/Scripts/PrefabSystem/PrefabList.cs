using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PrefabList", menuName = "ScriptableObjects/PrefabList")]
public class PrefabList : ScriptableObject
{
    [SerializeField]
    public Prefab[] prefabs;
}

[Serializable]
public class Prefab
{
    [SerializeField]
    public GameObject gameObject;
    [SerializeField]
    public Texture2D previewTexture;
}
