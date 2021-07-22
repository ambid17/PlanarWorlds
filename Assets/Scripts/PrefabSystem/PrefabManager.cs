using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : StaticMonoBehaviour<PrefabManager>
{
    public Transform prefabParent;
    public PrefabItemContainer prefabItemContainer;
    public PrefabList prefabList;
    public GameObject listItemPrefab;
    public Transform scrollViewContainer;

    private Camera mainCamera;
    private PrefabGizmoManager _prefabGizmoManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        mainCamera = Camera.main;
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
        instance.layer = Constants.PrefabParentLayer;
        foreach(Transform child in instance.transform)
        {
            child.gameObject.layer = Constants.PrefabChildLayer;
        }

        PlaceObjectInContext(instance);
        _prefabGizmoManager.OnTargetObjectChanged(instance);
    }

    private void PlaceObjectInContext(GameObject instance)
    {
        if(_prefabGizmoManager.TargetObject != null)
        {
            GameObject snapTarget = _prefabGizmoManager.TargetObject;
            // TODO: try and get snapping on placement to work
            Vector3 startPosition = snapTarget.transform.position;
            startPosition.y += instance.transform.localScale.y / 2; // add half of my height
            startPosition.y += snapTarget.transform.localScale.y / 2; // add half of the snap target's height

            instance.transform.position = startPosition;
        }
        else
        {
            Vector3 startPosition = mainCamera.transform.position + (mainCamera.transform.forward * 10);
            startPosition.y = 0;
            instance.transform.position = startPosition;

            Vector3 instanceYRotation = instance.transform.rotation.eulerAngles;
            instanceYRotation.y = mainCamera.transform.rotation.eulerAngles.y;
            //snap to nearest 15 degrees
            instanceYRotation.y = Mathf.Round(instanceYRotation.y / 15) * 15;
            instance.transform.rotation = Quaternion.Euler(instanceYRotation);
        }
    }

    public void LoadPrefabFromSave(SerializedPrefab prefab)
    {
        //PrefabItem prefabItem = GetPrefabItem(prefab);
        //GameObject instance = Instantiate(prefabItem.prefab, prefabParent);
        //instance.layer = Constants.PrefabLayer;
        //instance.transform.position = prefab.position;
        //instance.transform.rotation = Quaternion.Euler(prefab.rotation);
        //instance.transform.localScale = prefab.scale;
    }
}
