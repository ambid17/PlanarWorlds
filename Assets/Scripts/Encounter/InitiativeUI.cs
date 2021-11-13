using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeUI : MonoBehaviour
{
    public TabButton sortButton;
    public TabButton lockButton;
    public TabButton nextTurnButton;

    public GameObject imageButtonPrefab;
    public GameObject initiativeImageContainer;

    private ImageTabButton _currentImageButton;

    private Dictionary<CharacterInstanceData, ImageTabButton> _characterIcons;

    void Start()
    {
        _characterIcons = new Dictionary<CharacterInstanceData, ImageTabButton>();
        InitButtons();
        UIManager.OnEditModeChanged += EditModeChanged;
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if(newEditMode == EditMode.Encounter)
        {
            RefreshCharacterList();
        }
    }

    private void InitButtons()
    {
        sortButton.SetupAction(() => Sort());
        lockButton.SetupAction(() => Lock());
        nextTurnButton.SetupAction(() => NextTurn());
    }

    private void Sort()
    {
        EncounterManager.Instance.SortCharactersByInitiative();
    }

    private void Lock()
    {
        if (lockButton.isSelected)
        {
            lockButton.Unselect();
        }
    }

    private void NextTurn()
    {
        CharacterInstanceData nextCharacter =  EncounterManager.Instance.GetNextCharacter();

        if(nextCharacter != null)
        {
            SetCurrentCharacter(nextCharacter);
        }

        nextTurnButton.Unselect();
        EncounterManager.Instance.FocusOnCharacter();
    }

    public void RefreshCharacterList()
    {
        ClearCharacterList();

        if (EncounterManager.Instance.Characters.Count == 0)
        {
            return;
        }

        foreach (CharacterInstanceData characterInstance in EncounterManager.Instance.Characters)
        {
            if (characterInstance.characterHp <= 0)
                continue;

            GameObject newButton = Instantiate(imageButtonPrefab, initiativeImageContainer.transform);
            newButton.name = $"{characterInstance.characterName} icon";

            ImageTabButton iconButton = newButton.GetComponent<ImageTabButton>();
            
            Texture2D previewTexture = PrefabManager.Instance.LookupPrefab(characterInstance.prefabType, characterInstance.prefabId).previewTexture;
            Sprite characterSprite = Sprite.Create(previewTexture, new Rect(0, 0, previewTexture.width, previewTexture.height), new Vector2(0.5f, 0.5f));
            iconButton.Setup(characterSprite, () => SetCurrentCharacter(characterInstance));
            iconButton.buttonText.text = characterInstance.characterName;

            _characterIcons.Add(characterInstance, iconButton);
        }
    }

    private void ClearCharacterList()
    {
        _characterIcons = new Dictionary<CharacterInstanceData, ImageTabButton>();
        foreach (Transform child in initiativeImageContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetCurrentCharacter(CharacterInstanceData characterInstance)
    {
        // Unselect the last image button
        if (_currentImageButton)
        {
            _currentImageButton.Unselect();
        }

        if (characterInstance)
        {
            _characterIcons.TryGetValue(characterInstance, out ImageTabButton buttonForCharacter);

            if (buttonForCharacter)
            {
                _currentImageButton = buttonForCharacter;
                _currentImageButton.Select();
            }
        }
        
        EncounterManager.Instance.OnCharacterChanged(characterInstance);
    }

    public void OnCharacterSelectedManually(CharacterInstanceData characterInstance)
    {
        if (_currentImageButton)
        {
            _currentImageButton.Unselect();
        }

        if (characterInstance)
        {
            _characterIcons.TryGetValue(characterInstance, out ImageTabButton buttonForCharacter);

            if (buttonForCharacter)
            {
                _currentImageButton = buttonForCharacter;
                _currentImageButton.Select();
            }
        }
        
    }

    public void UpdateCharacterName(CharacterInstanceData characterInstance)
    {
        if (characterInstance)
        {
            _characterIcons.TryGetValue(characterInstance, out ImageTabButton buttonForCharacter);

            if (buttonForCharacter)
            {
                buttonForCharacter.buttonText.text = characterInstance.characterName;
            }
        }
    }
}
