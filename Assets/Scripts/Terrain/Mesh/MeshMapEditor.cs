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
    Paint,
    Trees,
    Foliage
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
    public PrefabList treePrefabList;
    public PrefabList foliagePrefabList;
    public Material terrainMaterial;

    // State
    public TerrainModificationMode currentMode;
    public Terrain terrain;
    private int _terrainHeightMapResolution;
    private int _terrainAlphaMapResolution;
    private TerrainData _terrainData;

    private Camera _mainCamera;
    private bool _isDirty;
    private UIManager _uiManager;
    private TerrainManager _terrainManager;

    private int currentTerrainLayerIndex;
    private int currentTreeIndex;
    private int currentFoliageIndex;
    private List<int> _terrainLayerIds;

    private Vector3 _mouseDragStartPosition;
    private Vector3 _startingHitPoint;
    private bool _isDragging;

    void Awake()
    {
        _isDirty = false;
        _mainCamera = Camera.main;

        _uiManager = UIManager.GetInstance();
        _terrainManager = TerrainManager.GetInstance();

        _terrainData = terrain.terrainData;
        _terrainHeightMapResolution = _terrainData.heightmapResolution;
        _terrainAlphaMapResolution = _terrainData.alphamapResolution;

        _terrainLayerIds = new List<int>();

        _isDragging = false;
    }

    private void Update()
    {
        if (_uiManager.EditMode != EditMode.Terrain || _uiManager.isPaused || _uiManager.isFileBrowserOpen || _terrainManager.currentTerrainMode == TerrainMode.TileMap)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = false;
        }
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            TryModification();
        }
        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            // This makes the process much quicker, see:
            // https://docs.unity3d.com/ScriptReference/TerrainData.SetHeightsDelayLOD.html
            _terrainData.SyncHeightmap();
        }
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            UpdateTerrainHighlight();
        }

        if (Input.GetKeyDown(KeyCode.Z))
            SwitchTerrainModificationMode(TerrainModificationMode.Raise);
        if (Input.GetKeyDown(KeyCode.X))
            SwitchTerrainModificationMode(TerrainModificationMode.Lower);
        if (Input.GetKeyDown(KeyCode.C))
            SwitchTerrainModificationMode(TerrainModificationMode.SetHeight);
        if (Input.GetKeyDown(KeyCode.V))
            SwitchTerrainModificationMode(TerrainModificationMode.Smooth);
        if (Input.GetKeyDown(KeyCode.B))
            SwitchTerrainModificationMode(TerrainModificationMode.Paint);
        if (Input.GetKeyDown(KeyCode.N))
            SwitchTerrainModificationMode(TerrainModificationMode.Trees);
        if (Input.GetKeyDown(KeyCode.M))
            SwitchTerrainModificationMode(TerrainModificationMode.Foliage);
    }

    private void OnApplicationQuit()
    {
        _terrainData.terrainLayers = new TerrainLayer[0];
        _terrainData = new TerrainData();
    }

    #region Modification
    private void TryModification()
    {
        // If we are just starting a drag, save off the starting points
        if(!_isDragging)
        {
            _mouseDragStartPosition = Input.mousePosition;
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, modificationLayerMask))
            {
                _startingHitPoint = hit.point;
            }
            _isDragging = true;
        }
        else // If we are in a drag, only raycast if the user has moved the mouse
        {
            Vector3 mouseDelta = Input.mousePosition - _mouseDragStartPosition;
            if (mouseDelta.magnitude > 1f)
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, modificationLayerMask))
                {
                    _startingHitPoint = hit.point;
                }
            }
        }

        if (currentMode == TerrainModificationMode.Paint)
        {
            PaintTerrain(_startingHitPoint);
        }
        else if(currentMode == TerrainModificationMode.Trees)
        {
            AddTree(_startingHitPoint);
        }
        else if (currentMode == TerrainModificationMode.Foliage)
        {
            AddDetail(_startingHitPoint);
        }
        else
        {
            ModifyTerrain(_startingHitPoint);
        }
    }

    private void ModifyTerrain(Vector3 hitPoint)
    {
        int brushRadius = brushSize / 2;

        float relativeHitX = hitPoint.x - terrain.transform.position.x;
        float relativeHitY = hitPoint.z - terrain.transform.position.z;

        float terX = relativeHitX / _terrainData.size.x;
        float terY = relativeHitY / _terrainData.size.z;

        int terrainX = Mathf.RoundToInt(terX * _terrainHeightMapResolution);
        int terrainY = Mathf.RoundToInt(terY * _terrainHeightMapResolution);

        int startingXIndex = terrainX - brushRadius;
        int startingYIndex = terrainY - brushRadius;

        startingXIndex = Mathf.Max(0, startingXIndex);
        startingYIndex = Mathf.Max(0, startingYIndex);


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
        int brushRadius = brushSize / 2;

        float relativeHitX = hitPoint.x - terrain.transform.position.x;
        float relativeHitY = hitPoint.z - terrain.transform.position.z;

        float terX = relativeHitX / _terrainData.size.x;
        float terY = relativeHitY / _terrainData.size.z;
        
        int terrainX = Mathf.RoundToInt(terX * _terrainAlphaMapResolution);
        int terrainY = Mathf.RoundToInt(terY * _terrainAlphaMapResolution);

        int startingXIndex = terrainX - brushRadius;
        int startingYIndex = terrainY - brushRadius;

        startingXIndex = Mathf.Max(0, startingXIndex);
        startingYIndex = Mathf.Max(0, startingYIndex);

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

                weights[currentTerrainLayerIndex] += brushStrength;

                float sum = weights.Sum();

                // Normalize the results to make sure they add up to 1
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

    private void AddTree(Vector3 hitPoint)
    {
        TreeInstance instance = new TreeInstance();
        instance.position = hitPoint;
        instance.prototypeIndex = currentTreeIndex;
        terrain.AddTreeInstance(instance);
        terrain.Flush();
    }

    private void AddDetail(Vector3 hitPoint)
    {
        int brushRadius = brushSize / 2;

        float relativeHitX = hitPoint.x - terrain.transform.position.x;
        float relativeHitY = hitPoint.z - terrain.transform.position.z;

        float terX = relativeHitX / _terrainData.size.x;
        float terY = relativeHitY / _terrainData.size.z;

        int terrainX = Mathf.RoundToInt(terX * _terrainAlphaMapResolution);
        int terrainY = Mathf.RoundToInt(terY * _terrainAlphaMapResolution);

        int startingXIndex = terrainX - brushRadius;
        int startingYIndex = terrainY - brushRadius;

        startingXIndex = Mathf.Max(0, startingXIndex);
        startingYIndex = Mathf.Max(0, startingYIndex);

        int[,] details = _terrainData.GetDetailLayer(startingXIndex, startingYIndex, brushSize, brushSize, currentFoliageIndex);

        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                details[x, y] = currentFoliageIndex;
            }
        }

        _terrainData.SetDetailLayer(startingXIndex, startingYIndex, currentFoliageIndex, details);
        terrain.Flush();
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

    private void UpdateTerrainHighlight()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, modificationLayerMask))
        {
            Vector3 hitPosition = hit.point;

            terrainMaterial.SetVector("_Center", hitPosition);

            float terrainWidth = _terrainData.size.x / 2;
            float scale = terrainWidth / brushSize;
            scale /= _terrainData.size.x;

            Vector2 textureOffset = new Vector2(hitPosition.x, hitPosition.z);
            float x = (textureOffset.x - brushSize) % (brushSize * 2);
            float y = (textureOffset.y - brushSize) % (brushSize * 2);
            textureOffset.x = 1 - (scale * x);
            textureOffset.y = 1 - (scale * y);
            terrainMaterial.SetTextureOffset("_MainTex", textureOffset);
        }
        else 
        {
            terrainMaterial.SetVector("_Center", new Vector3(-10000, 0, -10000));
        }
    }
    #endregion

    #region Settings
    public void TryAddTerrainLayer(TerrainLayerTexture layer)
    {
        TerrainLayer[] oldLayers = _terrainData.terrainLayers;

        // check to see that we are not adding a duplicate TerrainLayer
        for (int i = 0; i < oldLayers.Length; ++i)
        {
            if (oldLayers[i].diffuseTexture == layer.diffuse)
            {
                currentTerrainLayerIndex = i;
                return;
            }
        }

        TerrainLayer newLayer = new TerrainLayer();
        newLayer.diffuseTexture = layer.diffuse;

        if (layer.normal != null)
            newLayer.normalMapTexture = layer.normal;

        TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];

        // copy old array into new array
        Array.Copy(oldLayers, newLayers, oldLayers.Length);

        // add new TerrainLayer to the end of the new array
        int layerIndex = newLayers.Length - 1;
        newLayers[layerIndex] = newLayer;
        terrain.terrainData.terrainLayers = newLayers;
        currentTerrainLayerIndex = layerIndex;
        _terrainLayerIds.Add(layerIndex);
    }

    public void TryAddTree(GameObject treePrefab)
    {
        TreePrototype[] oldPrototypes = _terrainData.treePrototypes;

        for (int i = 0; i < oldPrototypes.Length; i++)
        {
            if(oldPrototypes[i].prefab.name == treePrefab.name)
            {
                currentTreeIndex = i;
                return;
            }
        }

        TreePrototype newPrototype = new TreePrototype();
        newPrototype.prefab = treePrefab;

        TreePrototype[] newPrototypes = new TreePrototype[oldPrototypes.Length + 1];

        // copy old array into new array
        Array.Copy(oldPrototypes, newPrototypes, oldPrototypes.Length);

        // add new TerrainLayer to the end of the new array
        int index = newPrototypes.Length - 1;
        newPrototypes[index] = newPrototype;
        terrain.terrainData.treePrototypes = newPrototypes;
        currentTreeIndex = index;
    }

    public void TryAddFoliageMesh(GameObject foliagePrefab)
    {
        DetailPrototype[] oldPrototypes = _terrainData.detailPrototypes;

        for (int i = 0; i < oldPrototypes.Length; i++)
        {
            if (oldPrototypes[i].prototype.name == foliagePrefab.name)
            {
                currentFoliageIndex = i;
                return;
            }
        }

        DetailPrototype newPrototype = new DetailPrototype();
        newPrototype.usePrototypeMesh = true;
        newPrototype.prototype = foliagePrefab;

        DetailPrototype[] newPrototypes = new DetailPrototype[oldPrototypes.Length + 1];

        // copy old array into new array
        Array.Copy(oldPrototypes, newPrototypes, oldPrototypes.Length);

        // add new TerrainLayer to the end of the new array
        int index = newPrototypes.Length - 1;
        newPrototypes[index] = newPrototype;
        terrain.terrainData.detailPrototypes = newPrototypes;
        currentFoliageIndex = index;
    }

    public void SetBrushHeight(float newHeight)
    {
        brushHeight = newHeight / _terrainData.size.y;
    }

    public void SetBrushSize(int newSize)
    {
        brushSize = newSize;

        terrainMaterial.SetFloat("_Radius", newSize);
        float terrainWidth = _terrainData.size.x / 2;
        float scale = terrainWidth / brushSize;
        terrainMaterial.SetTextureScale("_MainTex", new Vector2(scale, scale));
    }

    public void SwitchTerrainModificationMode(TerrainModificationMode mode)
    {
        currentMode = mode;
        mapInspector.TerrainModificationModeChanged(mode);
    }
    #endregion

    #region Utils
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
        TerrainModel newModel = new TerrainModel();
        try
        {
            float[] flattenedHeights = new float[_terrainHeightMapResolution * _terrainHeightMapResolution];
            float[,] a = _terrainData.GetHeights(0, 0, _terrainHeightMapResolution, _terrainHeightMapResolution);
            Buffer.BlockCopy(a, 0, flattenedHeights, 0, flattenedHeights.Length * sizeof(float));
            newModel.heightMap = flattenedHeights;

            float[] flattenedAlphaMaps = new float[_terrainAlphaMapResolution * _terrainAlphaMapResolution * _terrainLayerIds.Count];
            Buffer.BlockCopy(_terrainData.GetAlphamaps(0, 0, _terrainAlphaMapResolution, _terrainAlphaMapResolution), 0, flattenedAlphaMaps, 0, flattenedAlphaMaps.Length * sizeof(float));
            newModel.splatMap = flattenedAlphaMaps;
            newModel.textureIds = _terrainLayerIds.ToArray();
        }catch(Exception e)
        {
            Debug.LogError($"Error saving mesh map: \n{e.Message}\n{e.StackTrace}");
        }
        

        campaign.terrainData = newModel;
    }

    public void LoadFromCampaign(Campaign campaign)
    {
        if(campaign.terrainData != null)
        {
            try
            {
                float[,] unflattenedHeights = new float[_terrainHeightMapResolution, _terrainHeightMapResolution];
                Buffer.BlockCopy(campaign.terrainData.heightMap, 0, unflattenedHeights, 0, _terrainHeightMapResolution * _terrainHeightMapResolution);
                _terrainData.SetHeights(0, 0, unflattenedHeights);

                foreach (int id in campaign.terrainData.textureIds)
                {
                    TerrainLayerTexture texture = terrainLayerTextures.layers.Where(layer => layer.Id == id).FirstOrDefault();
                    if (texture != null)
                        TryAddTerrainLayer(texture);
                }

                float[,,] unflattenedAlphas = new float[_terrainAlphaMapResolution, _terrainAlphaMapResolution, campaign.terrainData.textureIds.Count()];
                Buffer.BlockCopy(campaign.terrainData.splatMap, 0, unflattenedAlphas, 0, _terrainAlphaMapResolution * _terrainAlphaMapResolution * campaign.terrainData.textureIds.Count());
                _terrainData.SetAlphamaps(0, 0, unflattenedAlphas);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error laoding mesh map: \n{e.Message}\n{e.StackTrace}");
            }
        }
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
    #endregion
}
