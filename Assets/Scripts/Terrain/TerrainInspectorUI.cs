using UnityEngine;
using UnityEngine.Tilemaps;

public enum TerrainEditMode
{
    Paint, Erase
}

public class TerrainInspectorUI : MonoBehaviour
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
        _tiles = _terrainManager.tileList.tiles;
        _tileGrids = _terrainManager.tileGrids;
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
        paintButton.SetupAction(() => ChangeEditMode(TerrainEditMode.Paint));
        eraseButton.SetupAction(() => ChangeEditMode(TerrainEditMode.Erase));

        paintButton.Select();
    }

    private void ChangeEditMode(TerrainEditMode newMode)
    {
        _terrainManager.SetCurrentEditMode(newMode);

        if(newMode == TerrainEditMode.Paint)
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

        _terrainManager.SetCurrentTile(tile);
        _currentTileButton = tabButton;
        _currentTile = tile;
    }

    private void SetCurrentTileGrid(TileGrid tileGrid, ImageTabButton tabButton)
    {
        if (_currentTileGridButton)
        {
            _currentTileGridButton.Unselect();
        }

        _terrainManager.SetCurrentTileGrid(tileGrid);
        _currentTileGridButton = tabButton;
        _currentTileGrid = tileGrid;
    }

    public void ToggleTileSelector(bool isSmartDragEnabled)
    {
        tileList.SetActive(!isSmartDragEnabled);
        tileGridList.SetActive(isSmartDragEnabled);

        if (isSmartDragEnabled)
        {
            _terrainManager.SetCurrentTileGrid(_currentTileGrid);
        }
        else
        {
            _terrainManager.SetCurrentTile(_currentTile);
        }
    }
}
