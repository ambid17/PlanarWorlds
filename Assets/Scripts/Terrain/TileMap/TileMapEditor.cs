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

public class TileMapEditor : MonoBehaviour
{
    public TileList tileList;
    public TileGrid[] tileGrids;

    public LayerMask terrainLayerMask;

    public Tilemap tileMap;
    public Tilemap highlightTileMap;
    public Tilemap shadowTileMap;

    public TileMapInspectorUI _tileMapInspectorUI;


    [SerializeField]
    private HighlightTileContainer _highlightTileSelector;

    private Tile _currentTile;
    private TileGrid _currentTileGrid;
    private TileMapEditMode _currentEditMode;

    private int _brushSize;
    public int BrushSize
    {
        get => _brushSize;
    }

    private Camera mainCamera;
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
    public bool isSmartDragEnabled;
    [HideInInspector]
    public bool isValidDrag;
    private Vector3 _dragStartPosition;
    private List<Vector3Int> _draggedTilePositions = new List<Vector3Int>();
    private List<Vector3Int> _highlightedTilePositions = new List<Vector3Int>();
    private List<Vector3Int> _shadowTilePositions = new List<Vector3Int>();

    private BoxCollider _tileMapCollider;
    private BoxCollider _highlightTileMapCollider;
    private BoxCollider _shadowTileMapCollider;

    private bool _isDirty;

    void Awake()
    {
        _brushSize = Constants.defaultBrushSize;

        mainCamera = Camera.main;
        _uiManager = UIManager.GetInstance();

        GetComponents();

        UIManager.OnEditModeChanged += EditModeChanged;

        _isDirty = false;
    }

    private void GetComponents()
    {
        _tileMapCollider = tileMap.gameObject.GetComponent<BoxCollider>();
        _highlightTileMapCollider = highlightTileMap.gameObject.GetComponent<BoxCollider>();
        _shadowTileMapCollider = shadowTileMap.gameObject.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (_uiManager.EditMode != EditMode.Terrain || _uiManager.isPaused || _uiManager.isFileBrowserOpen)
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
                if (_currentEditMode == TileMapEditMode.Erase)
                    tileToPaint = null;
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
                {
                    tileMap.SetTile(tilePosition, tileToPaint);
                    _isDirty = true;
                }
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
        shadowTileMap.ClearTiles(_shadowTilePositions);
        isValidDrag = false;
    }
    #endregion

    public void SetBrushSize(int brushSize)
    {
        _brushSize = brushSize;
    }

    public void SetCurrentEditMode(TileMapEditMode newEditMode)
    {
        _currentEditMode = newEditMode;
        shadowTileMap.ClearTiles(_shadowTilePositions);
    }

    public void SetCurrentTile(Tile tile)
    {
        _currentTile = tile;
        _currentTileGrid = null;
    }

    public void SetCurrentTileGrid(TileGrid tileGrid)
    {
        _currentTileGrid = tileGrid;
        _currentTile = null;
    }

    public void PaintTile(Vector3 hitPoint)
    {
        if (!_currentTile)
            return;

        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);
        List<Vector3Int> tilePositionsForBrush = GetTilesByBrushSize(centerPosition);

        foreach (Vector3Int tilePosition in tilePositionsForBrush)
        {
            _isDirty = true;
            if (_currentEditMode == TileMapEditMode.Paint)
                tileMap.SetTile(tilePosition, _currentTile, _tileMapCollider.bounds);
            else if (_currentEditMode == TileMapEditMode.Erase)
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
        highlightTileMap.ClearTiles(_highlightedTilePositions);
        PaintHighlightTiles(hitPoint);
        PaintShadowTiles(hitPoint);
    }

    private void PaintHighlightTiles(Vector3 hitPoint)
    {
        Vector3Int startPosition = tileMap.WorldToCell(_dragStartPosition);
        Vector3Int dragEndPosition = tileMap.WorldToCell(hitPoint);

        Dictionary<Vector3Int, Tile> tilesForBrush;
        if (isDragEnabled)
        {
            if (isValidDrag)
            {
                tilesForBrush = _highlightTileSelector
                .GetHighlightTilesForDrag(startPosition, dragEndPosition);
            }
            else
            {
                tilesForBrush = _highlightTileSelector
                .GetHighlightTilesByBrushSize(dragEndPosition, 1); // We don't want to use the brush size when in drag mode but not dragging
            }
            
        }
        else
        {
            tilesForBrush = _highlightTileSelector
            .GetHighlightTilesByBrushSize(dragEndPosition, _brushSize);
        }
        

        foreach (KeyValuePair<Vector3Int, Tile> keyValuePair in tilesForBrush)
        {
            highlightTileMap.SetTile(keyValuePair.Key, keyValuePair.Value,
                _highlightTileMapCollider.bounds);
            _highlightedTilePositions.Add(keyValuePair.Key);
        }
    }

    public void PaintShadowTiles(Vector3 hitPoint)
    {
        if (!_currentTile
            || _currentEditMode == TileMapEditMode.Erase)
            return;

        if (!isValidDrag)
            shadowTileMap.ClearTiles(_shadowTilePositions);

        Vector3Int centerPosition = tileMap.WorldToCell(hitPoint);
        List<Vector3Int> tilePositionsForBrush = GetTilesByBrushSize(centerPosition);

        foreach (Vector3Int tilePosition in tilePositionsForBrush)
        {
            PaintShadowTile(tilePosition, _currentTile);
        }
    }

    private void PaintShadowTile(Vector3Int tilePosition, Tile tileToPaint)
    {
        _shadowTilePositions.Add(tilePosition);
        shadowTileMap.SetTile(tilePosition, tileToPaint, _shadowTileMapCollider.bounds);
        shadowTileMap.SetTileFlags(tilePosition, TileFlags.None);
        shadowTileMap.SetColor(tilePosition, Constants.ShadowTileColor);
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if (newEditMode != EditMode.Terrain)
        {
            highlightTileMap.ClearAllTiles();
            shadowTileMap.ClearAllTiles();
        }
    }

    public void Clear()
    {
        _isDirty = false;
        tileMap.ClearAllTiles();
        highlightTileMap.ClearAllTiles();
        shadowTileMap.ClearAllTiles();
    }

    public void ToggleTileSelector()
    {
        _tileMapInspectorUI.ToggleTileSelector(isSmartDragEnabled);
        ResetDrag();
    }

    public bool IsDirty()
    {
        return _isDirty;
    }

    public void SaveIntoCampaign(Campaign campaign)
    {
        campaign.tiles = new List<TileModel>();

        foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
        {
            if (!tileMap.HasTile(position))
                continue;

            TileModel tile = new TileModel();
            tile.tileName = tileMap.GetTile(position).name;
            tile.tilePosition = position;

            campaign.tiles.Add(tile);
        }

        _isDirty = false;
    }

    public void LoadFromCampaign(Campaign campaign)
    {
        foreach (TileModel tile in campaign.tiles)
        {
            Tile tileToSet = tileList.tiles.Where(t => t.name == tile.tileName).FirstOrDefault();
            tileMap.SetTile(tile.tilePosition, tileToSet);
        }

        _isDirty = false;
    }
}
