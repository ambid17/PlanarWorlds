using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using RTG;

public enum TargetingType
{
    Prefab, PrefabPlacement, None
}

public class PrefabGizmoManager : StaticMonoBehaviour<PrefabGizmoManager>
{
    [SerializeField]
    private LayerMask layerMask;

    private InspectorManager _inspectorManager;
    private TerrainManager _terrainManager;
    private UIManager _uiManager;

    private ObjectTransformGizmo positionGizmo;
    private ObjectTransformGizmo rotationGizmo;
    private ObjectTransformGizmo scaleGizmo;
    private ObjectTransformGizmo activeGizmo;

    #region State management variables
    private TransformType currentTransformType;
    private Color currentColor;

    private List<GameObject> _targetObjects;
    public List<GameObject> TargetObjects { get => _targetObjects; }

    [SerializeField]
    private TargetingType _currentTargetingType;
    public TargetingType CurrentTargetingType { get => _currentTargetingType; }

    private Camera mainCamera;
    #endregion

    void Start()
    {
        mainCamera = Camera.main;

        _inspectorManager = InspectorManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();
        _uiManager = UIManager.GetInstance();

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
        ToggleZoomAbility();

        UIManager.OnEditModeChanged += EditModeChanged;

        _targetObjects = new List<GameObject>();
    }

    void Update()
    {
        if (_uiManager.EditMode != EditMode.Prefab || _uiManager.isEditingValues || _uiManager.isPaused)
            return;

        // We need to do this first, otherwise the targetingType may change and this could get called in the same frame
        if (_currentTargetingType != TargetingType.PrefabPlacement)
        {
            TrySelectObject();
        }
        else if (_currentTargetingType == TargetingType.PrefabPlacement)
        {
            TryPlacePrefab();
            TryRotateObject();
            TryCancelPrefabPlacement();
        }

        TryHideMouse();

        if(_currentTargetingType == TargetingType.Prefab)
        {
            TryChangeMode();
            TryDuplicate();
            TryDelete();
            TryFocusObject();
        }
    }

    #region Prefab Selection
    private void TrySelectObject()
    {
        if (DidValidClick())
        {
            GameObject selectedObject = null;

            // Build a ray using the current mouse cursor position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Check if the ray intersects a game object. If it does, return it
            if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, layerMask))
            {
                selectedObject = rayHit.collider.gameObject;
            }
            
            if (!_targetObjects.Contains(selectedObject))
            {
                OnTargetObjectChanged(selectedObject, false);
            }
        }
    }

    public void OnTargetObjectChanged(GameObject newTargetObject, bool shouldClearMultiSelect)
    {
        bool isMultiSelecting = Input.GetKey(KeyCode.LeftShift);

        ToggleOutlineRender(false);

        if(newTargetObject == null)
        {
            _targetObjects.Clear();
        }

        if (shouldClearMultiSelect && !isMultiSelecting)
        {
            _targetObjects.Clear();
        }
        

        _currentTargetingType = TargetingType.None;

        if (newTargetObject != null)
        {
            _targetObjects.Add(newTargetObject);

            if (newTargetObject.layer == Constants.PrefabParentLayer)
            {
                _currentTargetingType = TargetingType.Prefab;
            }
            else if (newTargetObject.layer == Constants.PrefabPlacementLayer)
            {
                _currentTargetingType = TargetingType.PrefabPlacement;
            }

            ToggleOutlineRender(true);
            _inspectorManager.UpdateInputFields();
        }

        _inspectorManager.ShowUiForTargetType(_currentTargetingType);
        ToggleGizmos();
        ToggleZoomAbility();
    }

    private void ToggleOutlineRender(bool shouldRender)
    {
        if (_targetObjects.Count > 0)
        {
            foreach(GameObject _targetObject in _targetObjects)
            {
                Renderer[] renderers = _targetObject.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.materials;
                    foreach (Material material in materials)
                    {
                        material.SetFloat("_OutlineWidth", shouldRender ? 1.015f : 0);
                    }
                }
            }
        }
    }
    #endregion

    #region Prefab Creation
    private void TryCancelPrefabPlacement()
    {
        // when 'holding' a prefab, but before placement we need to add right click or escape to discontinue placing the prefab
        if (Input.GetMouseButtonDown(1))
		{
            CancelPrefabPlacement();
        }
    }

    private void CancelPrefabPlacement()
    {
        // destroy the current object, set target object to null, call on target change
        if (_targetObjects.Count > 0)
        {
            foreach (GameObject go in _targetObjects)
            {
                Destroy(go);
            }
            OnTargetObjectChanged(null, true);
        }
    }

	private void TryPlacePrefab()
    {
        if (_uiManager.isEditingValues)
            return;

        UpdatePrefabPosition();

        if (Input.GetMouseButtonDown(0) )
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                CancelPrefabPlacement();
                return;
            }
            List<GameObject> newTargets = Duplicate();

            // Change the non-duplicated object's layer so it can be targeted
            _targetObjects[0].layer = Constants.PrefabParentLayer;

            // THEN change the target object, so we keep the PrefabPlacementLayer
            OnTargetObjectChanged(newTargets[0], true);
        }
    }

    private void UpdatePrefabPosition()
    {
        if (_targetObjects.Count == 0)
            return;

        // Build a ray using the current mouse cursor position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Check if the ray intersects the tilemap. If it does, snap the object to the terrain
        if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, _terrainManager.terrainLayerMask))
        {
            BoxCollider myCollider = _targetObjects[0].GetComponent<BoxCollider>();

            Vector3 snappedPosition = rayHit.point;

            if (myCollider)
            {
                // TODO: we can possibly take this out, doesn't seem to be necessary with logans models
                //snappedPosition.y += myCollider.bounds.size.y / 2; // add half of my height
            }

            _targetObjects[0].transform.position = snappedPosition;
        }
    }

    private void TryRotateObject()
    {
        if (_targetObjects.Count == 0)
            return;

        Vector2 mouseScrollDelta = Input.mouseScrollDelta;

        Vector3 rotation = _targetObjects[0].transform.rotation.eulerAngles;
        if (mouseScrollDelta.y > 0)
        {
            rotation.y += Constants.rotationSpeed;
        }
        else if(mouseScrollDelta.y < 0)
        {
            rotation.y -= Constants.rotationSpeed;
        }

        _targetObjects[0].transform.rotation = Quaternion.Euler(rotation);
    }
    #endregion

    #region Hotkeys
    private void TryChangeMode()
    {
        if (HotKeyManager.GetKeyDown(HotkeyConstants.SelectPosition))
        {
            ChangeGimzoMode(TransformType.Position);
            _inspectorManager.GizmoModeChanged(TransformType.Position);
        }

        if (HotKeyManager.GetKeyDown(HotkeyConstants.SelectRotation))
        {
            ChangeGimzoMode(TransformType.Rotation);
            _inspectorManager.GizmoModeChanged(TransformType.Rotation);
        }

        if (HotKeyManager.GetKeyDown(HotkeyConstants.SelectScale))
        {
            ChangeGimzoMode(TransformType.Scale);
            _inspectorManager.GizmoModeChanged(TransformType.Scale);
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

        if (_targetObjects.Count > 0)
        {
            activeGizmo.SetTargetObjects(_targetObjects);
        }

        ToggleGizmos();
    }

    private void ToggleGizmos()
    {
        bool gizmosCanShow = _targetObjects.Count > 0 && _currentTargetingType == TargetingType.Prefab;
        positionGizmo.Gizmo.SetEnabled(gizmosCanShow && currentTransformType == TransformType.Position);
        rotationGizmo.Gizmo.SetEnabled(gizmosCanShow && currentTransformType == TransformType.Rotation);
        scaleGizmo.Gizmo.SetEnabled(gizmosCanShow && currentTransformType == TransformType.Scale);

        if (_targetObjects.Count > 0)
        {
            activeGizmo.SetTargetObjects(_targetObjects);

            // Access the move gizmo behaviour and specify the vertex snap target objects
            MoveGizmo moveGizmo = positionGizmo.Gizmo.MoveGizmo;

            // Vertex snapping won't function properly if the meshes are children of an empty parent
            moveGizmo.SetVertexSnapTargetObjects(_targetObjects);
        }
    }

    private void TryDuplicate()
    {
        if (HotKeyManager.GetModifiedKeyDown(HotkeyConstants.Duplicate) && _targetObjects.Count > 0)
        {
            List<GameObject> duplicateObjects = Duplicate();

            ToggleOutlineRender(false);
            _targetObjects.Clear();

            foreach (GameObject go in duplicateObjects)
            {
                OnTargetObjectChanged(go, false);
            }
        }
    }

    void TryDelete()
    {
        if(HotKeyManager.GetKeyDown(HotkeyConstants.DeletePrefab))
        {
            if (_targetObjects.Count > 0) 
            {
                foreach(GameObject go in _targetObjects)
                {
                    Destroy(go);
                }
                OnTargetObjectChanged(null, true);
            }
        }
    }

    private void TryFocusObject()
    {
        if(HotKeyManager.GetKeyDown(HotkeyConstants.Focus) && _targetObjects.Count > 0)
        {
            RTFocusCamera.Get.Focus(_targetObjects);
        }
    }

    private void TryHideMouse()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    #endregion

    #region Gizmo Callbacks
    void OnGizmoPostDragBegin(Gizmo gizmo, int handleId)
    {
        
    }

    void OnGizmoPostDragEnd(Gizmo gizmo, int handleId)
    {

    }

    void OnGizmoPostDragUpdate(Gizmo gizmo, int handleId)
    {
        if(currentTransformType == TransformType.Scale)
        {
            ValidateScale();
        }

        _inspectorManager.UpdateInputFields();
    }
    #endregion

    #region Utils
    // We can only click on an object if:
    // - We click the left mouse button
    // - We aren't clicking on a gizmo
    // - We aren't clicking on any UI
    private bool DidValidClick()
    {
        return Input.GetMouseButtonDown(0)
            && RTGizmosEngine.Get.HoveredGizmo == null
            && !EventSystem.current.IsPointerOverGameObject();
    }

    private List<GameObject> Duplicate()
    {
        List<GameObject> toReturn = new List<GameObject>();

        foreach(GameObject go in _targetObjects)
        {
            GameObject duplicateObject = Instantiate(go);
            duplicateObject.transform.position = go.transform.position;
            duplicateObject.transform.rotation = go.transform.rotation;
            duplicateObject.transform.localScale = go.transform.localScale;
            duplicateObject.layer = go.layer;
            duplicateObject.transform.parent = go.transform.parent;

            toReturn.Add(duplicateObject);
        }
        
        return toReturn;
    }

    private void ValidateScale()
    {
        Vector3 minimumScale = new Vector3(0.1f, 0.1f, 0.1f);

        foreach (GameObject go in _targetObjects)
        {
            Vector3 newScale = go.transform.localScale;
            go.transform.localScale = Vector3.Max(newScale, minimumScale);
        }
    }

    public void UpdateTargetTransforms(float value, TransformType transformType, TransformAxis transformAxis)
    {
        if (_targetObjects.Count == 0)
            return;

        foreach(GameObject go in _targetObjects)
        {
            Vector3 newPosition = go.transform.position;
            Vector3 newRotation = go.transform.eulerAngles;
            Vector3 newScale = go.transform.localScale;

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

            go.transform.position = newPosition;
            go.transform.rotation = Quaternion.Euler(newRotation);
            go.transform.localScale = newScale;
            activeGizmo.SetTargetObjects(_targetObjects);
        }
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if(newEditMode != EditMode.Prefab)
        {
            OnTargetObjectChanged(null, true);
        }
    }

    private void ToggleZoomAbility()
    {
        bool canZoom = _currentTargetingType != TargetingType.PrefabPlacement;
        RTFocusCamera.Get.ZoomSettings.IsZoomEnabled = canZoom;
    }
    #endregion
}
