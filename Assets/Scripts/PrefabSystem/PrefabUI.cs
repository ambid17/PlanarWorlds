using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabUI : MonoBehaviour
{
    public Transform prefabParent;
    public PrefabList prefabList;
    public GameObject listItemPrefab;
    public Transform scrollViewContainer;

    private PrefabGizmoManager _prefabGizmoManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        PopulatePrefabMenu();
    }

    private void PopulatePrefabMenu()
    {
        foreach (Prefab prefab in prefabList.prefabs)
        {
            GameObject prefabItemGO = Instantiate(listItemPrefab, scrollViewContainer);
            prefabItemGO.name = prefab.gameObject.name;

            PrefabListItem prefabListItem = prefabItemGO.GetComponent<PrefabListItem>();

            prefabListItem.button.onClick.RemoveAllListeners();
            prefabListItem.button.onClick.AddListener(() => PrefabButtonClicked(prefab));

            prefabListItem.image.sprite = Sprite.Create(prefab.previewTexture, new Rect(0.0f, 0.0f, prefab.previewTexture.width, prefab.previewTexture.height), new Vector2(0.5f, 0.5f));
            prefabListItem.text.text = prefab.gameObject.name;
        }
    }

    private void PrefabButtonClicked(Prefab prefab)
    {
        GameObject instance = Instantiate(prefab.gameObject, prefabParent);
        instance.layer = Constants.PrefabPlacementLayer;

        MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer renderer in renderers)
        {
            renderer.material.shader = Shader.Find("");
        }

        CreateObjectCollider(instance);
        _prefabGizmoManager.OnTargetObjectChanged(instance);
    }

    // If the object doesn't have a collider
    // Create a parent collider, and make it encapsulate all it's child meshes
    private void CreateObjectCollider(GameObject instance)
    {
        BoxCollider myCollider = instance.GetComponent<BoxCollider>();
        if (!myCollider)
        {
            myCollider = instance.AddComponent<BoxCollider>();
        }

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
