using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "TerrainLayers", menuName = "ScriptableObjects/TerrainLayers")]
public class TerrainLayerTextures : ScriptableObject
{
    [SerializeField]
    public List<TerrainLayerTexture> layers;
}

[Serializable]
public class TerrainLayerTexture
{
    [SerializeField]
    public Texture2D diffuse;
    [SerializeField]
    public Texture2D normal;
}
