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
    public Transform buttonParent;

    public Button paintButton;
    public Button eraseButton;

    public Tilemap tileMap;

    private TerrainEditMode currentEditMode;

    private Tile _currentTile;

    private PrefabGizmoManager _prefabGizmoManager;

    void Start()
    {
        _prefabGizmoManager = PrefabGizmoManager.GetInstance();
        CreateTileButtons();
        InitModeButtons();
    }

    private void CreateTileButtons()
    {
        foreach (Tile tile in tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonParent);
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


    private void ChangeEditMode(TerrainEditMode newMode)
    {
        currentEditMode = newMode;
    }


    public void TryPaintTile(RaycastHit hit)
    {
        if (!_currentTile)
            return;


        Vector3Int tilePos = tileMap.WorldToCell(hit.point);

        if (currentEditMode == TerrainEditMode.Paint)
        {
            tileMap.SetTile(tilePos, _currentTile);
        }
        else if (currentEditMode == TerrainEditMode.Erase)
        {
            tileMap.SetTile(tilePos, null);
        }
    }
}
