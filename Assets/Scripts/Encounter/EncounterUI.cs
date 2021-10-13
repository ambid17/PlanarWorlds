using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EncounterUI : MonoBehaviour
{

    public GameObject effectsList;
    public GameObject playersList;

    public GameObject playerItemPrefab;

    public TabButton playerButton;
    public TabButton effectsButton;

    public TMP_InputField playerHpInput;
    public TMP_InputField playerNameInput;
    public TMP_InputField playerInitiativeInput;
    public TMP_InputField speedInput;

    void Start()
    {
        playerButton.Select();
    }

    void Update()
    {
        
    }

    private void PopulateEffectsList()
    {

    }

    private void InitInpuits()
    {
        playerHpInput.onValueChanged.AddListener(delegate { OnHpUpdated(); });
        playerHpInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        playerHpInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });

        playerNameInput.onValueChanged.AddListener(delegate { OnHpUpdated(); });
        playerNameInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        playerNameInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });

        playerInitiativeInput.onValueChanged.AddListener(delegate { OnHpUpdated(); });
        playerInitiativeInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        playerInitiativeInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });

        speedInput.onValueChanged.AddListener(delegate { OnHpUpdated(); });
        speedInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        speedInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });
    }

    private void OnHpUpdated()
    {

    }
}
