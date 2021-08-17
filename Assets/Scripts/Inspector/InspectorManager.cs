using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorManager : StaticMonoBehaviour<InspectorManager>
{
    public Button positionButton;
    public Button rotationButton;
    public Button scaleButton;

    public TMP_InputField xPositionInput;
    public TMP_InputField yPositionInput;
    public TMP_InputField zPositionInput;

    public TMP_InputField xRotationInput;
    public TMP_InputField yRotationInput;
    public TMP_InputField zRotationInput;

    public TMP_InputField xScaleInput;
    public TMP_InputField yScaleInput;
    public TMP_InputField zScaleInput;

    // Containers
    public GameObject objectInspectorParent;
    public GameObject terrainInspectorParent;


    private PrefabGizmoManager _prefabGizmoManager;
    private UIManager _uIManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        _uIManager = UIManager.GetInstance();

        InitInputFields();
        positionButton.onClick.AddListener(() => GizmoButtonClicked(TransformType.Position));
        rotationButton.onClick.AddListener(() => GizmoButtonClicked(TransformType.Rotation));
        scaleButton.onClick.AddListener(() => GizmoButtonClicked(TransformType.Scale));

        ShowUiForTarget(TargetingType.None);
    }

    public void UpdateInputFields(TransformType transformType, Vector3 value)
    {
        switch (transformType)
        {
            case TransformType.Position:
                xPositionInput.text = value.x.ToString("F");
                yPositionInput.text = value.y.ToString("F");
                zPositionInput.text = value.z.ToString("F");
                break;
            case TransformType.Rotation:
                xRotationInput.text = value.x.ToString("##0");
                yRotationInput.text = value.y.ToString("##0");
                zRotationInput.text = value.z.ToString("##0");
                break;
            case TransformType.Scale:
                xScaleInput.text = value.x.ToString("F");
                yScaleInput.text = value.y.ToString("F");
                zScaleInput.text = value.z.ToString("F");
                break;
        }
    }

    private void GizmoButtonClicked(TransformType transformType)
    {
        _prefabGizmoManager.ChangeGimzoMode(transformType);
    }

    private void InputFieldUpdated(TMP_InputField inputField, TransformType transformType, TransformAxis transformAxis)
    {
        float parsedValue = InputValidation.ValidateFloat(text: inputField.text, defaultValue: 1);

        _prefabGizmoManager.UpdateTargetTransform(parsedValue, transformType, transformAxis);
    }

    public void ShowUiForTarget(TargetingType targetingType)
    {
        objectInspectorParent.SetActive(targetingType == TargetingType.Prefab);
        terrainInspectorParent.SetActive(targetingType == TargetingType.Terrain);
    }

    private void InitInputFields()
    {
        xPositionInput.onValueChanged.AddListener(delegate { InputFieldUpdated(xPositionInput, TransformType.Position, TransformAxis.X); });
        xPositionInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        xPositionInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        yPositionInput.onValueChanged.AddListener(delegate { InputFieldUpdated(yPositionInput, TransformType.Position, TransformAxis.Y); });
        yPositionInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        yPositionInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        zPositionInput.onValueChanged.AddListener(delegate { InputFieldUpdated(zPositionInput, TransformType.Position, TransformAxis.Z); });
        zPositionInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        zPositionInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });


        xRotationInput.onValueChanged.AddListener(delegate { InputFieldUpdated(xRotationInput, TransformType.Rotation, TransformAxis.X); });
        xRotationInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        xRotationInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        yRotationInput.onValueChanged.AddListener(delegate { InputFieldUpdated(yRotationInput, TransformType.Rotation, TransformAxis.Y); });
        yRotationInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        yRotationInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        zRotationInput.onValueChanged.AddListener(delegate { InputFieldUpdated(zRotationInput, TransformType.Rotation, TransformAxis.Z); });
        zRotationInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        zRotationInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });


        xScaleInput.onValueChanged.AddListener(delegate { InputFieldUpdated(xScaleInput, TransformType.Scale, TransformAxis.X); });
        xScaleInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        xScaleInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        yScaleInput.onValueChanged.AddListener(delegate { InputFieldUpdated(yScaleInput, TransformType.Scale, TransformAxis.Y); });
        yScaleInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        yScaleInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });

        zScaleInput.onValueChanged.AddListener(delegate { InputFieldUpdated(zScaleInput, TransformType.Scale, TransformAxis.Z); });
        zScaleInput.onSelect.AddListener(delegate { _uIManager.isEditingValues = true; });
        zScaleInput.onDeselect.AddListener(delegate { _uIManager.isEditingValues = false; });
    }
}

public enum TransformType
{
    Position,
    Rotation,
    Scale
}

public enum TransformAxis
{
    X,
    Y,
    Z
}
