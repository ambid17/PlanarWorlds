using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class TilingInput : MonoBehaviour
{
    public TMP_InputField xTilingInput;
    public TMP_InputField yTilingInput;

    private PrefabGizmoManager _prefabGizmoManager;
    private InspectorManager _inspectorManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        _inspectorManager = InspectorManager.GetInstance();
        InitInputs();
    }

    private void InitInputs()
    {
        xTilingInput.onValueChanged.AddListener(delegate { InputFieldUpdated(); });
        xTilingInput.onSelect.AddListener(delegate { _inspectorManager.IsEditingText = true; });
        xTilingInput.onDeselect.AddListener(delegate { _inspectorManager.IsEditingText = false; });

        yTilingInput.onValueChanged.AddListener(delegate { InputFieldUpdated(); });
        yTilingInput.onSelect.AddListener(delegate { _inspectorManager.IsEditingText = true; });
        yTilingInput.onDeselect.AddListener(delegate { _inspectorManager.IsEditingText = false; });
    }

    private void InputFieldUpdated()
    {
        float xscale = ValidateFloatInput(xTilingInput);
        float yscale = ValidateFloatInput(yTilingInput);

        _prefabGizmoManager.SetTilingForCurrentObject(xscale, yscale);
    }

    public void RefreshInputs()
    {
        GameObject target = _prefabGizmoManager.TargetObject;
        if (target == null)
        {
            return;
        }
        Renderer myRenderer = target.GetComponent<Renderer>();
        Vector2 tiling = myRenderer.material.mainTextureScale;
        xTilingInput.text = tiling.x.ToString();
        yTilingInput.text = tiling.y.ToString();
    }

	private float ValidateFloatInput(TMP_InputField inputField)
    {
        bool isValid = float.TryParse(inputField.text, out float result);

        if (!isValid)
        {
            result = 1;
        }

        return result;
    }
}
