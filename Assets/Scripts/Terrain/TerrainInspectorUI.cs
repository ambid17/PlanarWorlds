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
    public TabButton settingsButton;
    public ToggleButton dragButton;

    public GameObject tileSelectorParent;

    public TerrainSettingsUI terrainSettingsUI;

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
            ImageTabButton toggleButton = newButton.GetComponent<ImageTabButton>();
            toggleButton.Setup(tile.sprite, () => SetCurrentTile(tile, toggleButton));
        }
    }

    private void SetCurrentTile(Tile tile, ImageTabButton toggleButton)
    {
        if (_currentSelectedButton)
        {
            _currentSelectedButton.Unselect();
        }

        _terrainManager.SetCurrentTile(tile);
        _terrainManager.SetCurrentTileGrid(null);

        _currentSelectedButton = toggleButton;
    }

    private void CreateTileGridButtons()
    {
        foreach (TileGrid tileGrid in tileGrids)
        {
            if (tileGrid != null)
            {
                GameObject newButton = Instantiate(buttonPrefab, tileSelectorParent.transform);
                ImageTabButton toggleButton = newButton.GetComponent<ImageTabButton>();
                toggleButton.Setup(tileGrid.Sprite, () => SetCurrentTileGrid(tileGrid, toggleButton));
            }
        }
    }

    private void SetCurrentTileGrid(TileGrid tileGrid, ImageTabButton toggleButton)
    {
        if (_currentSelectedButton)
        {
            _currentSelectedButton.Unselect();
        }

        _terrainManager.SetCurrentTileGrid(tileGrid);
        _terrainManager.SetCurrentTile(null);

        _currentSelectedButton = toggleButton;
    }

    private void InitModeButtons()
    {
        paintButton.SetupAction(() => ChangeEditMode(TerrainEditMode.Paint));
        eraseButton.SetupAction(() => ChangeEditMode(TerrainEditMode.Erase));
        settingsButton.SetupAction(() => ToggleSettingsMenu(true));
        dragButton.SetupAction(ToggleDragTerrain);

        ToggleSettingsMenu(false);
        paintButton.Select();
    }

    private void ToggleSettingsMenu(bool shouldBeActive)
    {
        terrainSettingsUI.ToggleSettingsMenu(shouldBeActive);

        paintButton.Unselect();
        eraseButton.Unselect();
        settingsButton.Unselect();
    }

    private void ChangeEditMode(TerrainEditMode newMode)
    {
        _terrainManager.SetCurrentEditMode(newMode);
        ToggleSettingsMenu(false);
    }

    private void ToggleDragTerrain()
    {
        _terrainManager.isDragEnabled = !_terrainManager.isDragEnabled;
        _terrainManager.isValidDrag = false;
    }
}
