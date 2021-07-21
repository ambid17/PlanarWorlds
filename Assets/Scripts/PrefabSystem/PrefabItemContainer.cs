using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabItemContainer", menuName = "ScriptableObjects/PrefabItemContainer")]
public class PrefabItemContainer : ScriptableObject
{
    [SerializeField]
    public PrefabItem[] prefabs;
}
