using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstanceData : MonoBehaviour
{
    public int prefabId;
    public PrefabType prefabType;

    public virtual PrefabModel GetPrefabModel()
    {
        PrefabModel myModel = new PrefabModel()
        {
            position = new MyVector3(transform.position),
            rotation = new MyVector3(transform.rotation.eulerAngles),
            scale = new MyVector3(transform.localScale),
            name = gameObject.name,
            prefabId = prefabId,
            prefabType = prefabType
        };

        return myModel;
    }
}
