using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class TerrainMenu : MonoBehaviour
{
    public TMP_InputField brushSizeInput;
    public SwitchManager dragSwitchManager;
    public SwitchManager smartDragSwitchManager;
    public Button closeButton;

    private UIManager _uIManager;
    private TileMapManager _terrainManager;

    private void Awake()
    {
        _uIManager = UIManager.GetInstance();
        _terrainManager = TileMapManager.GetInstance();

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
        brushSizeInput.text = _terrainManager.BrushSize.ToString();
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

        _terrainManager.SetBrushSize(newBrushSize);
    }

    private void OnDragClicked()
    {
        _terrainManager.isDragEnabled = !_terrainManager.isDragEnabled;

        if (!_terrainManager.isDragEnabled && _terrainManager.isSmartDragEnabled) 
        {
            _terrainManager.isSmartDragEnabled = false;
            smartDragSwitchManager.AnimateSwitch();
        }

        _terrainManager.isValidDrag = false;
        _terrainManager.ToggleTileSelector();
    }

    private void OnSmartDragClicked()
    {
        _terrainManager.isSmartDragEnabled = !_terrainManager.isSmartDragEnabled;

        if (_terrainManager.isSmartDragEnabled && !_terrainManager.isDragEnabled)
        {
            _terrainManager.isDragEnabled = true;
            dragSwitchManager.AnimateSwitch();
        }

        _terrainManager.isValidDrag = false;
        _terrainManager.ToggleTileSelector();
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
