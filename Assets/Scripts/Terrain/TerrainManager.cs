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


    // Drag Mode
    private bool _isDragging;
    private Vector3Int _cursorCellPos;

    private TileBase _tileBeingDragged;
    
    public Tile[] gridTiles; 
    private Vector3Int _tilePatternGrid;

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
        // Build a ray using the current mouse cursor position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        bool hitTileMap = false;

        // Check if the ray intersects a game object. If it does, return it
        if (Physics.Raycast(ray, out RaycastHit rayHit, float.MaxValue, terrainLayerMask))
        {
            hitTileMap = true;
        }

        if (hitTileMap)
        {
            if (Input.GetMouseButton(0)
            && !EventSystem.current.IsPointerOverGameObject())
            {   
                if (_currentEditMode == TerrainEditMode.Paint)
                    PaintTile(rayHit.point);
            }

            Vector3Int newCursorCellPos = tileMap.WorldToCell(rayHit.point);
            TileBase tileUnderCursor = tileMap.GetTile(newCursorCellPos);

            // Drag handling 
            if (_currentEditMode == TerrainEditMode.Drag)
            {
                // Start drag if clicking on tile when in drag mode
                if (Input.GetMouseButtonDown(0)
                    && tileUnderCursor != null)
                {
                    _isDragging = true;
                }
                
                if (Input.GetMouseButtonUp(0))
                {
                    _isDragging = false;
                }

                if (_isDragging)
                {
                    // Todo: Replace this temporary tile indexing solution
                    //int tileIndex = Convert.ToInt32(tileUnderCursor.name.First());
                    int tileIndex = Array.IndexOf(gridTiles, _tileBeingDragged);

                    int row = Mathf.FloorToInt(tileIndex / 3);
                    int col = tileIndex % 3;

                    Vector3Int coords = new Vector3Int(row, col, 0);

                    if (newCursorCellPos != _cursorCellPos)
                    {
                        Vector3Int delta = newCursorCellPos - _cursorCellPos;

                        // Todo: Configurable pattern grid dimensions 
                        // Todo: Wrap round when row = 0 and delta.y = -1
                        row = Mathf.Max(0, (row + delta.y) % 3);
                        col = Mathf.Max(0, (col + delta.x) % 3);

                        // 2D -> 1D index
                        int newTileIndex = row * 3 + col;

                        // Index determined by cursor tile's neighbour
                        tileMap.SetTile(newCursorCellPos, gridTiles[newTileIndex]);
                    }

                    _tileBeingDragged = tileMap.GetTile(newCursorCellPos);
                }
            }

            _cursorCellPos = newCursorCellPos;

            HighlightSelection(rayHit.point);
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
            //else if (_currentEditMode == TerrainEditMode.Drag)
                
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
