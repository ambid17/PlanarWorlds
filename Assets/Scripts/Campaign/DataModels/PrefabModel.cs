using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PrefabModel
{
    [SerializeField]
    public MyVector3 position;
    [SerializeField]
    public MyVector3 rotation;
    [SerializeField]
    public MyVector3 scale;
    [SerializeField]
    public int prefabId;
    [SerializeField]
    public string name;
    [SerializeField]
    public PrefabType prefabType;


}

public enum PrefabType
{
    Prop, Player, Monster
}

/// <summary>
/// We have to make a custom data type because Vector3 isn't marked as Serializable
/// </summary>
[Serializable]
public class MyVector3
{
    [SerializeField]
    public float x;
    public float y;
    public float z;

    public MyVector3(Vector3 toConvert)
    {
        x = toConvert.x;
        y = toConvert.y;
        z = toConvert.z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

}