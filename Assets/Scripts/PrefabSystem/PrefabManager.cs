using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrefabManager : StaticMonoBehaviour<PrefabManager>
{
    public Transform prefabContainer;
    public PrefabList prefabList;


    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void LoadPrefabFromSave(PrefabModel model)
    {
        Prefab prefab = prefabList.prefabs.Where(p => p.prefabId == model.prefabId).FirstOrDefault();

        GameObject instance = CreatePrefabInstance(prefab.gameObject, prefab.prefabId);

        instance.transform.position = model.position;
        instance.transform.rotation = Quaternion.Euler(model.rotation);
        instance.transform.localScale = model.scale;

        instance.layer = Constants.PrefabParentLayer;
    }

    public GameObject CreatePrefabInstance(GameObject prefabToInstantiate, int prefabId)
    {
        GameObject instance = Instantiate(prefabToInstantiate, prefabContainer);

        PrefabModelContainer container = instance.AddComponent<PrefabModelContainer>();
        container.prefabId = prefabId;

        SetObjectShader(instance);
        CreateObjectCollider(instance);


        return instance;
    }

    public void SetObjectShader(GameObject instance)
    {
        MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                material.shader = Shader.Find("Custom/Outline");
                material.SetFloat("_OutlineWidth", 0);
            }
        }
    }

    // If the object doesn't have a collider
    // Create a parent collider, and make it encapsulate all it's child meshes
    public void CreateObjectCollider(GameObject instance)
    {
        BoxCollider myCollider = instance.GetComponent<BoxCollider>();
        if (!myCollider)
        {
            myCollider = instance.AddComponent<BoxCollider>();
        }

        // Unity will auto calculate collider size for only one object, we don't need the rest of the code if the model is set up properly
        if (instance.transform.childCount == 0)
            return;

        Bounds bounds = new Bounds(instance.transform.position, Vector3.zero);
        Renderer myRenderer = instance.GetComponent<Renderer>();
        if (myRenderer)
        {
            bounds.Encapsulate(myRenderer.bounds);
        }

        myCollider.center = bounds.center - instance.transform.position;
        myCollider.size = bounds.size;

        Transform[] children = instance.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child == transform)
                return;

            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer)
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
            myCollider.center = bounds.center - instance.transform.position;
            myCollider.size = bounds.size;
        }
    }
}