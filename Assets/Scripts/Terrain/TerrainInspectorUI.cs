using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Michsky.UI.ModernUIPack;

public enum TerrainEditMode
{
    Paint, Erase, BoxPaint
}

public class TerrainInspectorUI : MonoBehaviour
{
    public GameObject buttonPrefab;

    public ToggleButton paintButton;
    public ToggleButton eraseButton;
    public ToggleButton settingsButton;

    public GameObject tileSelectorParent;

    public TerrainSettingsUI terrainSettingsUI;

    private TerrainManager _terrainManager;
    private ImageToggleButton _currentSelectedButton;

    void Awake()
    {
        _terrainManager = TerrainManager.GetInstance();
    }

    void Start()
    {
        CreateTileButtons();
        InitModeButtons();
    }

    private void CreateTileButtons()
    {
        foreach (Tile tile in _terrainManager.tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab, tileSelectorParent.transform);
            ImageToggleButton toggleButton = newButton.GetComponent<ImageToggleButton>();
            toggleButton.Setup(tile.sprite, () => SetCurrentTile(tile, toggleButton));
        }
    }

    private void SetCurrentTile(Tile tile, ImageToggleButton toggleButton)
    {
        if (_currentSelectedButton)
        {
            _currentSelectedButton.Unselect();
        }

        _terrainManager.SetCurrentTile(tile);

        _currentSelectedButton = toggleButton;
    }

    private void InitModeButtons()
    {
        paintButton.SetupAction(() => ChangeEditMode(TerrainEditMode.Paint));
        eraseButton.SetupAction(() => ChangeEditMode(TerrainEditMode.Erase));

        settingsButton.SetupAction(() => ToggleSettingsMenu(true));
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
}
