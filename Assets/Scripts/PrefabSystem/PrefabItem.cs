using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabItem", menuName = "ScriptableObjects/PrefabItem")]
public class PrefabItem : ScriptableObject
{
    [SerializeField]
    public string objectName;
    [SerializeField]
    public Sprite previewImage;
    [SerializeField]
    public GameObject prefab;
    [SerializeField]
    public PrefabType prefabType;
    [SerializeField]
    public Vector3 minimumScale;
    [SerializeField]
    public Vector3 defaultScale;
}

public enum PrefabType
{
    SlabImperial,
    BeamImperial,
    WallImperial,
    CurbImperial,
    SlabMetric
}
