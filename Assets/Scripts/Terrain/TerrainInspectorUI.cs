using UnityEngine;
using UnityEngine.Tilemaps;

public enum TerrainEditMode
{
    Paint, Erase, BoxPaint
}

public class TerrainInspectorUI : MonoBehaviour
{
    public Tile[] tiles;
    public TileGrid[] tileGrids;

    public GameObject buttonPrefab;

    public TabButton paintButton;
    public TabButton eraseButton;

    public GameObject tileSelectorParent;

    private TerrainManager _terrainManager;
    private ImageTabButton _currentSelectedButton;

    void Awake()
    {
        _terrainManager = TerrainManager.GetInstance();
    }

    void Start()
    {
        CreateTileButtons();
        CreateTileGridButtons();
        InitModeButtons();
    }

    private void CreateTileButtons()
    {
        foreach (Tile tile in tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab, tileSelectorParent.transform);
            ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();
            tabButton.Setup(tile.sprite, () => SetCurrentTile(tile, tabButton));
        }
    }

    private void SetCurrentTile(Tile tile, ImageTabButton tabButton)
    {
        if (_currentSelectedButton)
        {
            _currentSelectedButton.Unselect();
        }

        _terrainManager.SetCurrentTile(tile);
        _terrainManager.SetCurrentTileGrid(null);

        _currentSelectedButton = tabButton;
    }

    private void CreateTileGridButtons()
    {
        foreach (TileGrid tileGrid in tileGrids)
        {
            if (tileGrid != null)
            {
                GameObject newButton = Instantiate(buttonPrefab, tileSelectorParent.transform);
                ImageTabButton tabButton = newButton.GetComponent<ImageTabButton>();
                tabButton.Setup(tileGrid.Sprite, () => SetCurrentTileGrid(tileGrid, tabButton));
            }
        }
    }

    private void SetCurrentTileGrid(TileGrid tileGrid, ImageTabButton tabButton)
    {
        if (_currentSelectedButton)
        {
            _currentSelectedButton.Unselect();
        }

        _terrainManager.SetCurrentTileGrid(tileGrid);
        _terrainManager.SetCurrentTile(null);

        _currentSelectedButton = tabButton;
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
}
