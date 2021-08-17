using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainManager : StaticMonoBehaviour<TerrainManager>
{
    public Tilemap tileMap;

    private Vector3Int _lastShadowTilePosition;
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


    void Start()
    {
        _brushSize = Constants.defaultBrushSize;
        _mapSize = new Vector2(Constants.defaultMapSize, Constants.defaultMapSize);
        _previousMapSize = _mapSize;
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
            {
                tileMap.SetTile(tilePosition, _currentTile);
                tileMap.SetTileFlags(tilePosition, TileFlags.None);
                tileMap.SetColor(tilePosition, new Color(1, 1, 1, 1));
            }
            else if (_currentEditMode == TerrainEditMode.Erase)
            {
                tileMap.SetTile(tilePosition, null);
            }
        }
    }

    private List<Vector3Int> GetTilesByBrushSize(Vector3Int centerPos)
    {
        List<Vector3Int> tilePositions = new List<Vector3Int>();

        if(_brushSize == 1)
            return new List<Vector3Int> { centerPos };

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

    public void PaintShadowTile(Vector3 hitPoint)
    {
        //if (!_currentTile || _currentEditMode == TerrainEditMode.Erase)
        //    return;

        //Vector3Int tilePos = tileMap.WorldToCell(hitPoint);

        //// Clear the last shadow tile
        //if (_lastShadowTilePosition != null
        //    && tileMap.GetColor(_lastShadowTilePosition).a == 0.5f
        //    && _lastShadowTilePosition != tilePos)
        //{
        //    tileMap.SetTile(_lastShadowTilePosition, null);
        //    tileMap.SetTileFlags(tilePos, TileFlags.None);
        //    tileMap.SetColor(tilePos, new Color(1, 1, 1, 1));
        //}

        //if (tileMap.GetTile(tilePos) == null)
        //{
        //    // Set the shadow tile
        //    tileMap.SetTile(tilePos, _currentTile);
        //    tileMap.SetTileFlags(tilePos, TileFlags.None);
        //    tileMap.SetColor(tilePos, new Color(1, 1, 1, 0.5f));
        //}

        //_lastShadowTilePosition = tilePos;
    }

    public void GenerateDefaultMap()
    {
        if (!_currentTile)
            return;

        for (int x = 0; x < _previousMapSize.x; x++)
        {
            for (int y = 0; y < _previousMapSize.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                tileMap.SetTile(tilePos, null);
            }
        }

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
}
