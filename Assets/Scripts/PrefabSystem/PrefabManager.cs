using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class PrefabManager : StaticMonoBehaviour<PrefabManager>
{
    public Transform prefabContainer;
    public PrefabList propList;
    public PrefabList playerList;
    public PrefabList monsterList;
    public GameObject playerNameTextPrefab;


    private int instanceIdCounter = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
    }

    public void LoadPrefabFromSave(PrefabModel model)
    {
        Prefab prefab = LookupPrefab(model.prefabType, model.prefabId);

        GameObject instance = Instantiate(prefab.gameObject, prefabContainer);
        instance.transform.position = model.position.ToVector3();
        instance.transform.rotation = Quaternion.Euler(model.rotation.ToVector3());
        instance.transform.localScale = model.scale.ToVector3();
        instance.layer = Constants.PrefabParentLayer;
        instance.name = model.name;

        if (model.prefabType == PrefabType.Prop)
        {
            PrefabInstanceData instanceData = instance.AddComponent<PrefabInstanceData>();
            instanceData.prefabId = prefab.prefabId;
            instanceData.prefabType = model.prefabType;
        }
        else
        {
            CharacterModel characterModel = model as CharacterModel;
            CharacterInstanceData instanceData = instance.AddComponent<CharacterInstanceData>();
            instanceData.prefabId = prefab.prefabId;
            instanceData.prefabType = model.prefabType;
            instanceData.characterName = characterModel.name;
            instanceData.characterHp = characterModel.characterHp;
            instanceData.characterInitiative = characterModel.characterInitiative;
            instanceData.characterSpeed = characterModel.characterSpeed;
            instanceData.instanceId = ++instanceIdCounter;

            TMP_Text nameText = instance.GetComponentInChildren<TMP_Text>();
            nameText.text = instanceData.characterName;

            EncounterManager.Instance.AddCharacter(instanceData);
        }

        SetObjectShader(instance);
        CreateObjectCollider(instance);
    }

    public Prefab LookupPrefab(PrefabType prefabType, int prefabId)
    {
        Prefab toReturn = new Prefab();

        if (prefabType == PrefabType.Prop)
        {
            toReturn = propList.prefabs.Where(p => p.prefabId == prefabId).FirstOrDefault();
        }
        else if (prefabType == PrefabType.Player)
        {
            toReturn = playerList.prefabs.Where(p => p.prefabId == prefabId).FirstOrDefault();
        }
        else
        {
            toReturn = monsterList.prefabs.Where(p => p.prefabId == prefabId).FirstOrDefault();
        }

        return toReturn;
    }

    public GameObject CreatePrefabInstance(Prefab prefab, PrefabType prefabType)
    {
        GameObject instance = Instantiate(prefab.gameObject, prefabContainer);
        instance.name = prefab.prefabName;

        if(prefabType == PrefabType.Prop)
        {
            PrefabInstanceData instanceData = instance.AddComponent<PrefabInstanceData>();
            instanceData.prefabId = prefab.prefabId;
            instanceData.prefabType = prefabType;
        }
        else
        {
            CharacterInstanceData instanceData = instance.AddComponent<CharacterInstanceData>();
            instanceData.prefabId = prefab.prefabId;
            instanceData.prefabType = prefabType;
            instanceData.characterName = prefab.prefabName;
            instanceData.characterHp = 10;
            instanceData.characterInitiative = 0;
            instanceData.characterSpeed = 30;
            instanceData.instanceId = ++instanceIdCounter;
            
            TMP_Text nameText = instance.GetComponentInChildren<TMP_Text>();
            nameText.text = instanceData.characterName;

            EncounterManager.Instance.AddCharacter(instanceData);
        }

        SetObjectShader(instance);
        CreateObjectCollider(instance);

        return instance;
    }

    public void SetObjectShader(GameObject instance)
    {
        MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            // The Player's name text is a child, don't change its material
            if (renderer.gameObject.name == "PlayerNameText")
            {
                continue;
            }
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
        Collider myCollider = instance.GetComponent<Collider>();
        if (myCollider != null)
        {
            return;
        }

        BoxCollider myBoxCollider = instance.AddComponent<BoxCollider>();

        // Unity will auto calculate collider size for only one object, we don't need the rest of the code if the model is set up properly
        if (instance.transform.childCount == 0)
            return;

        Bounds bounds = new Bounds(instance.transform.position, Vector3.zero);
        Renderer myRenderer = instance.GetComponent<Renderer>();
        if (myRenderer)
        {
            bounds.Encapsulate(myRenderer.bounds);
        }

        myBoxCollider.center = bounds.center - instance.transform.position;
        myBoxCollider.size = bounds.size;

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
            myBoxCollider.center = bounds.center - instance.transform.position;
            myBoxCollider.size = bounds.size;
        }
    }

    public void LoadCampaign(Campaign campaign)
    {
        foreach (PrefabModel propModel in campaign.props)
        {
            LoadPrefabFromSave(propModel);
        }

        foreach (CharacterModel characterModel in campaign.characters)
        {
            LoadPrefabFromSave(characterModel);
        }
    }

    public void PopulateCampaign(Campaign campaign)
    {
        campaign.props = new List<PrefabModel>();
        campaign.characters = new List<CharacterModel>();

        foreach (Transform child in prefabContainer)
        {
            PrefabInstanceData instanceData = child.GetComponent<PrefabInstanceData>();
            if(instanceData.prefabType == PrefabType.Prop)
            {
                campaign.props.Add(instanceData.GetPrefabModel());
            }
            else
            {
                CharacterInstanceData characterInstance = instanceData as CharacterInstanceData;
                campaign.characters.Add(characterInstance.GetCharacterModel());
            }
        }
    }

    public void Clear()
    {
        // Delete all of the current prefabs
        foreach (Transform child in prefabContainer)
        {
            Destroy(child.gameObject);
        }

        instanceIdCounter = 0;
    }
}
