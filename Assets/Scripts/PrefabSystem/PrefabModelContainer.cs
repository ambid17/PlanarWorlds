using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabModelContainer : MonoBehaviour
{
    public int prefabId;

    public PrefabModel GetPrefabModel()
    {
        PrefabModel myModel = new PrefabModel()
        {
            position = transform.position,
            rotation = transform.rotation.eulerAngles,
            scale = transform.localScale,
            prefabId = prefabId,
            name = gameObject.name,
        };

        return myModel;
    }
}
