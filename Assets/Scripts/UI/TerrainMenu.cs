using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class TerrainMenu : MonoBehaviour
{
    public TMP_Dropdown terrainTypeDropdown;
    public Button closeButton;

    //TileMap
    public GameObject brushContainer;
    public GameObject dragContainer;
    public GameObject smartDragContainer;

    public TMP_InputField brushSizeInput;
    public SwitchManager dragSwitchManager;
    public SwitchManager smartDragSwitchManager;

    // Terrain

    private UIManager _uIManager;
    private TerrainManager _terrainManager;

    private void Awake()
    {
        _uIManager = UIManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();

        UIManager.OnEditModeChanged += EditModeChanged;
    }

    void Start()
    {
        PopulateTerrainTypes();
        terrainTypeDropdown.onValueChanged.AddListener(OnTerrainTypeChanged);
        terrainTypeDropdown.value = 0;
        InitInputFields();
    }

    private void PopulateTerrainTypes()
    {
        terrainTypeDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("2D");
        options.Add("3D");

        terrainTypeDropdown.AddOptions(options);
    }

    private void InitInputFields()
    {
        InitInputField(brushSizeInput);
        dragSwitchManager.switchButton.onClick.AddListener(OnDragClicked);
        smartDragSwitchManager.switchButton.onClick.AddListener(OnSmartDragClicked);
        closeButton.onClick.AddListener(Close);
    }

    private void InitInputField(TMP_InputField inputField)
    {
        brushSizeInput.text = _terrainManager.tileMapEditor.BrushSize.ToString();
        brushSizeInput.onValueChanged.AddListener(delegate { BrushSizeUpdated(); });
        brushSizeInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        brushSizeInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });
        brushSizeInput.onValidateInput += (string input, int charIndex, char addedChar) =>
        {
            return InputValidation.ValidateCharAsUnsignedInt(addedChar);
        };
    }

    private void OnTerrainTypeChanged(int index)
    {
        brushContainer.gameObject.SetActive(index == 0);
        dragContainer.gameObject.SetActive(index == 0);
        smartDragContainer.gameObject.SetActive(index == 0);

        _terrainManager.ChangeTerrainMode(index == 0 ? TerrainMode.TileMap : TerrainMode.Mesh);
    }

    private void BrushSizeUpdated()
    {
        int newBrushSize = InputValidation.ValidateInt(text: brushSizeInput.text, defaultValue: Constants.defaultBrushSize);

        _terrainManager.tileMapEditor.SetBrushSize(newBrushSize);
    }

    private void OnDragClicked()
    {
        _terrainManager.tileMapEditor.isDragEnabled = !_terrainManager.tileMapEditor.isDragEnabled;

        if (!_terrainManager.tileMapEditor.isDragEnabled && _terrainManager.tileMapEditor.isSmartDragEnabled) 
        {
            _terrainManager.tileMapEditor.isSmartDragEnabled = false;
            smartDragSwitchManager.AnimateSwitch();
        }

        _terrainManager.tileMapEditor.isValidDrag = false;
        _terrainManager.tileMapEditor.ToggleTileSelector();
    }

    private void OnSmartDragClicked()
    {
        _terrainManager.tileMapEditor.isSmartDragEnabled = !_terrainManager.tileMapEditor.isSmartDragEnabled;

        if (_terrainManager.tileMapEditor.isSmartDragEnabled && !_terrainManager.tileMapEditor.isDragEnabled)
        {
            _terrainManager.tileMapEditor.isDragEnabled = true;
            dragSwitchManager.AnimateSwitch();
        }

        _terrainManager.tileMapEditor.isValidDrag = false;
        _terrainManager.tileMapEditor.ToggleTileSelector();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if(newEditMode != EditMode.Terrain)
        {
            Close();
        }
    }
}
