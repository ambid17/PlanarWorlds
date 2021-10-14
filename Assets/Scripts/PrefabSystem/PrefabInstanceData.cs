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
            position = transform.position,
            rotation = transform.rotation.eulerAngles,
            scale = transform.localScale,
            name = gameObject.name,
            prefabId = prefabId,
            prefabType = prefabType
        };

        return myModel;
    }
}
