using RTG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EncounterPlacementType
{

}
public class EncounterManager : StaticMonoBehaviour<EncounterManager>
{
    [SerializeField]
    private CharacterInspector _characterInspector;
    [SerializeField]
    private InitiativeUI _initiativeUI;
    [SerializeField]
    private LayerMask _characterLayerMask;
    [SerializeField]
    private LayerMask _terrainLayerMask;
    private Camera _mainCamera;

    private UIManager _uiManager;

    private List<CharacterInstanceData> _characters = new List<CharacterInstanceData>();
    public List<CharacterInstanceData> Characters => _characters;

    public CharacterInstanceData selectedCharacter;

    private bool _isMovingCharacter = false;
    private Vector3 _moveStartPosition;

    void Start()
    {
        _mainCamera = Camera.main;
        _uiManager = UIManager.Instance;
    }

    void Update()
    {
        if (_uiManager.EditMode != EditMode.Encounter || _uiManager.UserCantInput)
            return;

        if (!_isMovingCharacter)
        {
            TrySelectCharacter();
            TryMoveCharacter();
        }
        else
        {
            MoveCharacter();
            TryCancelMove();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            FocusOnCharacter();
        }
        
    }

    private void TrySelectCharacter()
    {
        if (Input.GetMouseButtonDown(0)
            && !EventSystem.current.IsPointerOverGameObject())
        {
            GameObject selectedObject = null;
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, _characterLayerMask))
            {
                selectedObject = rayHit.collider.gameObject;
            }

            CharacterInstanceData characterInstance = null;

            if (selectedObject)
            {
                characterInstance = selectedObject.GetComponent<CharacterInstanceData>();
            }

            if (characterInstance != selectedCharacter)
            {
                OnCharacterChanged(characterInstance);
                _initiativeUI.OnCharacterSelectedManually(characterInstance);
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

    private void TryMoveCharacter()
    {
        if(Input.GetMouseButtonDown(2)
            && !EventSystem.current.IsPointerOverGameObject()
            && selectedCharacter != null)
        {
            _isMovingCharacter = true;
            _moveStartPosition = selectedCharacter.transform.position;
        }
    }

    private void MoveCharacter()
    {
        if (selectedCharacter == null)
            return;

        // Build a ray using the current mouse cursor position
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        // Check if the ray intersects the terrain. If it does, snap the object to the terrain
        if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, _terrainLayerMask))
        {
            selectedCharacter.transform.position = rayHit.point;
        }
        else
        {
            _isMovingCharacter = false;
            selectedCharacter.transform.position = _moveStartPosition;
        }
    }

    private void TryCancelMove()
    {
        if (Input.GetMouseButton(0))
        {
            _isMovingCharacter = false;
        }

        if(Input.GetMouseButton(1))
        {
            _isMovingCharacter = false;
            selectedCharacter.transform.position = _moveStartPosition;
        }
    }

    public void AddCharacter(CharacterInstanceData newCharacter)
    {
        _characters.Add(newCharacter);
        SortCharactersByInitiative();
    }

    public void RemoveCharacter(CharacterInstanceData character)
    {
        _characters.Remove(character);
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

    public void OnHpUpdated()
    {
        if (selectedCharacter && selectedCharacter.characterHp <= 0)
        {
            selectedCharacter.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
            _initiativeUI.RefreshCharacterList();
            _characters.Remove(selectedCharacter);
            OnCharacterChanged(null);
        }
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
        if(selectedCharacter){
            // Always focus the camera pointing north
            _mainCamera.transform.rotation = Quaternion.Euler(new Vector3(35, 0, 0));
            RTFocusCamera.Get.Focus(new List<GameObject>() { selectedCharacter.gameObject });
        }
    }
}
