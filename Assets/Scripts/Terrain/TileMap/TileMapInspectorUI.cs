using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileMapEditMode
{
    Paint, Erase
}

public class TileMapInspectorUI : MonoBehaviour
{
    private Tile[] _tiles;
    private TileGrid[] _tileGrids;

    public GameObject tileButtonPrefab;

    public TabButton paintButton;
    public TabButton eraseButton;

    public GameObject tileSelectorParent;
    public GameObject tileList;
    public GameObject tileGridSelectorParent;
    public GameObject tileGridList;

    private TerrainManager _terrainManager;

    private ImageTabButton _currentTileButton;
    private Tile _currentTile;
    private ImageTabButton _currentTileGridButton;
    private TileGrid _currentTileGrid;

    void Awake()
    {
        _terrainManager = TerrainManager.GetInstance();
        _tiles = _terrainManager.tileMapEditor.tileList.tiles;
        _tileGrids = _terrainManager.tileMapEditor.tileGrids;
    }

    void Start()
    {
        CreateTileButtons();
        CreateTileGridButtons();
        InitModeButtons();
        ToggleTileSelector(false);
    }

    private void CreateTileButtons()
    {
        bool isFirst = true;
        foreach (Tile tile in _tiles)
        {
            GameObject newButton = Instantiate(tileButtonPrefab, tileSelectorParent.transform);
            ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();
            tabButton.Setup(tile.sprite, () => SetCurrentTile(tile, tabButton));

            if (isFirst)
            {
                tabButton.Select();
                _currentTileButton = tabButton;
                _currentTile = tile;

                isFirst = false;
            }
        }
    }

    private void CreateTileGridButtons()
    {
        bool isFirst = true;
        foreach (TileGrid tileGrid in _tileGrids)
        {
            if (tileGrid != null)
            {
                GameObject newButton = Instantiate(tileButtonPrefab, tileGridSelectorParent.transform);
                ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();
                tabButton.Setup(tileGrid.Sprite, () => SetCurrentTileGrid(tileGrid, tabButton));

                if (isFirst)
                {
                    tabButton.Select();
                    _currentTileGridButton = tabButton;
                    _currentTileGrid = tileGrid;

                    isFirst = false;
                }
            }
        }
    }

    private void InitModeButtons()
    {
        paintButton.SetupAction(() => ChangeEditMode(TileMapEditMode.Paint));
        eraseButton.SetupAction(() => ChangeEditMode(TileMapEditMode.Erase));

        paintButton.Select();
    }

    private void ChangeEditMode(TileMapEditMode newMode)
    {
        _terrainManager.tileMapEditor.SetCurrentEditMode(newMode);

        if(newMode == TileMapEditMode.Paint)
        {
            paintButton.Select();
            eraseButton.Unselect();
        }
        else
        {
            eraseButton.Select();
            paintButton.Unselect();
        }
    }

    private void SetCurrentTile(Tile tile, ImageTabButton tabButton)
    {
        if (_currentTileButton)
        {
            _currentTileButton.Unselect();
        }

        _terrainManager.tileMapEditor.SetCurrentTile(tile);
        _currentTileButton = tabButton;
        _currentTile = tile;
    }

    private void SetCurrentTileGrid(TileGrid tileGrid, ImageTabButton tabButton)
    {
        if (_currentTileGridButton)
        {
            _currentTileGridButton.Unselect();
        }

        _terrainManager.tileMapEditor.SetCurrentTileGrid(tileGrid);
        _currentTileGridButton = tabButton;
        _currentTileGrid = tileGrid;
    }

    public void ToggleTileSelector(bool isSmartDragEnabled)
    {
        tileList.SetActive(!isSmartDragEnabled);
        tileGridList.SetActive(isSmartDragEnabled);

        if (isSmartDragEnabled)
        {
            _terrainManager.tileMapEditor.SetCurrentTileGrid(_currentTileGrid);
        }
        else
        {
            _terrainManager.tileMapEditor.SetCurrentTile(_currentTile);
        }
    }
}
