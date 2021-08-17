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

public class TerrainInspector : StaticMonoBehaviour<TerrainInspector>
{
    public Tile[] tiles;

    public GameObject buttonPrefab;

    public Button paintButton;
    public Button eraseButton;
    public Button settingsButton;

    public GameObject tileSelectorParent;
    public GameObject settingsParent;

    public Tilemap tileMap;

    private TerrainEditMode currentEditMode;

    private Tile _currentTile;
    private Vector3Int _lastShadowTilePosition;
    private int brushSize;

    private PrefabGizmoManager _prefabGizmoManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
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

    public void SetCurrentTile(Tile tile)
    {
        _currentTile = tile;
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

    private void ChangeEditMode(TerrainEditMode newMode)
    {
        currentEditMode = newMode;
        ToggleSettingsMenu(false);
    }

    private void ToggleSettingsMenu(bool shouldBeActive)
    {
        settingsParent.SetActive(shouldBeActive);
        tileSelectorParent.SetActive(!shouldBeActive);
    }


    public void TryPaintTile(Vector3 hitPoint)
    {
        if (!_currentTile)
            return;

        Vector3Int tilePos = tileMap.WorldToCell(hitPoint);

        if (currentEditMode == TerrainEditMode.Paint)
        {
            tileMap.SetTile(tilePos, _currentTile);
            tileMap.SetTileFlags(tilePos, TileFlags.None);
            tileMap.SetColor(tilePos, new Color(1, 1, 1, 1));
        }
        else if (currentEditMode == TerrainEditMode.Erase)
        {
            tileMap.SetTile(tilePos, null);
        }
    }

    public void PaintShadowTile(Vector3 hitPoint)
    {
        if (!_currentTile || currentEditMode == TerrainEditMode.Erase)
            return;

        Vector3Int tilePos = tileMap.WorldToCell(hitPoint);

        // Clear the last shadow tile
        if (_lastShadowTilePosition != null
            && tileMap.GetColor(_lastShadowTilePosition).a == 0.5f
            && _lastShadowTilePosition != tilePos)
        {
            tileMap.SetTile(_lastShadowTilePosition, null);
            tileMap.SetTileFlags(tilePos, TileFlags.None);
            tileMap.SetColor(tilePos, new Color(1, 1, 1, 1));
        }

        if (tileMap.GetTile(tilePos) == null)
        {
            // Set the shadow tile
            tileMap.SetTile(tilePos, _currentTile);
            tileMap.SetTileFlags(tilePos, TileFlags.None);
            tileMap.SetColor(tilePos, new Color(1, 1, 1, 0.5f));
        }

        _lastShadowTilePosition = tilePos;
    }
}
