using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TerrainModificationMode
{
    Raise,
    Lower,
    Flatten,
}

public class MeshMapEditor : MonoBehaviour
{
    [Header("Settings")]
    public int brushSize;
    public float brushStrength;
    public float brushHeight;
    public LayerMask modificationLayerMask;
    public MeshMapInspector mapInspector;

    // State
    public TerrainModificationMode currentModificationAction;
    public Terrain terrain;
    private int _terrainResolution;
    private TerrainData _terrainData;

    private Camera _mainCamera;
    private bool _isDirty;
    private UIManager _uiManager;
    private TerrainManager _terrainManager;

    void Awake()
    {
        _isDirty = false;
        _mainCamera = Camera.main;

        _uiManager = UIManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();

        _terrainData = terrain.terrainData;
        _terrainResolution = _terrainData.heightmapResolution;
    }

    private void Update()
    {
        if (_uiManager.EditMode != EditMode.Terrain || _uiManager.isPaused || _uiManager.isFileBrowserOpen || _terrainManager.currentTerrainMode == TerrainMode.TileMap)
            return;

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            TryModification(false);
        }
        //if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject())
        //{
        //    TryModification(true);
        //}
        if (Input.GetMouseButtonUp(0))
        {
            // This makes the process much quicker, see:
            // https://docs.unity3d.com/ScriptReference/TerrainData.SetHeightsDelayLOD.html
            _terrainData.SyncHeightmap();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SwitchTerrainModificationMode(TerrainModificationMode.Raise);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            SwitchTerrainModificationMode(TerrainModificationMode.Lower);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchTerrainModificationMode(TerrainModificationMode.Flatten);
        }
        
    }

    private void TryModification(bool shouldSample)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, float.MaxValue, modificationLayerMask))
        {
            if (shouldSample)
            {
                brushHeight = SampleHeight(hit.point);
            }
            else 
            {
                ModifyTerrain(hit.point);
            }
        }
    }

    public void SetBrushHeight(float newHeight)
    {
        brushHeight = newHeight / _terrainData.size.y;
    }

    public void SwitchTerrainModificationMode(TerrainModificationMode mode)
    {
        currentModificationAction = mode;
        mapInspector.TerrainModificationModeChanged(mode);
    }

    public void ModifyTerrain(Vector3 hitPoint)
    {
        int brushRadius = brushSize / 2;

        float relativeHitX = hitPoint.x - terrain.transform.position.x;
        float relativeHitY = hitPoint.z - terrain.transform.position.z;

        int terrainX = (int) ((relativeHitX / _terrainData.size.x) * _terrainResolution);
        int terrainY = (int)((relativeHitY / _terrainData.size.z) * _terrainResolution);

        int startingXIndex = terrainX - brushRadius;
        int startingYIndex = terrainY - brushRadius;

        if (startingXIndex < 0 || startingYIndex < 0)
            return;

        // Note: Terrain heights are 0-1, indexed as y,x
        // See: https://docs.unity3d.com/ScriptReference/TerrainData.GetHeights.html
        float[,] heights = _terrainData.GetHeights(startingXIndex, startingYIndex, brushSize, brushSize);

        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                float modificationAmount = Mathf.Min(1, brushStrength * Time.smoothDeltaTime);
                switch (currentModificationAction)
                {
                    case TerrainModificationMode.Raise:
                        heights[x, y] += modificationAmount;
                        break;
                    case TerrainModificationMode.Lower:
                        heights[x, y] -= modificationAmount;
                        break;
                    case TerrainModificationMode.Flatten:
                        heights[x, y] = brushHeight;
                        break;
                }
            }
        }

        _terrainData.SetHeightsDelayLOD(startingXIndex, startingYIndex, heights);
    }

    private float SampleHeight(Vector3 hitPoint)
    {
        Vector3 relativeHitPoint = (hitPoint - terrain.GetPosition());
        Vector3 normalizedHitPoint = new Vector3
            (
            (relativeHitPoint.x / _terrainData.size.x),
            (relativeHitPoint.y / _terrainData.size.y),
            (relativeHitPoint.z / _terrainData.size.z)
            );

        Vector3 locationInTerrain = new Vector3(normalizedHitPoint.x * _terrainResolution, 0, normalizedHitPoint.z * _terrainResolution);

        int xBase = (int)locationInTerrain.x;
        int yBase = (int)locationInTerrain.z;
        return Mathf.LerpUnclamped(0f, 1f, (terrain.terrainData.GetHeight(xBase, yBase) / terrain.terrainData.size.y));
    }

    public void Enable()
    {
        terrain.gameObject.SetActive(true);
    }

    public void Disable()
    {
        terrain.gameObject.SetActive(false);
        Clear();
    }

    public bool IsDirty()
    {
        return _isDirty;
    }

    public void SaveIntoCampaign(Campaign campaign)
    {

    }

    public void LoadFromCampaign(Campaign campaign)
    {

    }

    public void Clear()
    {

    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if (newEditMode != EditMode.Terrain)
        {
            // Cleanup
        }
    }
}
