using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileList", menuName = "ScriptableObjects/TileList")]
public class TileList : ScriptableObject
{
    public Tile[] tiles;
}
