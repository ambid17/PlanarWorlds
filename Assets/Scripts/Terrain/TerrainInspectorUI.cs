using UnityEngine;
using UnityEngine.Tilemaps;

public enum TerrainEditMode
{
    Paint, Erase, BoxPaint
}

public class TerrainInspectorUI : MonoBehaviour
{
    private Tile[] _tiles;
    private TileGrid[] _tileGrids;

    public GameObject tileButtonPrefab;

    public TabButton paintButton;
    public TabButton eraseButton;

    public GameObject tileSelectorParent;
    public GameObject tileGridSelectorParent;

    private TerrainManager _terrainManager;
    private ImageTabButton _currentSelectedButton;

    private ImageTabButton _firstTilebutton;
    private ImageTabButton _firstTileGridbutton;

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
                isFirst = false;
                _firstTilebutton = tabButton;
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
                    isFirst = false;
                    _firstTileGridbutton = tabButton;
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
    }

    private void SetCurrentTile(Tile tile, ImageTabButton tabButton)
    {
        if (_currentSelectedButton)
        {
            _currentSelectedButton.Unselect();
        }

        _terrainManager.SetCurrentTile(tile);
        _currentSelectedButton = tabButton;
    }

    private void SetCurrentTileGrid(TileGrid tileGrid, ImageTabButton tabButton)
    {
        if (_currentSelectedButton)
        {
            _currentSelectedButton.Unselect();
        }

        _terrainManager.SetCurrentTileGrid(tileGrid);
        _currentSelectedButton = tabButton;
    }

    public void ToggleTileSelector(bool isSmartDragEnabled)
    {
        tileSelectorParent.SetActive(!isSmartDragEnabled);
        tileGridSelectorParent.SetActive(isSmartDragEnabled);

        if (isSmartDragEnabled)
        {
            SetCurrentTileGrid(_tileGrids[0], _firstTileGridbutton);
        }
        else
        {
            SetCurrentTile(_tiles[0], _firstTilebutton);
        }
    }
}
