using RTG;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum DragState
{
    Paint, Indicate
}

public class TerrainManager : StaticMonoBehaviour<TerrainManager>
{
    public LayerMask terrainLayerMask;
    public Tilemap tileMap;
    public Tilemap highlightTileMap;
    public Tilemap shadowTileMap;

    private Vector3Int _lastShadowTilePosition;
    [SerializeField]
    private HighlightTileSelector _highlightTileSelector;

    private Tile _currentTile;
    private TileGrid _currentTileGrid;
    private TerrainEditMode _currentEditMode;

    private int _brushSize;
    public int BrushSize
    {
        get => _brushSize;
    }

    private Vector2 _mapSize;
    public Vector2 MapSize
    {
        get => _mapSize;
    }
    private Vector2 _previousMapSize;

    private Camera mainCamera;
    private PrefabGizmoManager _prefabGizmoManager;
    private UIManager _uiManager;

    private bool CanModifyTerrain
    {
        get => (!EventSystem.current.IsPointerOverGameObject()
            && RTGizmosEngine.Get.HoveredGizmo == null)
            || (isDragEnabled && isValidDrag);
    }

    [HideInInspector]
    public bool isDragEnabled;
    [HideInInspector]
    public bool isValidDrag;
    private Vector3 _dragStartPosition;
    private List<Vector3Int> _draggedTilePositions = new List<Vector3Int>();

    private BoxCollider _tileMapCollider;
    private BoxCollider _highlightTileMapCollider;
    private BoxCollider _shadowTileMapCollider;

    private void Start()
    {
        _brushSize = Constants.defaultBrushSize;
        _mapSize = new Vector2(Constants.defaultMapSize, Constants.defaultMapSize);
        _previousMapSize = _mapSize;

        mainCamera = Camera.main;
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        _uiManager = UIManager.GetInstance();

        GetComponents();

        UIManager.OnEditModeChanged += EditModeChanged;
    }

    private void GetComponents()
    {
        _tileMapCollider = tileMap.gameObject.GetComponent<BoxCollider>();
        _highlightTileMapCollider = highlightTileMap.gameObject.GetComponent<BoxCollider>();
        _shadowTileMapCollider = shadowTileMap.gameObject.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (_uiManager.EditMode != EditMode.Terrain || _uiManager.isPaused)
            return;

        TryTerrainModification();
    }

    private void TryTerrainModification()
    {
        if (!CanModifyTerrain)
        {
            return;
        }

        // Build a ray using the current mouse cursor position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Check if the ray intersects a game object. If it does, return it
        if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, terrainLayerMask))
        {
            if (isDragEnabled)
                HandleDrag(rayHit.point);
            else if (Input.GetMouseButton(0))
                PaintTile(rayHit.point);

            HighlightSelection(rayHit.point);
        }
    }

    #region DragPaint
    private void HandleDrag(Vector3 hitPoint)
    {
        // Begin drag with LMB
        if (Input.GetMouseButtonDown(0))
        {
            isValidDrag = true;
            _dragStartPosition = hitPoint;
        }

        // Cancel drag if RMB is pressed
        if (Input.GetMouseButtonDown(1))
            ResetDrag();

        // Paint when LMB is released 
        if (Input.GetMouseButtonUp(0))
        {
            if (isValidDrag)
                DragPaintTiles(dragEndPosition: hitPoint, DragState.Paint);

            ResetDrag();
        }

        // Paint shadow tiles (Indicate mode) while drag is in progress
        if (isValidDrag)
            DragPaintTiles(dragEndPosition: hitPoint, DragState.Indicate);
    }

    private void DragPaintTiles(Vector3 dragEndPosition, DragState dragState)
    {
        Vector3Int startPosition = tileMap.WorldToCell(_dragStartPosition);
        Vector3Int endPosition = tileMap.WorldToCell(dragEndPosition);

        Vector3Int offset = TerrainUtils.GetDragPaintOffset(ref startPosition, ref endPosition);

        // Keep track of tile positions we have dragged over
        List<Vector3Int> newDraggedPositions = new List<Vector3Int>();

        for (int x = 0; x <= offset.x; x++)
        {
            for (int y = 0; y <= offset.y; y++)
            {
                Tile tileToPaint = null;

                // Get tile based on edit mode and whether we're painting Tiles or TileGrids 
                if (_currentEditMode == TerrainEditMode.Erase)
                    tileToPaint = dragState == DragState.Indicate ? _highlightTileSelector.centre : null;
                else if (_currentTile != null)
                    tileToPaint = _currentTile;
                else if (_currentTileGrid != null)
                    tileToPaint = _currentTileGrid.GetTileByPosition(x, y, offset);

                Vector3Int tilePosition = new Vector3Int(startPosition.x + x, startPosition.y + y, 0);
                newDraggedPositions.Add(tilePosition);

                // Paint on shadow map if we're still selecting which tiles to paint
                if (dragState == DragState.Indicate)
                    PaintShadowTile(tilePosition, tileToPaint);
                else
                    tileMap.SetTile(tilePosition, tileToPaint);
            }
        }

        if (dragState == DragState.Indicate)
        {
            // Remove any shadow tiles that are no longer representing tiles being dragged
            IEnumerable<Vector3Int> shadowPositionsToRemove = _draggedTilePositions
                .Where(x => !newDraggedPositions.Contains(x));

            shadowTileMap.ClearTiles(shadowPositionsToRemove);
        }

        _draggedTilePositions = newDraggedPositions;
    }

    private void ResetDrag()
    {
        _draggedTilePositions.Clear();
        shadowTileMap.ClearAllTiles();
        isValidDrag = false;
    }
    #endregion

    public void SetBrushSize(int brushSize)
    {
        _brushSize = brushSize;
    }

    public void SetMapSize(TransformAxis axis, int size)
    {
        _previousMapSize = _mapSize;
        if (axis == TransformAxis.X)
            _mapSize.x = size;
        else
            _mapSize.y = size;

        // Resize box colliders to fit new map size
        SetTileMapColliderSizes(_mapSize);
    }

    public void SetCurrentEditMode(TerrainEditMode newEditMode)
    {
        _currentEditMode = newEditMode;
        shadowTileMap.ClearAllTiles();
    }

    public void SetCurrentTile(Tile tile)
    {
        _currentTile = tile;
    }

    public void SetCurrentTileGrid(TileGrid tileGrid)
    {
        _currentTileGrid = tileGrid;
    }

    public void PaintTile(Vector3 hitPoint)
    {
        if (!_currentTile)
            return;

        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);
        List<Vector3Int> tilePositionsForBrush = GetTilesByBrushSize(centerPosition);

        foreach (Vector3Int tilePosition in tilePositionsForBrush)
        {
            if (_currentEditMode == TerrainEditMode.Paint)
                tileMap.SetTile(tilePosition, _currentTile, _tileMapCollider.bounds);
            else if (_currentEditMode == TerrainEditMode.Erase)
                tileMap.SetTile(tilePosition, null, _tileMapCollider.bounds);
        }
    }

    private List<Vector3Int> GetTilesByBrushSize(Vector3Int centerPos)
    {
        List<Vector3Int> tilePositions = new List<Vector3Int>();

        if (_brushSize == 1 || isDragEnabled)
            return new List<Vector3Int> { centerPos };

        // convert the brush size, to the number of tiles from the center
        int sizeFromCenter = _brushSize - 1;

        for (int x = -sizeFromCenter; x <= sizeFromCenter; x++)
        {
            for (int y = -sizeFromCenter; y <= sizeFromCenter; y++)
            {
                tilePositions.Add(new Vector3Int(x + centerPos.x, y + centerPos.y, 0));
            }
        }

        return tilePositions;
    }

    public void HighlightSelection(Vector3 hitPoint)
    {
        highlightTileMap.ClearAllTiles();
        PaintHighlightTiles(hitPoint);
        PaintShadowTiles(hitPoint);
    }

    private void PaintHighlightTiles(Vector3 hitPoint)
    {
        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);

        Dictionary<Vector3Int, Tile> tilesForBrush = _highlightTileSelector
            .GetHighlightTilesByBrushSize(centerPosition, _brushSize, isDragEnabled);

        foreach (KeyValuePair<Vector3Int, Tile> keyValuePair in tilesForBrush)
        {
            highlightTileMap.SetTile(keyValuePair.Key, keyValuePair.Value,
                _highlightTileMapCollider.bounds);
        }
    }

    public void PaintShadowTiles(Vector3 hitPoint)
    {
        if (!_currentTile
            || _currentEditMode == TerrainEditMode.Erase
            || (isDragEnabled && !isValidDrag))
            return;

        if (!isValidDrag)
            shadowTileMap.ClearAllTiles();

        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);
        List<Vector3Int> tilePositionsForBrush = GetTilesByBrushSize(centerPosition);

        foreach (Vector3Int tilePosition in tilePositionsForBrush)
        {
            PaintShadowTile(tilePosition, _currentTile);
        }
    }

    private void PaintShadowTile(Vector3Int tilePosition, Tile tileToPaint)
    {
        shadowTileMap.SetTile(tilePosition, tileToPaint, _shadowTileMapCollider.bounds);
        shadowTileMap.SetTileFlags(tilePosition, TileFlags.None);
        shadowTileMap.SetColor(tilePosition, Constants.ShadowTileColor);
    }

    public void GenerateDefaultMap(bool clearAllTiles)
    {
        if (!_currentTile)
            return;

        if (clearAllTiles)
            tileMap.ClearAllTiles();

        FillTiles();
    }

    private void FillTiles()
    {
        int xRange = Mathf.FloorToInt(_mapSize.x / 2);
        int yRange = Mathf.FloorToInt(_mapSize.y / 2);

        for (int x = -xRange; x < xRange; x++)
        {
            for (int y = -yRange; y < yRange; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                tileMap.SetTile(tilePos, _currentTile);
                // This allows us to change the opacity of the tile in PaintShadowTile
                tileMap.SetTileFlags(tilePos, TileFlags.None);
            }
        }
    }

    private void SetTileMapColliderSizes(Vector2 colliderSize)
    {
        _tileMapCollider.size = colliderSize;
        _highlightTileMapCollider.size = colliderSize;
        _shadowTileMapCollider.size = colliderSize;
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if (newEditMode != EditMode.Terrain)
        {
            highlightTileMap.ClearAllTiles();
            shadowTileMap.ClearAllTiles();
        }
    }
}
