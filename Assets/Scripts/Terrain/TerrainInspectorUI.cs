using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TerrainInspectorUI : MonoBehaviour
{
    public ImageTabButton raiseButton;
    public ImageTabButton lowerButton;
    public ImageTabButton setHeightButton;
    public ImageTabButton smoothButton;
    public ImageTabButton paintButton;
    public ImageTabButton treeButton;
    public ImageTabButton foliageButton;

    public TMP_InputField brushSizeInput;
    public TMP_InputField brushHeightInput;
    public TMP_InputField brushStrengthInput;

    public TMP_Text terrainToolText;
    public SliderManager brushSizeSlider;
    public SliderManager brushHeightSlider;
    public SliderManager brushStrengthSlider;
    public SwitchManager eraseSwitch;

    public GameObject brushSizeContainer;
    public GameObject brushHeightContainer;
    public GameObject brushStrengthContainer;
    public GameObject paintContainer;
    public GameObject treeContainer;
    public GameObject foliageContainer;
    public GameObject eraseContainer;

    public GameObject imageButtonPrefab;


    private TerrainManager _terrainManager;
    private UIManager _uiManager;

    private ImageTabButton _currentLayerButton;
    private ImageTabButton _currentTreeButton;
    private ImageTabButton _currentFoliageButton;

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
        SetupSliders();
        SetupPaints();
        SetupTrees();
        SetupFoliage();
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
        treeButton.Setup(() => ChangeTerrainModificationMode(TerrainModificationMode.Trees));
        foliageButton.Setup(() => ChangeTerrainModificationMode(TerrainModificationMode.Foliage));

    }

    private void SetupInputs()
    {
        brushSizeInput.onValueChanged.AddListener((newText) => BrushSizeTextUpdated());
        brushSizeInput.onSelect.AddListener(delegate { _uiManager.isEditingValues = true; });
        brushSizeInput.onDeselect.AddListener(delegate { _uiManager.isEditingValues = false; });
        brushSizeInput.text = "1";

        brushHeightInput.onValueChanged.AddListener((newText) => BrushHeightTextUpdated());
        brushHeightInput.onSelect.AddListener(delegate { _uiManager.isEditingValues = true; });
        brushHeightInput.onDeselect.AddListener(delegate { _uiManager.isEditingValues = false; });
        brushHeightInput.text = "0";

        brushStrengthInput.onValueChanged.AddListener((newText) => BrushStrengthTextUpdated());
        brushStrengthInput.onSelect.AddListener(delegate { _uiManager.isEditingValues = true; });
        brushStrengthInput.onDeselect.AddListener(delegate { _uiManager.isEditingValues = false; });
        brushStrengthInput.text = "0.01";
        
        eraseSwitch.OnEvents.AddListener(EraseToggled);
        eraseSwitch.OffEvents.AddListener(EraseToggled);
    }

    private void SetupSliders()
    {
        brushSizeSlider.mainSlider.wholeNumbers = true;
        brushSizeSlider.mainSlider.minValue = 1;
        brushSizeSlider.mainSlider.maxValue = 40;
        brushSizeSlider.mainSlider.value = 1;
        brushSizeSlider.mainSlider.onValueChanged.AddListener( BrushSizeSliderUpdated );
        
        brushHeightSlider.mainSlider.wholeNumbers = true;
        brushHeightSlider.mainSlider.minValue = 1;
        brushHeightSlider.mainSlider.maxValue = 40;
        brushHeightSlider.mainSlider.value = 1;
        brushHeightSlider.mainSlider.onValueChanged.AddListener( BrushHeightSliderUpdated );
        
        brushStrengthSlider.mainSlider.wholeNumbers = false;
        brushStrengthSlider.mainSlider.minValue = 0;
        brushStrengthSlider.mainSlider.maxValue = 1;
        brushStrengthSlider.mainSlider.value = 0.1f;
        brushStrengthSlider.mainSlider.onValueChanged.AddListener( BrushStrengthSliderUpdated );
    }
    
    private void BrushSizeTextUpdated()
    {
        int validatedSize = InputValidation.ValidateInt(brushSizeInput.text, 1);
        brushSizeSlider.mainSlider.value = validatedSize;
        _terrainManager.terrainEditor.SetBrushSize(validatedSize);
    }
    
    private void BrushSizeSliderUpdated(float sliderValue)
    {
        int newSize = Mathf.RoundToInt(sliderValue);
        brushSizeInput.text = newSize.ToString();
        _terrainManager.terrainEditor.SetBrushSize(newSize);
    }

    private void BrushHeightTextUpdated()
    {
        float validatedHeight = InputValidation.ValidateFloat(brushHeightInput.text, 0);
        brushHeightSlider.mainSlider.value = validatedHeight;
        _terrainManager.terrainEditor.SetBrushHeight(validatedHeight);
    }
    
    private void BrushHeightSliderUpdated(float sliderValue)
    {
        brushHeightInput.text = sliderValue.ToString();
        _terrainManager.terrainEditor.SetBrushHeight(sliderValue);
    }

    private void BrushStrengthTextUpdated()
    {
        float validatedStrength = InputValidation.ValidateFloat(brushStrengthInput.text, 0);
        brushStrengthSlider.mainSlider.value = validatedStrength;
        _terrainManager.terrainEditor.brushStrength = validatedStrength;
    }
    
    private void BrushStrengthSliderUpdated(float sliderValue)
    {
        brushStrengthInput.text = sliderValue.ToString();
        _terrainManager.terrainEditor.brushStrength = sliderValue;
    }

    private void EraseToggled()
    {
        _terrainManager.terrainEditor.isErasing = eraseSwitch.isOn;
    }

    private void SetupPaints()
    {
        bool isFirst = true;
        foreach (TerrainLayerTexture layer in _terrainManager.terrainLayers.layers)
        {
            GameObject newButton = Instantiate(imageButtonPrefab, paintContainer.transform);
            ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();

            Sprite layerSprite = Sprite.Create(layer.diffuse, new Rect(0, 0, layer.diffuse.width, layer.diffuse.height), new Vector2(0.5f, 0.5f));
            tabButton.Setup(layerSprite, () => SetCurrentTerrainLayer(layer, tabButton));

            if (isFirst)
            {
                tabButton.Select();
                _currentLayerButton = tabButton;
                isFirst = false;
            }
        }
    }

    private void SetupTrees()
    {
        bool isFirst = true;
        foreach (Prefab prefab in _terrainManager.treePrefabList.prefabs)
        {
            GameObject newButton = Instantiate(imageButtonPrefab, treeContainer.transform);
            ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();

            Sprite layerSprite = Sprite.Create(prefab.previewTexture, new Rect(0, 0, prefab.previewTexture.width, prefab.previewTexture.height), new Vector2(0.5f, 0.5f));
            tabButton.Setup(layerSprite, () => SetCurrentTree(prefab.gameObject, tabButton));

            if (isFirst)
            {
                tabButton.Select();
                _currentTreeButton = tabButton;
                isFirst = false;
            }
        }
    }

    private void SetupFoliage()
    {
        bool isFirst = true;
        foreach (Prefab prefab in _terrainManager.foliagePrefabList.prefabs)
        {
            GameObject newButton = Instantiate(imageButtonPrefab, foliageContainer.transform);
            ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();

            Sprite layerSprite = Sprite.Create(prefab.previewTexture, new Rect(0, 0, prefab.previewTexture.width, prefab.previewTexture.height), new Vector2(0.5f, 0.5f));
            tabButton.Setup(layerSprite, () => SetCurrentFoliage(prefab.gameObject, tabButton));

            if (isFirst)
            {
                tabButton.Select();
                _currentFoliageButton = tabButton;
                isFirst = false;
            }
        }
    }

    private void SetCurrentTerrainLayer(TerrainLayerTexture layer, ImageTabButton tabButton)
    {
        if (_currentLayerButton)
        {
            _currentLayerButton.Unselect();
        }

        _terrainManager.terrainEditor.SetTerrainLayer(layer);
        _currentLayerButton = tabButton;
    }

    private void SetCurrentTree(GameObject prefab, ImageTabButton tabButton)
    {
        if (_currentTreeButton)
        {
            _currentTreeButton.Unselect();
        }

        _terrainManager.terrainEditor.SetTree(prefab);
        _currentTreeButton = tabButton;
    }

    private void SetCurrentFoliage(GameObject prefab, ImageTabButton tabButton)
    {
        if (_currentFoliageButton)
        {
            _currentFoliageButton.Unselect();
        }

        _terrainManager.terrainEditor.SetDetailMesh(prefab);
        _currentFoliageButton = tabButton;
    }

    private void ChangeTerrainModificationMode(TerrainModificationMode newMode)
    {
        raiseButton.Unselect();
        lowerButton.Unselect();
        setHeightButton.Unselect();
        smoothButton.Unselect();
        paintButton.Unselect();
        treeButton.Unselect();
        foliageButton.Unselect();

        _terrainManager.terrainEditor.SwitchTerrainModificationMode(newMode);

        switch (newMode)
        {
            case TerrainModificationMode.Foliage:
                terrainToolText.text = "Foliage";
                break;
            case TerrainModificationMode.Lower:
                terrainToolText.text = "Lower Terrain";
                break;
            case TerrainModificationMode.Raise:
                terrainToolText.text = "Raise Terrain";
                break;
            case TerrainModificationMode.Trees:
                terrainToolText.text = "Trees";
                break;
            case TerrainModificationMode.Smooth:
                terrainToolText.text = "Smooth Terrain";
                break;
            case TerrainModificationMode.SetHeight:
                terrainToolText.text = "Set Height";
                break;
            case TerrainModificationMode.Paint:
                terrainToolText.text = "Paint Terrain";
                break;
        }
    }

    public void TerrainModificationModeChanged(TerrainModificationMode newMode)
    {
        raiseButton.Unselect();
        lowerButton.Unselect();
        setHeightButton.Unselect();
        smoothButton.Unselect();
        paintButton.Unselect();
        treeButton.Unselect();
        foliageButton.Unselect();

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
        if (newMode == TerrainModificationMode.Trees)
            treeButton.Select();
        if (newMode == TerrainModificationMode.Foliage)
            foliageButton.Select();

        UpdateUI(newMode);
    }

    private void UpdateUI(TerrainModificationMode newMode)
    {
        brushSizeContainer.SetActive(true);
        brushHeightContainer.SetActive(newMode == TerrainModificationMode.SetHeight);
        brushStrengthContainer.SetActive(newMode != TerrainModificationMode.SetHeight);
        paintContainer.SetActive(newMode == TerrainModificationMode.Paint);
        treeContainer.SetActive(newMode == TerrainModificationMode.Trees);
        foliageContainer.SetActive(newMode == TerrainModificationMode.Foliage);
        eraseContainer.SetActive(newMode == TerrainModificationMode.Trees || newMode == TerrainModificationMode.Foliage);
    }
}
