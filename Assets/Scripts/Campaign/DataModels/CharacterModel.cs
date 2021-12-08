using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CharacterModel : PrefabModel
{
    [SerializeField]
    public int characterHp;
    [SerializeField]
    public int characterSpeed;
    [SerializeField]
    public int characterInitiative;
}
