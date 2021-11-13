using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInspector : MonoBehaviour
{
    public GameObject characterDataContainer;

    public TMP_InputField characterHpInput;
    public TMP_InputField characterNameInput;
    public TMP_InputField characterInitiativeInput;
    public TMP_InputField characterSpeedInput;

    void Start()
    {
        InitInputs();
        CharacterSelected(null);
    }

    private void InitInputs()
    {
        characterHpInput.onValueChanged.AddListener(delegate { OnHpUpdated(); });
        characterHpInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        characterHpInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });

        characterNameInput.onValueChanged.AddListener(delegate { OnNameUpdated(); });
        characterNameInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        characterNameInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });

        characterInitiativeInput.onValueChanged.AddListener(delegate { OnInitiativeUpdated(); });
        characterInitiativeInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        characterInitiativeInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });

        characterSpeedInput.onValueChanged.AddListener(delegate { OnSpeedUpdated(); });
        characterSpeedInput.onSelect.AddListener(delegate { UIManager.Instance.isEditingValues = true; });
        characterSpeedInput.onDeselect.AddListener(delegate { UIManager.Instance.isEditingValues = false; });
    }

    private void OnNameUpdated()
    {
        EncounterManager.Instance.selectedCharacter.characterName = characterNameInput.text;
        EncounterManager.Instance.OnNameUpdated();
    }

    private void OnHpUpdated()
    {
        int parsedValue = InputValidation.ValidateInt(text: characterHpInput.text, defaultValue: 0);
        EncounterManager.Instance.OnHpUpdated(parsedValue);
    }

    public void OnCharacterDowned()
    {

    }

    public void OnCharacterRevived()
    {

    }

    public void OnCharacterKilled()
    {

    }

    private void OnSpeedUpdated()
    {
        int parsedValue = InputValidation.ValidateInt(text: characterSpeedInput.text, defaultValue: 0);
        EncounterManager.Instance.selectedCharacter.characterSpeed = parsedValue;
    }

    private void OnInitiativeUpdated()
    {
        int parsedValue = InputValidation.ValidateInt(text: characterInitiativeInput.text, defaultValue: 0);
        EncounterManager.Instance.selectedCharacter.characterInitiative = parsedValue;
        EncounterManager.Instance.OnInitiativeUpdated();
    }

    public void CharacterSelected(CharacterInstanceData characterData)
    {
        if(characterData != null)
        {
            characterDataContainer.SetActive(true);
            characterHpInput.text = characterData.characterHp.ToString();
            characterInitiativeInput.text = characterData.characterInitiative.ToString();
            characterNameInput.text = characterData.characterName;
            characterSpeedInput.text = characterData.characterSpeed.ToString();
        }
        else
        {
            characterDataContainer.SetActive(false);
        }
    }
}
