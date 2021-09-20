using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainSettingsUI : MonoBehaviour
{
    public TMP_InputField brushSizeInput;

    public GameObject brushSizeContainer;

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
        InitInputFieldValidation(brushSizeInput);
    }

    private void InitInputFieldValidation(TMP_InputField inputField)
    {
        inputField.onValidateInput += (string input, int charIndex, char addedChar) =>
        {
            return InputValidation.ValidateCharAsUnsignedInt(addedChar);
        };
    }

    private void BrushSizeUpdated()
    {
        int newBrushSize = InputValidation.ValidateInt(text: brushSizeInput.text, defaultValue: Constants.defaultBrushSize);

        _terrainManager.SetBrushSize(newBrushSize);
    }

    public void ToggleSettingsMenu(bool shouldBeActive)
    {
        brushSizeContainer.SetActive(shouldBeActive);
        brushSizeInput.text = _terrainManager.BrushSize.ToString();
    }
}
