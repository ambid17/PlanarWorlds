using RTG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TerrainManager : StaticMonoBehaviour<TerrainManager>
{
    public LayerMask terrainLayerMask;
    public Tilemap tileMap;
    public Tilemap highlightTileMap;
    public Tilemap shadowTileMap;

    private Vector3Int _lastShadowTilePosition;
    [SerializeField]
    private Tile highlightTile;

    private Tile _currentTile;
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

    public bool isDragEnabled;
    private Vector3 _dragStartPosition;

    public TileGrid currentTileGrid;

    private bool _isValidClick;

    void Start()
    {
        _brushSize = Constants.defaultBrushSize;
        _mapSize = new Vector2(Constants.defaultMapSize, Constants.defaultMapSize);
        _previousMapSize = _mapSize;

        mainCamera = Camera.main;
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        _uiManager = UIManager.GetInstance();

        UIManager.OnEditModeChanged += EditModeChanged;
    }

    private void Update()
    {
        if (_uiManager.EditMode != EditMode.Terrain || _uiManager.isPaused)
            return;

        TryTerrainModification();
    }

    private void TryTerrainModification()
    {
        if (EventSystem.current.IsPointerOverGameObject()
            || RTGizmosEngine.Get.HoveredGizmo != null)
        {
            return;
        }

        // Build a ray using the current mouse cursor position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Check if the ray intersects a game object. If it does, return it
        if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, terrainLayerMask))
        {
            if (isDragEnabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _isValidClick = true;
                    _dragStartPosition = rayHit.point;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (_isValidClick)
                        DragPaintTiles(dragEndPosition: rayHit.point);

                    _isValidClick = false;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                PaintTile(rayHit.point);
            }

            HighlightSelection(rayHit.point);
        }
    }

    private void DragPaintTiles(Vector3 dragEndPosition)
    {
        Vector3Int startPosition = tileMap.WorldToCell(_dragStartPosition);
        Vector3Int endPosition = tileMap.WorldToCell(dragEndPosition);

        Vector3Int offset = TerrainUtils.GetDragPaintOffset(startPosition, endPosition);

        for (int x = 0; x <= offset.x; x++)
        {
            for (int y = 0; y <= offset.y; y++)
            {
                Tile tileToPaint = currentTileGrid.GetTileByPosition(x, y, offset);
                tileMap.SetTile(new Vector3Int(startPosition.x + x, startPosition.y + y, 0), tileToPaint);
            }
        }
    }

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
    }

    public void SetCurrentEditMode(TerrainEditMode newEditMode)
    {
        _currentEditMode = newEditMode;
    }

    public void SetCurrentTile(Tile tile)
    {
        _currentTile = tile;
    }

    public void SetCurrentTileGrid(TileGrid tileGrid)
    {
        currentTileGrid = tileGrid;
    }

    public void PaintTile(Vector3 hitPoint)
    {
        if (!_currentTile)
            return;

        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);
        List<Vector3Int> tilePositionsForBrush = GetTilesByBrushSize(centerPosition);

        foreach(Vector3Int tilePosition in tilePositionsForBrush)
        {
            if (_currentEditMode == TerrainEditMode.Paint)
                tileMap.SetTile(tilePosition, _currentTile);
            else if (_currentEditMode == TerrainEditMode.Erase)
                tileMap.SetTile(tilePosition, null);
        }
    }

    private List<Vector3Int> GetTilesByBrushSize(Vector3Int centerPos)
    {
        List<Vector3Int> tilePositions = new List<Vector3Int>();

        if(_brushSize == 1)
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

        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);
        List<Vector3Int> tilePositionsForBrush = GetTilesByBrushSize(centerPosition);

        foreach (Vector3Int tilePosition in tilePositionsForBrush)
        {
            highlightTileMap.SetTile(tilePosition, highlightTile);
        }

        PaintShadowTiles(hitPoint);
    }

    public void PaintShadowTiles(Vector3 hitPoint)
    {
        shadowTileMap.ClearAllTiles();

        if (!_currentTile || _currentEditMode == TerrainEditMode.Erase)
            return;

        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);
        List<Vector3Int> tilePositionsForBrush = GetTilesByBrushSize(centerPosition);

        foreach (Vector3Int tilePosition in tilePositionsForBrush)
        {
            shadowTileMap.SetTile(tilePosition, _currentTile);
            shadowTileMap.SetTileFlags(tilePosition, TileFlags.None);
            shadowTileMap.SetColor(tilePosition, new Color(1, 1, 1, 0.3f));
        }
    }

    public void GenerateDefaultMap()
    {
        if (!_currentTile)
            return;

        tileMap.ClearAllTiles();

        for (int x = 0; x < _mapSize.x; x++)
        {
            for (int y = 0; y < _mapSize.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                tileMap.SetTile(tilePos, _currentTile);
                // This allows us to change the opacity of the tile in PaintShadowTile
                tileMap.SetTileFlags(tilePos, TileFlags.None);
            }
        }
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
