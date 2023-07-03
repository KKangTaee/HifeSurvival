using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class WorldTilemap : MonoBehaviour
{
    [SerializeField] protected Tilemap          _tilemap;

    private Dictionary<Vector3Int, WorldTile> _tilemapDict = new Dictionary<Vector3Int, WorldTile>();
    
    public Material TilemapMat { get; private set; }


    //------------------
    // unity events
    //------------------

    protected virtual void Awake()
    {
        SetTile();
    }

    //----------------
    // functions
    //----------------
    
    private void SetTile()
    {
        _tilemapDict?.Clear();

        BoundsInt bounds = _tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = _tilemap.GetTile(cellPosition);

                if (tile is WorldTile worldTile)
                    _tilemapDict.Add(cellPosition, worldTile);
            }
        }

        TilemapMat = _tilemap.GetComponent<TilemapRenderer>().sharedMaterial;
    }


    public WorldTile GetTileByCell(Vector3Int cellPos)
    {
        if (_tilemapDict.TryGetValue(cellPos, out var tile) == false)
        {
            Debug.LogError($"[{nameof(GetTileByCell)}]tile is null or empty! ## cellPos : {cellPos}");
            return null;
        }

        return tile;
    }


    public Vector3 GetCellPos(Vector3 worldPos)
    {
        var cellPos = _tilemap.WorldToCell(worldPos);

        if (_tilemapDict.ContainsKey(cellPos) == false)
        {
            Debug.LogError($"[{nameof(GetCellPos)}] cellPos is invalied! ## cellPos : {cellPos}");
            return default;
        }

        return cellPos;
    }
}
