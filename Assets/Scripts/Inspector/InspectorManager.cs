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
    public GameObject gizmoModeButtons;
    public GameObject positionInputs;
    public GameObject rotationInputs;
    public GameObject scaleInputs;

    public bool IsEditingText;


    private PrefabGizmoManager _prefabGizmoManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();

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
                xScaleInput.text = value.z.ToString("F");
                yScaleInput.text = value.z.ToString("F");
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
        float parsedValue = ValidateFloatInput(inputField, transformType);

        _prefabGizmoManager.UpdateTargetTransform(parsedValue, transformType, transformAxis);
    }

    private float ValidateFloatInput(TMP_InputField inputField, TransformType transformType)
    {
        bool isValid = float.TryParse(inputField.text, out float result);
        
        if (!isValid)
        {
            result = 1;
        }

        return result;
    }

    public void ShowUiForTarget(TargetingType targetingType)
    {
        gizmoModeButtons.SetActive(targetingType == TargetingType.Prefab);
        positionInputs.SetActive(targetingType == TargetingType.Prefab);
        rotationInputs.SetActive(targetingType == TargetingType.Prefab);
        scaleInputs.SetActive(targetingType == TargetingType.Prefab);
    }

    private void InitInputFields()
    {
        xPositionInput.onValueChanged.AddListener(delegate { InputFieldUpdated(xPositionInput, TransformType.Position, TransformAxis.X); });
        xPositionInput.onSelect.AddListener(delegate { IsEditingText = true; });
        xPositionInput.onDeselect.AddListener(delegate { IsEditingText = false; });

        yPositionInput.onValueChanged.AddListener(delegate { InputFieldUpdated(yPositionInput, TransformType.Position, TransformAxis.Y); });
        yPositionInput.onSelect.AddListener(delegate { IsEditingText = true; });
        yPositionInput.onDeselect.AddListener(delegate { IsEditingText = false; });

        zPositionInput.onValueChanged.AddListener(delegate { InputFieldUpdated(zPositionInput, TransformType.Position, TransformAxis.Z); });
        zPositionInput.onSelect.AddListener(delegate { IsEditingText = true; });
        zPositionInput.onDeselect.AddListener(delegate { IsEditingText = false; });


        xRotationInput.onValueChanged.AddListener(delegate { InputFieldUpdated(xRotationInput, TransformType.Rotation, TransformAxis.X); });
        xRotationInput.onSelect.AddListener(delegate { IsEditingText = true; });
        xRotationInput.onDeselect.AddListener(delegate { IsEditingText = false; });

        yRotationInput.onValueChanged.AddListener(delegate { InputFieldUpdated(yRotationInput, TransformType.Rotation, TransformAxis.Y); });
        yRotationInput.onSelect.AddListener(delegate { IsEditingText = true; });
        yRotationInput.onDeselect.AddListener(delegate { IsEditingText = false; });

        zRotationInput.onValueChanged.AddListener(delegate { InputFieldUpdated(zRotationInput, TransformType.Rotation, TransformAxis.Z); });
        zRotationInput.onSelect.AddListener(delegate { IsEditingText = true; });
        zRotationInput.onDeselect.AddListener(delegate { IsEditingText = false; });


        xScaleInput.onValueChanged.AddListener(delegate { InputFieldUpdated(xScaleInput, TransformType.Scale, TransformAxis.X); });
        xScaleInput.onSelect.AddListener(delegate { IsEditingText = true; });
        xScaleInput.onDeselect.AddListener(delegate { IsEditingText = false; });

        yScaleInput.onValueChanged.AddListener(delegate { InputFieldUpdated(yScaleInput, TransformType.Scale, TransformAxis.Y); });
        yScaleInput.onSelect.AddListener(delegate { IsEditingText = true; });
        yScaleInput.onDeselect.AddListener(delegate { IsEditingText = false; });

        zScaleInput.onValueChanged.AddListener(delegate { InputFieldUpdated(zScaleInput, TransformType.Scale, TransformAxis.Z); });
        zScaleInput.onSelect.AddListener(delegate { IsEditingText = true; });
        zScaleInput.onDeselect.AddListener(delegate { IsEditingText = false; });
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
