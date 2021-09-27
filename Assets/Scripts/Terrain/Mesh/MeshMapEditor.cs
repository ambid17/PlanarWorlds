using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TerrainModificationMode
{
    Raise,
    Lower,
    SetHeight,
    Smooth,
    Paint
}

public class MeshMapEditor : MonoBehaviour
{
    [Header("Settings")]
    public int brushSize;
    public float brushStrength;
    public float brushHeight;
    public LayerMask modificationLayerMask;
    public MeshMapInspector mapInspector;
    public TerrainLayerTextures terrainLayerTextures;

    // State
    public TerrainModificationMode currentMode;
    public Terrain terrain;
    private int _terrainResolution;
    private TerrainData _terrainData;

    private Camera _mainCamera;
    private bool _isDirty;
    private UIManager _uiManager;
    private TerrainManager _terrainManager;
    private int currentSplatMapIndex;

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
            TryModification();
        }
        if (Input.GetMouseButtonUp(0))
        {
            // This makes the process much quicker, see:
            // https://docs.unity3d.com/ScriptReference/TerrainData.SetHeightsDelayLOD.html
            _terrainData.SyncHeightmap();
        }

        if (Input.GetKeyDown(KeyCode.Z))
            SwitchTerrainModificationMode(TerrainModificationMode.Raise);
        if (Input.GetKeyDown(KeyCode.X))
            SwitchTerrainModificationMode(TerrainModificationMode.Lower);
        if (Input.GetKeyDown(KeyCode.C))
            SwitchTerrainModificationMode(TerrainModificationMode.SetHeight);
        if (Input.GetKeyDown(KeyCode.C))
            SwitchTerrainModificationMode(TerrainModificationMode.Smooth);
        if (Input.GetKeyDown(KeyCode.C))
            SwitchTerrainModificationMode(TerrainModificationMode.Paint);
    }

    private void TryModification()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, float.MaxValue, modificationLayerMask))
        {
            if(currentMode == TerrainModificationMode.Paint)
            {
                PaintTerrain(hit.point);
            }
            else
            {
                ModifyTerrain(hit.point);
            }
        }
    }

    private void ModifyTerrain(Vector3 hitPoint)
    {
        (int startingXIndex, int startingYIndex) = GetTerrainIndicesForRay(hitPoint);

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
                switch (currentMode)
                {
                    case TerrainModificationMode.Raise:
                        heights[x, y] += modificationAmount;
                        break;
                    case TerrainModificationMode.Lower:
                        heights[x, y] -= modificationAmount;
                        break;
                    case TerrainModificationMode.SetHeight:
                        heights[x, y] = brushHeight;
                        break;
                    case TerrainModificationMode.Smooth:
                        heights[x, y] = Mathf.MoveTowards(heights[x, y], GetAverageHeight(heights), Time.deltaTime * brushStrength);
                        break;
                }
            }
        }

        _terrainData.SetHeightsDelayLOD(startingXIndex, startingYIndex, heights);
    }

    private void PaintTerrain(Vector3 hitPoint)
    {
        (int startingXIndex, int startingYIndex) = GetTerrainIndicesForRay(hitPoint);

        if (startingXIndex < 0 || startingYIndex < 0)
            return;

        float [,,] splatMaps = _terrainData.GetAlphamaps(startingXIndex, startingYIndex, brushSize, brushSize);

        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                float[] weights = new float[_terrainData.alphamapLayers];

                for (int z = 0; z < splatMaps.GetLength(2); z++)
                {
                    weights[z] = splatMaps[x, y, z];
                }

                weights[currentSplatMapIndex] += brushStrength;

                float sum = weights.Sum();

                for (int w = 0; w < weights.Length; w++)
                {
                    weights[w] /= sum;
                    splatMaps[x, y, w] = weights[w];
                }
            }
        }

        _terrainData.SetAlphamaps(startingXIndex, startingYIndex, splatMaps);
        terrain.Flush();
    }

    private (int, int) GetTerrainIndicesForRay(Vector3 hitPoint)
    {
        int brushRadius = brushSize / 2;

        float relativeHitX = hitPoint.x - terrain.transform.position.x;
        float relativeHitY = hitPoint.z - terrain.transform.position.z;

        int terrainX = (int)((relativeHitX / _terrainData.size.x) * _terrainResolution);
        int terrainY = (int)((relativeHitY / _terrainData.size.z) * _terrainResolution);

        int startingXIndex = terrainX - brushRadius;
        int startingYIndex = terrainY - brushRadius;

        return (startingXIndex, startingYIndex);
    }

    private float GetAverageHeight(float[,] heights)
    {
        float total = 0; 
        int count = 0;

        foreach(float height in heights)
        {
            total += height;
            count++;
        }

        return total / count;
    }

    public void SelectSplatMap(TerrainLayerTexture layer)
    {
        currentSplatMapIndex = TryAddTerrainLayer(layer.diffuse, layer.normal); ;
    }

    private int TryAddTerrainLayer(Texture2D diffuseTexture, Texture2D normalTexture)
    {
        TerrainLayer newLayer = new TerrainLayer();
        newLayer.diffuseTexture = diffuseTexture;

        if(normalTexture != null)
            newLayer.normalMapTexture = normalTexture;

        TerrainLayer[] oldLayers = _terrainData.terrainLayers;

        // check to see that we are not adding a duplicate TerrainLayer
        for (int i = 0; i < oldLayers.Length; ++i)
        {
            if (oldLayers[i].diffuseTexture == newLayer.diffuseTexture)
                return i;
        }

        TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];

        // copy old array into new array
        Array.Copy(oldLayers, newLayers, oldLayers.Length);

        // add new TerrainLayer to the new array
        newLayers[newLayers.Length - 1] = newLayer;
        terrain.terrainData.terrainLayers = newLayers;
        return newLayers.Length - 1;
    }

    public void SetBrushHeight(float newHeight)
    {
        brushHeight = newHeight / _terrainData.size.y;
    }

    public void SwitchTerrainModificationMode(TerrainModificationMode mode)
    {
        currentMode = mode;
        mapInspector.TerrainModificationModeChanged(mode);
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
