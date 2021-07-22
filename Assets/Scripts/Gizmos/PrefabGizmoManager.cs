using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RTG;

public enum TargetingType
{
    Prefab, None
}

public class PrefabGizmoManager : StaticMonoBehaviour<PrefabGizmoManager>
{
    [SerializeField]
    private LayerMask layerMask;

    private InspectorManager _inspectorManager;
    private PrefabManager _prefabManager;

    private ObjectTransformGizmo positionGizmo;
    private ObjectTransformGizmo rotationGizmo;
    private ObjectTransformGizmo scaleGizmo;
    private ObjectTransformGizmo activeGizmo;


    #region State management variables
    private TransformType currentTransformType;
    private Color currentColor;

    private GameObject _targetObject;
    public GameObject TargetObject { get => _targetObject; }

    private bool isHoldingControl;

    private TargetingType _currentTargetingType;
    public TargetingType CurrentTargetingType { get => _currentTargetingType; }
    #endregion

    void Start()
    {
        _inspectorManager = InspectorManager.GetInstance();
        _prefabManager = PrefabManager.GetInstance();

        // TODO enable snapping setup
        positionGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        positionGizmo.Gizmo.PostDragEnd += OnGizmoPostDragEnd;
        positionGizmo.Gizmo.PostDragUpdate += OnGizmoPostDragUpdate;
        positionGizmo.Gizmo.SetEnabled(false);

        rotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        rotationGizmo.Gizmo.PostDragEnd += OnGizmoPostDragEnd;
        rotationGizmo.Gizmo.PostDragUpdate += OnGizmoPostDragUpdate;
        rotationGizmo.Gizmo.SetEnabled(false);

        scaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        scaleGizmo.Gizmo.PostDragEnd += OnGizmoPostDragEnd;
        scaleGizmo.Gizmo.PostDragUpdate += OnGizmoPostDragUpdate;
        scaleGizmo.Gizmo.PostDragBegin += OnGizmoPostDragBegin;
        scaleGizmo.Gizmo.SetEnabled(false);

        activeGizmo = positionGizmo;

        _currentTargetingType = TargetingType.None;
    }

    void Update()
    {
        TrySelectObject();

        // Don't use any hotkeys while using a text field
        if (!_inspectorManager.IsEditingText)
        {
            TryChangeMode();
            TryDuplicate();
            TryDelete();
            TryFocusObject();
        }
    }

    private void TrySelectObject()
    {
        if (Input.GetMouseButtonDown(0)
            && RTGizmosEngine.Get.HoveredGizmo == null
            && !EventSystem.current.IsPointerOverGameObject()) // only try a raycast if the mouse isn't over UI
        {
            GameObject pickedObject = null;

            // Build a ray using the current mouse cursor position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            // Check if the ray intersects a game object. If it does, return it
            if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, layerMask))
            {
                pickedObject = rayHit.collider.gameObject;
            }


            if (pickedObject != _targetObject)
            {
                // Some of the assets might be made up of children meshes, if so, grab the parent
                if(pickedObject && pickedObject.layer == Constants.PrefabChildLayer)
                {
                    pickedObject = pickedObject.transform.parent.gameObject;
                }
                OnTargetObjectChanged(pickedObject);
            }
        }
    }

    public void OnTargetObjectChanged(GameObject newTargetObject)
    {
        ToggleOutlineRender(false);
        _targetObject = newTargetObject;

        _currentTargetingType = TargetingType.None;

        if (_targetObject != null)
        {
            _currentTargetingType = TargetingType.Prefab;

            activeGizmo.SetTargetObject(_targetObject);

            // Access the move gizmo behaviour and specify the vertex snap target objects
            MoveGizmo moveGizmo = positionGizmo.Gizmo.MoveGizmo;

            // Vertex snapping won't function properly if the meshes are children of an empty parent
            List<GameObject> snapTargets = new List<GameObject>() { _targetObject };
            moveGizmo.SetVertexSnapTargetObjects(snapTargets);

            ToggleOutlineRender(true);
        }

        _inspectorManager.ShowUiForTarget(_currentTargetingType);
        ToggleGizmos();
        UpdateAllInputFields();
    }

    private void ToggleOutlineRender(bool shouldRender)
    {
        if (_targetObject != null)
        {
            Renderer myRenderer = _targetObject.GetComponent<Renderer>();

            if (myRenderer)
            {
                myRenderer.material.SetFloat("_OutlineWidth", shouldRender ? 1.015f : 1f);
            }
        }
    }

    private void TryChangeMode()
    {
        if (Input.GetKeyDown(Constants.positionHotkey))
        {
            ChangeGimzoMode(TransformType.Position);
        }

        if (Input.GetKeyDown(Constants.rotationHotkey))
        {
            ChangeGimzoMode(TransformType.Rotation);
        }

        if (Input.GetKeyDown(Constants.scaleHotkey))
        {
            ChangeGimzoMode(TransformType.Scale);
        }
    }

    public void ChangeGimzoMode(TransformType gizmoMode)
    {
        currentTransformType = gizmoMode;

        switch (gizmoMode)
        {
            case TransformType.Position:
                activeGizmo = positionGizmo;
                break;
            case TransformType.Rotation:
                activeGizmo = rotationGizmo;
                break;
            case TransformType.Scale:
                activeGizmo = scaleGizmo;
                break;
        }

        if (_targetObject != null)
        {
            activeGizmo.SetTargetObject(_targetObject);
        }

        ToggleGizmos();
    }

    private void ToggleGizmos()
    {
        positionGizmo.Gizmo.SetEnabled(_targetObject != null && currentTransformType == TransformType.Position);
        rotationGizmo.Gizmo.SetEnabled(_targetObject != null && currentTransformType == TransformType.Rotation);
        scaleGizmo.Gizmo.SetEnabled(_targetObject != null && currentTransformType == TransformType.Scale);
    }

    private void TryDuplicate()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isHoldingControl = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isHoldingControl = false;
        }

        if (isHoldingControl && Input.GetKeyDown(KeyCode.D) && _targetObject != null)
        {
            GameObject duplicateObject = Instantiate(_targetObject);
            duplicateObject.transform.position = _targetObject.transform.position;
            duplicateObject.transform.rotation = _targetObject.transform.rotation;
            duplicateObject.transform.localScale = _targetObject.transform.localScale;
            duplicateObject.layer = Constants.PrefabParentLayer;
            duplicateObject.transform.parent = _targetObject.transform.parent;

            OnTargetObjectChanged(duplicateObject);
        }
    }

    void TryDelete()
    {
        if(Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
        {
            if (_targetObject != null) 
            {
                Destroy(_targetObject);
                OnTargetObjectChanged(null);
            }
        }
    }

    private void TryFocusObject()
    {
        if(Input.GetKeyDown(KeyCode.F) && _targetObject != null)
        {
            RTFocusCamera.Get.Focus(new List<GameObject>() { _targetObject });
        }
    }

    void OnGizmoPostDragBegin(Gizmo gizmo, int handleId)
    {
        
    }

    void OnGizmoPostDragEnd(Gizmo gizmo, int handleId)
    {

    }

    void OnGizmoPostDragUpdate(Gizmo gizmo, int handleId)
    {
        Vector3 value = new Vector3();

        switch (currentTransformType)
        {
            case TransformType.Position:
                value = _targetObject.transform.position;
                break;
            case TransformType.Rotation:
                value = _targetObject.transform.rotation.eulerAngles;
                break;
            case TransformType.Scale:
                ValidateScale();
                value = _targetObject.transform.localScale;
                break;
        }

        _inspectorManager.UpdateInputFields(currentTransformType, value);
    }

    private void ValidateScale()
    {
        Vector3 newScale = _targetObject.transform.localScale;
        Vector3 minimumScale = new Vector3(0.1f, 0.1f, 0.1f);

        _targetObject.transform.localScale = Vector3.Max(newScale, minimumScale);
    }

    private void UpdateAllInputFields()
    {
        if (_targetObject == null)
            return;

        Vector3 value = _targetObject.transform.position;
        _inspectorManager.UpdateInputFields(TransformType.Position, value);

        value = _targetObject.transform.rotation.eulerAngles;
        _inspectorManager.UpdateInputFields(TransformType.Rotation, value);

        value = _targetObject.transform.localScale;
        _inspectorManager.UpdateInputFields(TransformType.Scale, value);
    }

    public void UpdateTargetTransform(float value, TransformType transformType, TransformAxis transformAxis)
    {
        if(_targetObject == null)
            return;

        Vector3 newPosition = _targetObject.transform.position;
        Vector3 newRotation = _targetObject.transform.eulerAngles;
        Vector3 newScale = _targetObject.transform.localScale;

        switch (transformType)
        {
            case TransformType.Position when transformAxis == TransformAxis.X:
                newPosition.x = value;
                break;
            case TransformType.Position when transformAxis == TransformAxis.Y:
                newPosition.y = value;
                break;
            case TransformType.Position when transformAxis == TransformAxis.Z:
                newPosition.z = value;
                break;
            case TransformType.Rotation when transformAxis == TransformAxis.X:
                newRotation.x = value;
                break;
            case TransformType.Rotation when transformAxis == TransformAxis.Y:
                newRotation.y = value;
                break;
            case TransformType.Rotation when transformAxis == TransformAxis.Z:
                newRotation.z = value;
                break;
            case TransformType.Scale when transformAxis == TransformAxis.X:
                newScale.x = value;
                break;
            case TransformType.Scale when transformAxis == TransformAxis.Y:
                newScale.y = value;
                break;
            case TransformType.Scale when transformAxis == TransformAxis.Z:
                newScale.z = value;
                break;
        }

        _targetObject.transform.position = newPosition;
        _targetObject.transform.rotation = Quaternion.Euler(newRotation);
        _targetObject.transform.localScale = newScale;
    }
}
