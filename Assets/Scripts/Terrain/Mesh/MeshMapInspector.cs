using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeshMapInspector : MonoBehaviour
{
    public TabButton raiseButton;
    public TabButton lowerButton;
    public TabButton flattenButton;

    public TMP_InputField brushSizeInput;
    public TMP_InputField brushHeightInput;
    public TMP_InputField brushStrengthInput;

    private TerrainManager _terrainManager;
    private UIManager _uiManager;

    private void Awake()
    {
        _terrainManager = TerrainManager.GetInstance();
        _uiManager = UIManager.GetInstance();
    }

    void Start()
    {
        raiseButton.SetupAction(() => ChangeTerrainModificationMode(TerrainModificationMode.Raise));
        lowerButton.SetupAction(() => ChangeTerrainModificationMode(TerrainModificationMode.Lower));
        flattenButton.SetupAction(() => ChangeTerrainModificationMode(TerrainModificationMode.Flatten));

        raiseButton.Select();

        brushSizeInput.onValueChanged.AddListener((newText) => BrushSizeUpdated());
        brushSizeInput.onSelect.AddListener(delegate { _uiManager.isEditingValues = true; });
        brushSizeInput.onDeselect.AddListener(delegate { _uiManager.isEditingValues = false; });
        brushSizeInput.text = "1";

        brushHeightInput.onValueChanged.AddListener((newText) => BrushHeightUpdated());
        brushHeightInput.onSelect.AddListener(delegate { _uiManager.isEditingValues = true; });
        brushHeightInput.onDeselect.AddListener(delegate { _uiManager.isEditingValues = false; });
        brushHeightInput.text = "0";

        brushStrengthInput.onValueChanged.AddListener((newText) => BrushStrengthUpdated());
        brushStrengthInput.onSelect.AddListener(delegate { _uiManager.isEditingValues = true; });
        brushStrengthInput.onDeselect.AddListener(delegate { _uiManager.isEditingValues = false; });
        brushStrengthInput.text = "0.01";
    }

    void Update()
    {
        
    }

    private void ChangeTerrainModificationMode(TerrainModificationMode newMode)
    {
        raiseButton.Unselect();
        lowerButton.Unselect();
        flattenButton.Unselect();

        _terrainManager.meshMapEditor.SwitchTerrainModificationMode(newMode);
    }

    public void TerrainModificationModeChanged(TerrainModificationMode newMode)
    {
        raiseButton.Unselect();
        lowerButton.Unselect();
        flattenButton.Unselect();

        if (newMode == TerrainModificationMode.Raise)
            raiseButton.Select();
        if (newMode == TerrainModificationMode.Lower)
            lowerButton.Select();
        if (newMode == TerrainModificationMode.Flatten)
            flattenButton.Select();
    }

    private void BrushSizeUpdated()
    {
        int validatedSize = InputValidation.ValidateInt(brushSizeInput.text, 1);
        _terrainManager.meshMapEditor.brushSize = validatedSize;
    }

    private void BrushHeightUpdated()
    {
        float validatedHeight = InputValidation.ValidateFloat(brushHeightInput.text, 0);
        _terrainManager.meshMapEditor.SetBrushHeight(validatedHeight);
    }

    private void BrushStrengthUpdated()
    {
        float validatedStrength = InputValidation.ValidateFloat(brushStrengthInput.text, 0);
        _terrainManager.meshMapEditor.brushStrength = validatedStrength;
    }
}
