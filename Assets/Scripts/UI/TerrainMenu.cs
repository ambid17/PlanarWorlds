using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class TerrainMenu : MonoBehaviour
{
    public Dropdown terrainTypeDropdown;
    public Button closeButton;

    //TileMap
    public TMP_InputField brushSizeInput;
    public SwitchManager dragSwitchManager;
    public SwitchManager smartDragSwitchManager;

    // Terrain

    private UIManager _uIManager;
    private TileMapManager _tileMapManager;

    private void Awake()
    {
        _uIManager = UIManager.GetInstance();
        _tileMapManager = TileMapManager.GetInstance();

        UIManager.OnEditModeChanged += EditModeChanged;
    }

    void Start()
    {
        InitInputFields();
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
        brushSizeInput.text = _tileMapManager.BrushSize.ToString();
        brushSizeInput.onValueChanged.AddListener(delegate { BrushSizeUpdated(); });
        brushSizeInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        brushSizeInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });
        brushSizeInput.onValidateInput += (string input, int charIndex, char addedChar) =>
        {
            return InputValidation.ValidateCharAsUnsignedInt(addedChar);
        };
    }

    private void BrushSizeUpdated()
    {
        int newBrushSize = InputValidation.ValidateInt(text: brushSizeInput.text, defaultValue: Constants.defaultBrushSize);

        _tileMapManager.SetBrushSize(newBrushSize);
    }

    private void OnDragClicked()
    {
        _tileMapManager.isDragEnabled = !_tileMapManager.isDragEnabled;

        if (!_tileMapManager.isDragEnabled && _tileMapManager.isSmartDragEnabled) 
        {
            _tileMapManager.isSmartDragEnabled = false;
            smartDragSwitchManager.AnimateSwitch();
        }

        _tileMapManager.isValidDrag = false;
        _tileMapManager.ToggleTileSelector();
    }

    private void OnSmartDragClicked()
    {
        _tileMapManager.isSmartDragEnabled = !_tileMapManager.isSmartDragEnabled;

        if (_tileMapManager.isSmartDragEnabled && !_tileMapManager.isDragEnabled)
        {
            _tileMapManager.isDragEnabled = true;
            dragSwitchManager.AnimateSwitch();
        }

        _tileMapManager.isValidDrag = false;
        _tileMapManager.ToggleTileSelector();
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
