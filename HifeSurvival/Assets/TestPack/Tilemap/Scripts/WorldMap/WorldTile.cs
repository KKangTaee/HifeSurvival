using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldTile", menuName = "Tiles/WorldTile")]
public class WorldTile : Tile
{
    [SerializeField] private Vector3Int _coord;
    [SerializeField] private bool       _isBlock;


    public bool IsBlock { get => _isBlock; }
}