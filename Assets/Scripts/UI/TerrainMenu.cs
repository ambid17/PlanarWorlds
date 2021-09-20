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
    private TerrainManager _terrainManager;

    private void Awake()
    {
        _uIManager = UIManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();
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
        _terrainManager.isValidDrag = false;
    }

    private void OnSmartDragClicked()
    {

    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
