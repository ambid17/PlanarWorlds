using RTG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EncounterManager : StaticMonoBehaviour<EncounterManager>
{
    private List<CharacterInstanceData> _characters = new List<CharacterInstanceData>();
    public List<CharacterInstanceData> Characters => _characters;

    public CharacterInstanceData selectedCharacter;

    [SerializeField]
    private CharacterInspector _characterInspector;
    [SerializeField]
    private InitiativeUI _initiativeUI;
    [SerializeField]
    private LayerMask _characterLayerMask;
    private Camera _mainCamera;

    private UIManager _uiManager;

    void Start()
    {
        _mainCamera = Camera.main;
        _uiManager = UIManager.Instance;
    }

    void Update()
    {
        if (_uiManager.EditMode != EditMode.Encounter || _uiManager.UserCantInput)
            return;

        TrySelectCharacter();
    }

    private void TrySelectCharacter()
    {
        if (DidValidClick())
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, _characterLayerMask))
            {
                CharacterInstanceData characterInstance = rayHit.collider.gameObject.GetComponent<CharacterInstanceData>();
                if (characterInstance && characterInstance != selectedCharacter)
                {
                    OnCharacterChanged(characterInstance);
                }
            }
        }
    }

    public void OnCharacterChanged(CharacterInstanceData characterInstance)
    {
        ToggleOutlineRender(false);
        selectedCharacter = characterInstance;
        ToggleOutlineRender(true);

        _characterInspector.CharacterSelected(selectedCharacter);
    }

    private void ToggleOutlineRender(bool shouldRender)
    {
        if (selectedCharacter)
        {
            Renderer[] renderers = selectedCharacter.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                foreach (Material material in materials)
                {
                    material.SetFloat("_OutlineWidth", shouldRender ? 1.015f : 0);
                }
            }
        }
    }

    private bool DidValidClick()
    {
        return Input.GetMouseButtonDown(0)
            && !EventSystem.current.IsPointerOverGameObject();
    }

    public void AddCharacter(CharacterInstanceData newCharacter)
    {
        _characters.Add(newCharacter);
        _initiativeUI.RefreshCharacterList();
        SortCharactersByInitiative();
    }

    public void RemoveCharacter(CharacterInstanceData character)
    {
        _characters.Remove(character);
        _initiativeUI.RefreshCharacterList();
        SortCharactersByInitiative();
    }

    public void SortCharactersByInitiative()
    {
        _characters.Sort();
    }

    public void OnInitiativeUpdated()
    {
        SortCharactersByInitiative();
        _initiativeUI.RefreshCharacterList();
    }

    public CharacterInstanceData GetNextCharacter()
    {
        int currentIndex = _characters.IndexOf(selectedCharacter);

        if(currentIndex < _characters.Count - 1)
        {
            currentIndex++;
        }
        else
        {
            currentIndex = 0;
        }

        return _characters[currentIndex];
    }

    public void FocusOnCharacter()
    {
        // Always focus the camera pointing north
        RTFocusCamera.Get.Focus(new List<GameObject>() { selectedCharacter.gameObject });
    }
}
