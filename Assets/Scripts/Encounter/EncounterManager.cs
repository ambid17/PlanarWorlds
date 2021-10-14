using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EncounterManager : StaticMonoBehaviour<EncounterManager>
{
    private List<CharacterInstanceData> _characters = new List<CharacterInstanceData>();

    public CharacterInstanceData selectedCharacter;

    [SerializeField]
    private CharacterInspector _characterInspector;
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

    private void OnCharacterChanged(CharacterInstanceData characterInstance)
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

    // We can only click on an object if:
    // - We click the left mouse button
    // - We aren't clicking on a gizmo
    // - We aren't clicking on any UI
    private bool DidValidClick()
    {
        return Input.GetMouseButtonDown(0)
            && !EventSystem.current.IsPointerOverGameObject();
    }

    public void AddCharacter(CharacterInstanceData newCharacter)
    {
        _characters.Add(newCharacter);
    }

    public void RemoveCharacter(CharacterInstanceData character)
    {
        _characters.Remove(character);
    }
}
