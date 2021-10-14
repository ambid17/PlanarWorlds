using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabUI : MonoBehaviour
{
    public GameObject listItemPrefab;

    public GameObject propList;
    public GameObject playersList;
    public GameObject monstersList;

    public Transform propsContainer;
    public Transform playersContainer;
    public Transform monstersContainer;

    public TabButton propButton;
    public TabButton playersButton;
    public TabButton monstersButton;

    private PrefabInteractionManager _prefabGizmoManager;
    private PrefabManager _prefabManager;

    void Start()
    {
        _prefabGizmoManager = PrefabInteractionManager.GetInstance();
        _prefabManager = PrefabManager.GetInstance();
        PopulateMenu(_prefabManager.propList.prefabs, propsContainer, PrefabType.Prop);
        PopulateMenu(_prefabManager.playerList.prefabs, playersContainer, PrefabType.Player);
        PopulateMenu(_prefabManager.monsterList.prefabs, monstersContainer, PrefabType.Monster);
        InitButtons();
        PrefabTypeButtonClicked(PrefabType.Prop);
    }

    private void PopulateMenu(Prefab[] prefabList, Transform container, PrefabType prefabType)
    {
        foreach (Prefab prefab in prefabList)
        {
            GameObject prefabItemGO = Instantiate(listItemPrefab, container);
            prefabItemGO.name = prefab.gameObject.name;

            PrefabListItem prefabListItem = prefabItemGO.GetComponent<PrefabListItem>();
            prefabListItem.button.onClick.AddListener(() => PrefabButtonClicked(prefab, prefabType));
            prefabListItem.image.sprite = Sprite.Create(prefab.previewTexture, new Rect(0, 0, prefab.previewTexture.width, prefab.previewTexture.height), new Vector2(0.5f, 0.5f));
            prefabListItem.text.text = prefab.gameObject.name;
        }
    }

    private void PrefabButtonClicked(Prefab prefab, PrefabType prefabType)
    {
        GameObject instance = _prefabManager.CreatePrefabInstance(prefab, prefabType);
        
        instance.layer = Constants.PrefabPlacementLayer;

        _prefabGizmoManager.ForceSelectObject(instance);
    }

    private void InitButtons()
    {
        propButton.SetupAction(() => PrefabTypeButtonClicked(PrefabType.Prop));
        playersButton.SetupAction(() => PrefabTypeButtonClicked(PrefabType.Player));
        monstersButton.SetupAction(() => PrefabTypeButtonClicked(PrefabType.Monster));
        propButton.Select();
    }

    private void PrefabTypeButtonClicked(PrefabType prefabType)
    {
        propList.SetActive(prefabType == PrefabType.Prop);
        playersList.SetActive(prefabType == PrefabType.Player);
        monstersList.SetActive(prefabType == PrefabType.Monster);

        propButton.Unselect();
        playersButton.Unselect();
        monstersButton.Unselect();
    }
}
