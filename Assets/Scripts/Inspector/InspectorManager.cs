using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorManager : StaticMonoBehaviour<InspectorManager>
{
    public ToggleButton positionButton;
    public ToggleButton rotationButton;
    public ToggleButton scaleButton;

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


    private PrefabGizmoManager _prefabGizmoManager;
    private UIManager _uIManager;

    private string multiSelectMismatch = "-";

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        _uIManager = UIManager.GetInstance();

        InitInputFields();

        positionButton.SetupAction(() => GizmoButtonClicked(TransformType.Position));
        rotationButton.SetupAction(() => GizmoButtonClicked(TransformType.Rotation));
        scaleButton.SetupAction(() => GizmoButtonClicked(TransformType.Scale));
        positionButton.Select();

        ShowUiForTargetType(TargetingType.None);
    }

    public void UpdateInputFields()
    {
        if (_prefabGizmoManager.TargetObjects.Count > 0)
        {
            // A lot of the time, due to floating point precision, the values can be like 8E-6, instead of 0
            // So we need to round to account for that
            RoundTargetObjectTransforms();
            Transform first = _prefabGizmoManager.TargetObjects.First().transform;

            if(!_prefabGizmoManager.TargetObjects.All(go => go.transform.position.x == first.transform.position.x))
                xPositionInput.text = multiSelectMismatch;
            else
                xPositionInput.text = first.position.x.ToString("F2");

            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.position.y == first.transform.position.y))
                yPositionInput.text = multiSelectMismatch;
            else
                yPositionInput.text = first.position.y.ToString("F2");

            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.position.z == first.transform.position.z))
                zPositionInput.text = multiSelectMismatch;
            else
                zPositionInput.text = first.position.z.ToString("F2");

            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.rotation.x == first.transform.rotation.x))
                xRotationInput.text = multiSelectMismatch;
            else
                xRotationInput.text = first.rotation.eulerAngles.x.ToString("##0");

            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.rotation.y == first.transform.rotation.y))
                yRotationInput.text = multiSelectMismatch;
            else
                yRotationInput.text = first.rotation.eulerAngles.y.ToString("##0");

            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.rotation.z == first.transform.rotation.z))
                zRotationInput.text = multiSelectMismatch;
            else
                zRotationInput.text = first.rotation.eulerAngles.z.ToString("##0");


            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.localScale.x == first.transform.localScale.x))
                xScaleInput.text = multiSelectMismatch;
            else
                xScaleInput.text = first.localScale.x.ToString("F2");
            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.localScale.y == first.transform.localScale.y))
                yScaleInput.text = multiSelectMismatch;
            else
                yScaleInput.text = first.localScale.y.ToString("F2");

            if (!_prefabGizmoManager.TargetObjects.All(go => go.transform.localScale.z == first.transform.localScale.z))
                zScaleInput.text = multiSelectMismatch;
            else
                zScaleInput.text = first.localScale.z.ToString("F2");
        }
    }

    private void RoundTargetObjectTransforms()
    {
        foreach(GameObject go in _prefabGizmoManager.TargetObjects)
        {
            Transform tf = go.transform;
            Vector3 newPosition = InputValidation.Round(tf.position, 2);
            Vector3 newRotation = InputValidation.Round(tf.rotation.eulerAngles, 2);
            Vector3 newScale = InputValidation.Round(tf.localScale, 2);

            go.transform.position = newPosition;
            go.transform.rotation = Quaternion.Euler(newRotation);
            go.transform.localScale = newScale;
        }
    }

    private void GizmoButtonClicked(TransformType transformType)
    {
        _prefabGizmoManager.ChangeGimzoMode(transformType);

        positionButton.Unselect();
        rotationButton.Unselect();
        scaleButton.Unselect();
    }

    public void GizmoModeChanged(TransformType transformType)
    {
        positionButton.Unselect();
        rotationButton.Unselect();
        scaleButton.Unselect();

        if(transformType == TransformType.Position)
            positionButton.Select();

        if (transformType == TransformType.Rotation)
            rotationButton.Select();

        if (transformType == TransformType.Scale)
            scaleButton.Select();
    }

    private void InputFieldUpdated(TMP_InputField inputField, TransformType transformType, TransformAxis transformAxis)
    {
        if(inputField.text == multiSelectMismatch)
        {
            return;
        }

        float parsedValue = InputValidation.ValidateFloat(text: inputField.text, defaultValue: 1);

        _prefabGizmoManager.UpdateTargetTransforms(parsedValue, transformType, transformAxis);
    }

    public void ShowUiForTargetType(TargetingType targetingType)
    {
        objectInspectorParent.SetActive(targetingType == TargetingType.Prefab);
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
