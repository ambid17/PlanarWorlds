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
            children = GetChildren()
        };

        return myModel;
    }

    private List<PrefabModel> GetChildren()
    {
        List<PrefabModel> children = new List<PrefabModel>();
        foreach (Transform child in transform)
        {
            PrefabModelContainer container = child.GetComponent<PrefabModelContainer>();
            children.Add(container.GetPrefabModel());
        }

        return children;
    }
}
