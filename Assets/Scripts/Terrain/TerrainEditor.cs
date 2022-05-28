using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

public class TerrainEditor : MonoBehaviour
{
    [Header("Settings")]
    public int brushSize;
    public float brushStrength;
    public float brushHeight;
    public bool isErasing;
    public LayerMask modificationLayerMask;
    public TerrainInspectorUI terrainInspectorUI;
    
    public Material terrainMaterial;

    // State
    public TerrainModificationMode currentMode;
    public Terrain terrain;
    private int _terrainHeightMapResolution;
    private int _terrainAlphaMapResolution;
    private TerrainData _terrainData;

    private Camera _mainCamera;
    private bool _isDirty;

    private int currentTerrainLayerIndex;
    private int currentTreeIndex;
    private int currentFoliageIndex;



    private Vector3 _mouseDragLastPosition;
    private Vector3 _currentHitPoint;
    private bool _isDragging;
    private bool _hasMouseMoved; // Has the mouse moved since the last operation?
    
    private bool debugTerrainHighlight = true;


    void Awake()
    {
        _isDirty = false;
        _mainCamera = Camera.main;

        _terrainData = terrain.terrainData;
        _terrainHeightMapResolution = _terrainData.heightmapResolution;
        _terrainAlphaMapResolution = _terrainData.alphamapResolution;


        _isDragging = false;
        _hasMouseMoved = false;

    }

    private void Start()
    {
        UIManager.OnEditModeChanged += EditModeChanged;
        SetupTerrain();
    }

    void SetupTerrain()
    {
        //_terrainData.detailPrototypes
        SetupTerrainLayers();
        SetupDetailLayers();
        SetupTrees();
    }


    private void Update()
    {
        if (UIManager.Instance.EditMode != EditMode.Terrain || UIManager.Instance.isPaused || UIManager.Instance.isFileBrowserOpen)
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
        
        // Disable terrain highlight updates for debugging the shader
        if (Input.GetKeyDown(KeyCode.K))
        {
            debugTerrainHighlight = !debugTerrainHighlight;
        }
        if (!EventSystem.current.IsPointerOverGameObject() && debugTerrainHighlight)
        {
            UpdateTerrainHighlight();
        }

        if (HotKeyManager.GetKeyDown(HotKeyName.TerrainRaise))
            SwitchTerrainModificationMode(TerrainModificationMode.Raise);
        if (HotKeyManager.GetKeyDown(HotKeyName.TerrainLower))
            SwitchTerrainModificationMode(TerrainModificationMode.Lower);
        if (HotKeyManager.GetKeyDown(HotKeyName.TerrainSetHeight))
            SwitchTerrainModificationMode(TerrainModificationMode.SetHeight);
        if (HotKeyManager.GetKeyDown(HotKeyName.TerrainSmooth))
            SwitchTerrainModificationMode(TerrainModificationMode.Smooth);
        if (HotKeyManager.GetKeyDown(HotKeyName.TerrainPaint))
            SwitchTerrainModificationMode(TerrainModificationMode.Paint);
        if (HotKeyManager.GetKeyDown(HotKeyName.TerrainTree))
            SwitchTerrainModificationMode(TerrainModificationMode.Trees);
        if (HotKeyManager.GetKeyDown(HotKeyName.TerrainFoliage))
            SwitchTerrainModificationMode(TerrainModificationMode.Foliage);
    }

    private void OnApplicationQuit()
    {
        #if UNITY_EDITOR
                Clear();
        #endif
    }

    #region Modification
    private void TryModification()
    {
        // If we are just starting a drag, save off the starting points
        if(!_isDragging)
        {
            _mouseDragLastPosition = Input.mousePosition;
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, modificationLayerMask))
            {
                _currentHitPoint = hit.point;
                _isDragging = true;
                _hasMouseMoved = true;
            }
        }
        else // If we are in a drag, only raycast if the user has moved the mouse
        {
            Vector3 mouseDelta = Input.mousePosition - _mouseDragLastPosition;
            if (mouseDelta.magnitude > 20f)
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, modificationLayerMask))
                {
                    _currentHitPoint = hit.point;
                    _hasMouseMoved = true;
                    _mouseDragLastPosition = Input.mousePosition;
                }
            }
            else
            {
                _hasMouseMoved = false;
            }
        }

        if (currentMode == TerrainModificationMode.Paint)
        {
            PaintTerrain(_currentHitPoint);
        }
        else if(currentMode == TerrainModificationMode.Trees)
        {
            if (isErasing)
            {
                DeleteTree(_currentHitPoint);
            }
            else
            {
                AddTree(_currentHitPoint);
            }
        }
        else if (currentMode == TerrainModificationMode.Foliage)
        {
            if (isErasing)
            {
                DeleteDetail(_currentHitPoint);
            }
            else
            {
                AddDetail(_currentHitPoint);

            }
        }
        else
        {
            ModifyTerrain(_currentHitPoint);
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

        // TODO: fix the bounds of the terrain cursor, otherwise we get index out of bounds error
        startingXIndex = Mathf.Max(0, startingXIndex);
        startingXIndex = Mathf.Min(_terrainHeightMapResolution, startingXIndex);
        startingYIndex = Mathf.Max(0, startingYIndex);
        startingYIndex = Mathf.Min(_terrainHeightMapResolution, startingYIndex);


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
        if (!_hasMouseMoved)
        {
           return; 
        }
        
        float treePlacementCount = brushStrength * 10;
        
        for(int i = 0; i < treePlacementCount; i++)
        {
            float randomX = UnityEngine.Random.Range(-brushSize / 2, brushSize / 2);
            float randomY = UnityEngine.Random.Range(-brushSize / 2, brushSize / 2);

            TreeInstance instance = new TreeInstance();

            Vector3 relativePosition = hitPoint;
            relativePosition.x -= terrain.transform.position.x + randomX;
            relativePosition.z -= terrain.transform.position.z + randomY;
            relativePosition.x /= _terrainData.size.x;
            relativePosition.z /= _terrainData.size.x;

            instance.position = relativePosition;
            instance.prototypeIndex = currentTreeIndex;
            instance.color = new Color32(1, 1, 1, 1);
            instance.heightScale = 1;
            instance.widthScale = 1;
            terrain.AddTreeInstance(instance);
        }
        
        terrain.Flush();

        TreeInstance[] instances = terrain.terrainData.treeInstances;
    }

    // TODO: this is HORRIBLY inefficient, either need to:
    // - make multiple, smaller terrains
    // - make the trees gameobjects instead of baked into the terrain
    private void DeleteTree(Vector3 hitPoint)
    {
        TreeInstance[] trees = _terrainData.treeInstances;

        List<TreeInstance> newTreeInstances = new List<TreeInstance>();
        foreach (var treeInstance in trees)
        {
            Vector3 treePos = treeInstance.position;

            float xPos = (treePos.x - 0.5f) * terrain.terrainData.size.x;
            float zPos = (treePos.z - 0.5f) * terrain.terrainData.size.z;

            Vector3 treeWorldPosition = new Vector3(xPos, 0, zPos);
            
            
            Vector3 offset = treeWorldPosition - new Vector3(hitPoint.x, 0, hitPoint.z);
            
            // Only let this tree continue to exist if it's outside of the brush radius
            if (offset.magnitude > (float)brushSize / 2)
            {
                newTreeInstances.Add(treeInstance);
            }
        }

        _terrainData.treeInstances = newTreeInstances.ToArray();
    }

    private void AddDetail(Vector3 hitPoint)
    {
        int brushRadius = brushSize / 2;

        float relativeHitX = hitPoint.x - terrain.transform.position.x;
        float relativeHitY = hitPoint.z - terrain.transform.position.z;

        float terX = relativeHitX / _terrainData.size.x;
        float terY = relativeHitY / _terrainData.size.z;

        int terrainX = Mathf.RoundToInt(terX * _terrainData.detailResolution);
        int terrainY = Mathf.RoundToInt(terY * _terrainData.detailResolution);

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

    private void DeleteDetail(Vector3 hitPoint)
    {
        int brushRadius = brushSize / 2;

        float relativeHitX = hitPoint.x - terrain.transform.position.x;
        float relativeHitY = hitPoint.z - terrain.transform.position.z;

        float terX = relativeHitX / _terrainData.size.x;
        float terY = relativeHitY / _terrainData.size.z;

        int terrainX = Mathf.RoundToInt(terX * _terrainData.detailResolution);
        int terrainY = Mathf.RoundToInt(terY * _terrainData.detailResolution);

        int startingXIndex = terrainX - brushRadius;
        int startingYIndex = terrainY - brushRadius;

        startingXIndex = Mathf.Max(0, startingXIndex);
        startingYIndex = Mathf.Max(0, startingYIndex);

        int[,] details = _terrainData.GetDetailLayer(startingXIndex, startingYIndex, brushSize, brushSize, currentFoliageIndex);

        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                details[x, y] = 0;
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

    public void SetTerrainLayer(TerrainLayerTexture layer)
    {
        TerrainLayer[] oldLayers = _terrainData.terrainLayers;
        
        for (int i = 0; i < oldLayers.Length; ++i)
        {
            if (oldLayers[i].diffuseTexture == layer.diffuse)
            {
                currentTerrainLayerIndex = i;
                return;
            }
        }
    }
    
    /// <summary>
    /// Convert from our Scriptable Object to TerrainLayer, populating the terrain with the layers so they can be painted
    /// </summary>
    public void SetupTerrainLayers()
    {
        List<TerrainLayerTexture> layerTextures = TerrainManager.Instance.terrainLayers.layers;
        TerrainLayer[] newLayers = new TerrainLayer[layerTextures.Count];
        
        for(int i = 0; i < layerTextures.Count; i++)
        {
            TerrainLayer newLayer = new TerrainLayer();
            newLayer.diffuseTexture = layerTextures[i].diffuse;

            if (layerTextures[i].normal != null)
                newLayer.normalMapTexture = layerTextures[i].normal;

            newLayers[i] = newLayer;
        }
        
        terrain.terrainData.terrainLayers = newLayers;
    }

    public void SetTree(GameObject treePrefab)
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
    }

    public void SetupTrees()
    {
        Prefab[] trees = TerrainManager.Instance.treePrefabList.prefabs;
        TreePrototype[] newPrototypes = new TreePrototype[trees.Length];
        
        for(int i = 0; i < newPrototypes.Length; i++)
        {
            TreePrototype newPrototype = new TreePrototype();
            newPrototype.prefab = trees[i].gameObject;

            newPrototypes[i] = newPrototype;
        }
        
        terrain.terrainData.treePrototypes = newPrototypes;
    }

    public void SetDetailMesh(GameObject foliagePrefab)
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
    }
    
    public void SetupDetailLayers()
    {
        Prefab[] details = TerrainManager.Instance.foliagePrefabList.prefabs;
        DetailPrototype[] newPrototypes = new DetailPrototype[details.Length];
        
        for(int i = 0; i < newPrototypes.Length; i++)
        {
            DetailPrototype newPrototype = new DetailPrototype();
            newPrototype.usePrototypeMesh = true;
            newPrototype.prototype = details[i].gameObject;
            newPrototype.maxHeight = 2;
            newPrototype.maxWidth = 2;
            newPrototype.minHeight = 0.5f;
            newPrototype.minWidth = 0.5f;
            newPrototype.noiseSpread = 0.5f;
            newPrototype.renderMode = DetailRenderMode.Grass;

            newPrototypes[i] = newPrototype;
        }
        
        terrain.terrainData.detailPrototypes = newPrototypes;
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
        terrainInspectorUI.TerrainModificationModeChanged(mode);
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
            newModel.heightMap = _terrainData.GetHeights(0, 0, _terrainHeightMapResolution, _terrainHeightMapResolution);
            newModel.alphaMap = _terrainData.GetAlphamaps(0, 0, _terrainAlphaMapResolution, _terrainAlphaMapResolution);
            newModel.detailMap = GetDetailMap();
            newModel.treeMap = GetTreeMap();
        }catch(Exception e)
        {
            Debug.LogError($"Error saving mesh map: \n{e.Message}\n{e.StackTrace}");
        }

        campaign.terrainData = newModel;
    }

    private int[][,] GetDetailMap()
    {
        int[][,] detailMap = new int[TerrainManager.Instance.foliagePrefabList.prefabs.Length][,];
            
        for (int i = 0; i < TerrainManager.Instance.foliagePrefabList.prefabs.Length; i++)
        {
            detailMap[i] = _terrainData.GetDetailLayer(0, 0, _terrainData.detailResolution, _terrainData.detailResolution, i);
        }

        return detailMap;
    }

    private SerializedTree[] GetTreeMap()
    {
        SerializedTree[] trees = new SerializedTree[_terrainData.treeInstances.Length];
        for(int i = 0; i < _terrainData.treeInstances.Length; i++)
        {
            TreeInstance treeInstance = _terrainData.treeInstances[i];
            trees[i] = new SerializedTree()
            {
                x = treeInstance.position.x,
                y = treeInstance.position.y,
                z = treeInstance.position.z,
                prefabIndex = treeInstance.prototypeIndex
            };
        }

        return trees;
    }

    public void LoadFromCampaign(Campaign campaign)
    {
        if(campaign.terrainData != null)
        {
            try
            {
                _terrainData.SetHeights(0, 0, campaign.terrainData.heightMap);
                _terrainData.SetAlphamaps(0, 0, campaign.terrainData.alphaMap);
                LoadDetails(campaign.terrainData.detailMap);
                LoadTrees(campaign.terrainData.treeMap);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading mesh map: \n{e.Message}\n{e.StackTrace}");
            }
        }
    }
    
    private void LoadDetails(int[][,] detailMap)
    {
        for (int i = 0; i < TerrainManager.Instance.foliagePrefabList.prefabs.Length; i++)
        {
            _terrainData.SetDetailLayer(0, 0, i, detailMap[i]);
        }
    }
    
    private void LoadTrees(SerializedTree[] treeMap)
    {
        TreeInstance[] treeInstances = new TreeInstance[treeMap.Length];

        for (int i = 0; i < treeInstances.Length; i++)
        {
            SerializedTree serializedTree = treeMap[i];
            TreeInstance instance = new TreeInstance()
            {
                position = new Vector3(serializedTree.x, serializedTree.y, serializedTree.z),
                prototypeIndex = serializedTree.prefabIndex,
                heightScale = 1,
                widthScale = 1,
                color = new Color32(0,0,0,0),
                rotation = 0,
                lightmapColor = new Color32(0,0,0,0)
            };
                
            treeInstances[i] = instance;
        }
        _terrainData.SetTreeInstances(treeInstances, true);
    }

    public void Clear()
    {
        terrain.terrainData.terrainLayers = new TerrainLayer[0];
        terrain.terrainData.treeInstances = new TreeInstance[0];

        float[,] heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];
        terrain.terrainData.SetHeights(0, 0, heights);

        terrain.terrainData.detailPrototypes = new DetailPrototype[0];
        
        
        SetupTerrain();
    }

    private void EditModeChanged(EditMode newEditMode)
    {
        if (newEditMode != EditMode.Terrain)
        {
            terrainMaterial.SetVector("_Center", new Vector3(-10000, 0, -10000));
        }
    }
    #endregion
}
