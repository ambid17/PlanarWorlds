using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabUI : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform scrollViewContainer;

    private PrefabGizmoManager _prefabGizmoManager;
    private PrefabManager _prefabManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        _prefabManager = PrefabManager.GetInstance();
        PopulatePrefabMenu();
    }

    private void PopulatePrefabMenu()
    {
        foreach (Prefab prefab in _prefabManager.prefabList.prefabs)
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
        GameObject instance = _prefabManager.CreatePrefabInstance(prefab.gameObject, prefab.prefabId);
        
        instance.layer = Constants.PrefabPlacementLayer;

        _prefabGizmoManager.OnTargetObjectChanged(instance, true);
    }
}
