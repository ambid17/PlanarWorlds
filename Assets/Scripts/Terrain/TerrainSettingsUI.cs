using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainSettingsUI : MonoBehaviour
{
    public TMP_InputField brushSizeInput;
    public TMP_InputField mapSizeXInput;
    public TMP_InputField mapSizeYInput;
    public Button fillButton;

    public GameObject brushSizeContainer;
    public GameObject mapSizeContainer;
    public GameObject fillContainer;

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
        brushSizeInput.onValueChanged.AddListener(delegate { BrushSizeUpdated(); });
        brushSizeInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        brushSizeInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        mapSizeXInput.onValueChanged.AddListener(delegate { MapSizeUpdated(TransformAxis.X); });
        mapSizeXInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        mapSizeXInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        mapSizeYInput.onValueChanged.AddListener(delegate { MapSizeUpdated(TransformAxis.Y); });
        mapSizeYInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        mapSizeYInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        fillButton.onClick.AddListener(() => _terrainManager.GenerateDefaultMap(false));
    }

    private void BrushSizeUpdated()
    {
        int newBrushSize = InputValidation.ValidateInt(text: brushSizeInput.text, defaultValue: Constants.defaultBrushSize);

        _terrainManager.SetBrushSize(newBrushSize);
    }

    private void MapSizeUpdated(TransformAxis axis)
    {
        string toValidate = axis == TransformAxis.X ? mapSizeXInput.text : mapSizeYInput.text;

        int newMapSize = InputValidation.ValidateInt(text: toValidate, defaultValue: Constants.defaultMapSize);

        _terrainManager.SetMapSize(axis, newMapSize);
    }

    public void ToggleSettingsMenu(bool shouldBeActive)
    {
        brushSizeContainer.SetActive(shouldBeActive);
        mapSizeContainer.SetActive(shouldBeActive);
        fillContainer.SetActive(shouldBeActive);

        brushSizeInput.text = _terrainManager.BrushSize.ToString();
        mapSizeXInput.text = _terrainManager.MapSize.x.ToString();
        mapSizeYInput.text = _terrainManager.MapSize.y.ToString();
    }
}
