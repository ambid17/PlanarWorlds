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

public class TerrainInspector : MonoBehaviour
{
    public Tile[] tiles;

    public GameObject buttonPrefab;

    public Button paintButton;
    public Button eraseButton;
    public Button settingsButton;

    public GameObject tileSelectorParent;

    public TerrainSettingsUI terrainSettingsUI;

    private TerrainManager _terrainManager;

    void Awake()
    {
        _terrainManager = TerrainManager.GetInstance();
    }

    void Start()
    {
        CreateTileButtons();
        InitModeButtons();
        InitSettingsMenu();
    }

    private void CreateTileButtons()
    {
        foreach (Tile tile in tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab, tileSelectorParent.transform);
            ButtonManagerBasicIcon buttonManager = newButton.GetComponent<ButtonManagerBasicIcon>();
            buttonManager.buttonIcon = tile.sprite;
            buttonManager.UpdateUI();

            Button myButton = newButton.GetComponent<Button>();
            myButton.onClick.AddListener(() => SetCurrentTile(tile));
        }
    }

    private void SetCurrentTile(Tile tile)
    {
        _terrainManager.SetCurrentTile(tile);
    }

    private void InitModeButtons()
    {
        paintButton.onClick.AddListener(() => ChangeEditMode(TerrainEditMode.Paint));
        eraseButton.onClick.AddListener(() => ChangeEditMode(TerrainEditMode.Erase));
    }

    private void InitSettingsMenu()
    {
        settingsButton.onClick.AddListener(() => ToggleSettingsMenu(true));
        ToggleSettingsMenu(false);
    }

    private void ToggleSettingsMenu(bool shouldBeActive)
    {
        terrainSettingsUI.ToggleSettingsMenu(shouldBeActive);
    }

    private void ChangeEditMode(TerrainEditMode newMode)
    {
        _terrainManager.SetCurrentEditMode(newMode);
        ToggleSettingsMenu(false);
    }
}
