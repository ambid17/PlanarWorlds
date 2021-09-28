using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MeshMapInspector : MonoBehaviour
{
    public ImageTabButton raiseButton;
    public ImageTabButton lowerButton;
    public ImageTabButton setHeightButton;
    public ImageTabButton smoothButton;
    public ImageTabButton paintButton;

    public TMP_InputField brushSizeInput;
    public TMP_InputField brushHeightInput;
    public TMP_InputField brushStrengthInput;

    public GameObject brushSizeContainer;
    public GameObject brushHeightContainer;
    public GameObject brushStrengthContainer;
    public GameObject paintContainer;

    public GameObject paintLayerPrefab;
    public Transform paintLayerParent;

    private TerrainManager _terrainManager;
    private UIManager _uiManager;

    private ImageTabButton _currentLayerButton;

    private VerticalLayoutGroup _myVerticalLayoutGroup;

    private void Awake()
    {
        _terrainManager = TerrainManager.GetInstance();
        _uiManager = UIManager.GetInstance();
        _myVerticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    void Start()
    {
        SetupButtons();
        SetupInputs();
        SetupPaints();
        raiseButton.Select();
        UpdateUI(TerrainModificationMode.Raise);
    }

    private void SetupButtons()
    {
        raiseButton.Setup(() => ChangeTerrainModificationMode(TerrainModificationMode.Raise));
        lowerButton.Setup(() => ChangeTerrainModificationMode(TerrainModificationMode.Lower));
        setHeightButton.Setup(() => ChangeTerrainModificationMode(TerrainModificationMode.SetHeight));
        smoothButton.Setup(() => ChangeTerrainModificationMode(TerrainModificationMode.Smooth));
        paintButton.Setup(() => ChangeTerrainModificationMode(TerrainModificationMode.Paint));

    }

    private void SetupInputs()
    {
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

    private void SetupPaints()
    {
        bool isFirst = true;
        foreach (TerrainLayerTexture layer in _terrainManager.meshMapEditor.terrainLayerTextures.layers)
        {
            GameObject newButton = Instantiate(paintLayerPrefab, paintLayerParent);
            ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();

            Sprite layerSprite = Sprite.Create(layer.diffuse, new Rect(0, 0, layer.diffuse.width, layer.diffuse.height), new Vector2(0.5f, 0.5f));
            tabButton.Setup(layerSprite, () => SetCurrentTile(layer, tabButton));

            if (isFirst)
            {
                tabButton.Select();
                _currentLayerButton = tabButton;
                isFirst = false;
            }
        }
    }

    private void SetCurrentTile(TerrainLayerTexture layer, ImageTabButton tabButton)
    {
        if (_currentLayerButton)
        {
            _currentLayerButton.Unselect();
        }

        _terrainManager.meshMapEditor.TryAddTerrainLayer(layer);
        _currentLayerButton = tabButton;
    }

    private void ChangeTerrainModificationMode(TerrainModificationMode newMode)
    {
        raiseButton.Unselect();
        lowerButton.Unselect();
        setHeightButton.Unselect();
        smoothButton.Unselect();
        paintButton.Unselect();

        _terrainManager.meshMapEditor.SwitchTerrainModificationMode(newMode);
    }

    public void TerrainModificationModeChanged(TerrainModificationMode newMode)
    {
        raiseButton.Unselect();
        lowerButton.Unselect();
        setHeightButton.Unselect();
        smoothButton.Unselect();
        paintButton.Unselect();

        if (newMode == TerrainModificationMode.Raise)
            raiseButton.Select();
        if (newMode == TerrainModificationMode.Lower)
            lowerButton.Select();
        if (newMode == TerrainModificationMode.SetHeight)
            setHeightButton.Select();
        if (newMode == TerrainModificationMode.Smooth)
            smoothButton.Select();
        if (newMode == TerrainModificationMode.Paint)
            paintButton.Select();

        UpdateUI(newMode);
    }

    private void UpdateUI(TerrainModificationMode newMode)
    {
        brushSizeContainer.SetActive(true);
        brushHeightContainer.SetActive(newMode == TerrainModificationMode.SetHeight);
        brushStrengthContainer.SetActive(true);
        paintContainer.SetActive(newMode == TerrainModificationMode.Paint);

        _myVerticalLayoutGroup.CalculateLayoutInputHorizontal();
        _myVerticalLayoutGroup.SetLayoutVertical();
    }

    private void BrushSizeUpdated()
    {
        int validatedSize = InputValidation.ValidateInt(brushSizeInput.text, 1);
        _terrainManager.meshMapEditor.SetBrushSize(validatedSize);
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
