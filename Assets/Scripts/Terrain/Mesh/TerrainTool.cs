using UnityEngine;

using System.Collections;

public class TerrainTool : MonoBehaviour
{
    public enum TerrainModificationMode
    {
        Raise,
        Lower,
        Flatten,
    }

    public int brushSize;
    [Range(0.001f, 0.1f)]
    public float brushStrength;
    public float brushHeigth;

    public TerrainModificationMode currentModificationAction;

    public LayerMask modificationLayerMask;

    public Terrain terrain;
    private int terrainResolution;
    private TerrainData terrainData;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(mainCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, float.MaxValue, modificationLayerMask))
            {
                ModifyTerrain(hit.point);
            }
        }

        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(mainCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, float.MaxValue, modificationLayerMask))
            {
                switch (currentModificationAction)
                {
                    case TerrainModificationMode.Flatten:
                        brushHeigth = SampleHeight(terrain, hit.point);
                        break;
                }
            }
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

    private void SwitchTerrainModificationMode(TerrainModificationMode mode)
    {
        currentModificationAction = mode;
    }

    public void ModifyTerrain(Vector3 hitPoint)
    {
        int brushRadius = brushSize / 2;

        Vector3 relativeHitPoint = (hitPoint - terrain.GetPosition());
        Vector3 normalizedHitPoint = new Vector3
            (
            (relativeHitPoint.x / terrainData.size.x),
            (relativeHitPoint.y / terrainData.size.y),
            (relativeHitPoint.z / terrainData.size.z)
            );

        Vector3 locationInTerrain = new Vector3(normalizedHitPoint.x * terrainResolution, 0, normalizedHitPoint.z * terrainResolution);

        int xBase = (int)locationInTerrain.x - brushRadius;
        int yBase = (int)locationInTerrain.z - brushRadius;
        float[,] heights = terrainData.GetHeights(xBase, yBase, brushSize, brushSize);

        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                switch (currentModificationAction)
                {
                    case TerrainModificationMode.Raise:
                        heights[x, y] += brushStrength * Time.smoothDeltaTime;
                        break;
                    case TerrainModificationMode.Lower:
                        heights[x, y] -= brushStrength * Time.smoothDeltaTime;
                        break;
                    case TerrainModificationMode.Flatten:
                        heights[x, y] = brushHeigth;
                        break;
                }
            }
        }

        terrainData.SetHeights(xBase, yBase, heights);
    }

    public float SampleHeight(Terrain terrain, Vector3 location)
    {
        Vector3 tempCoord = (location - terrain.GetPosition());
        Vector3 coord;

        coord = new Vector3
            (
            (tempCoord.x / terrainData.size.x),
            (tempCoord.y / terrainData.size.y),
            (tempCoord.z / terrainData.size.z)
            );

        Vector3 locationInTerrain = new Vector3(coord.x * terrainResolution, 0, coord.z * terrainResolution);

        int terX = (int)locationInTerrain.x;

        int terZ = (int)locationInTerrain.z;

        return Mathf.LerpUnclamped(0f, 1f, (terrain.terrainData.GetHeight(terX, terZ) / terrain.terrainData.size.y));
    }
}
