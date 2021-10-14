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

    private List<ImageTabButton> _characterIcons;

    void Start()
    {
        _characterIcons = new List<ImageTabButton>();
        InitButtons();
    }

    void Update()
    {

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
            ImageTabButton iconButton = GetIconButton();
            iconButton.Select();
            SetCurrentCharacter(nextCharacter, iconButton);
        }

        nextTurnButton.Unselect();
    }

    private ImageTabButton GetIconButton()
    {
        int currentIndex = _characterIcons.IndexOf(_currentImageButton);

        if (currentIndex < _characterIcons.Count - 1)
        {
            currentIndex++;
        }
        else
        {
            currentIndex = 0;
        }

        return _characterIcons[currentIndex];
    }

    public void RefreshCharacterList()
    {
        ClearCharacterList();
        bool isFirst = true;
        foreach (CharacterInstanceData characterInstance in EncounterManager.Instance.Characters)
        {
            GameObject newButton = Instantiate(imageButtonPrefab, initiativeImageContainer.transform);
            ImageTabButton iconButton = newButton.GetComponent<ImageTabButton>();

            Texture2D previewTexture = PrefabManager.Instance.LookupPrefab(characterInstance.prefabType, characterInstance.prefabId).previewTexture;
            Sprite characterSprite = Sprite.Create(previewTexture, new Rect(0, 0, previewTexture.width, previewTexture.height), new Vector2(0.5f, 0.5f));
            iconButton.Setup(characterSprite, () => SetCurrentCharacter(characterInstance, iconButton));

            _characterIcons.Add(iconButton);
            if (isFirst)
            {
                iconButton.Select();
                _currentImageButton = iconButton;
                EncounterManager.Instance.OnCharacterChanged(characterInstance);

                isFirst = false;
            }
        }
    }

    private void ClearCharacterList()
    {
        _characterIcons = new List<ImageTabButton>();
        foreach (Transform child in initiativeImageContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetCurrentCharacter(CharacterInstanceData characterInstance, ImageTabButton iconButton)
    {
        if (_currentImageButton)
        {
            _currentImageButton.Unselect();
        }

        _currentImageButton = iconButton;
        EncounterManager.Instance.OnCharacterChanged(characterInstance);
        EncounterManager.Instance.FocusOnCharacter();
    }
}
