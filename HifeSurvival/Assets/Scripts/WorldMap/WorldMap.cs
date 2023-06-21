using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;


public class WorldMap : MonoBehaviour
{
    [SerializeField] private Tilemap    _ground;
    [SerializeField] private Tilemap    _wall;
    [SerializeField] private Tilemap    _collider;

    [SerializeField] private Transform  _objectRoot;

    private Dictionary<Type, List<WorldObjectBase>> _worldObjDict;
    private Dictionary<Vector3Int, WorldTile> _bgTileDic;
    
    private Material _wallMat;

    private ObjectPoolController _objectPoolController;

    public void Init()
    {
        // 타일맵 세팅
        SetupToTilemap();

        // 월드 오브젝트 설정
        SetupToWorldObject();

        _objectPoolController = ControllerManager.Instance.GetController<ObjectPoolController>();

        GameMode.Instance.OnRecvDropRewardHandler   += OnRecvDropReward;
        GameMode.Instance.OnRecvGetItemHandler      += OnRecvGetItem;
        GameMode.Instance.OnRecvGetGoldHandler      += OnRecvGetGold;

    }


    public void SetupToTilemap()
    {
        _wallMat = _wall.GetComponent<TilemapRenderer>()?.sharedMaterial;
        _bgTileDic = GetAllTilePositionsAndTileBases();
    }


    public void SetupToWorldObject()
    {
        _worldObjDict = new Dictionary<Type, List<WorldObjectBase>>();

        var typeArr = Assembly.GetAssembly(typeof(WorldObjectBase)).GetTypes();
        var derivedTypes = typeArr.Where(x => x.IsClass == true &&
                                              x.IsAbstract == false &&
                                              x.IsSubclassOf(typeof(WorldObjectBase)))
                                  .ToList();


        var stack = new Stack<Transform>();
        stack.Push(_objectRoot);

        while (stack.Count > 0)
        {
            var tr = stack.Pop();

            foreach (Transform child in tr)
            {
                var worldObj = child.GetComponent<WorldObjectBase>();

                if (worldObj != null)
                {
                    var type = worldObj.GetType();
                    AddWorldObject(worldObj);
                }

                if (child.childCount > 0)
                    stack.Push(child);
            }
        }
    }

    public void SetPosWallMask(Vector3 inWorldPos)
    {
        _wallMat?.SetVector("_StarPosition", inWorldPos);
    }


    public void UpdateWallMasking(Vector3 hitPoint, Vector3 inWorldPos)
    {
        Vector3Int wallCoord = _wall.WorldToCell(hitPoint);
        Vector3Int targetCoord = _wall.WorldToCell(transform.position);

        if ((targetCoord.x > wallCoord.x || targetCoord.y > wallCoord.y) ||
             targetCoord == wallCoord)
        {
            SetPosWallMask(inWorldPos);
        }
        else
        {
            DoneWallMasking();
        }
    }

    public void DoneWallMasking()
    {
        SetPosWallMask(new Vector3(-9999, -9999, -9999));
    }


    public IEnumerable<T> GetWorldObjEnumerable<T>() where T : WorldObjectBase
    {
        return _worldObjDict.TryGetValue(typeof(T), out var list) ? list.Cast<T>()
                                                                 : Enumerable.Empty<T>();
    }


    public void AddWorldObject<T>(T inObj) where T : WorldObjectBase
    {
        if(_worldObjDict.TryGetValue(typeof(T), out var list) == true)
        {
            list.Add(inObj);
        }
        else
        {
            var objList = new List<WorldObjectBase>();
            objList.Add(inObj);

            _worldObjDict.Add(typeof(T), objList);
        }
    }

    public void RemoveWorldObject<T>(T inObj) where T : WorldObjectBase
    {
        if(_worldObjDict.TryGetValue(inObj.GetType(), out var list) == true)
        {
            list.Remove(inObj);
        }
    }


    public Vector3Int GetCoord(Vector3 inWorldPos)
    {
        Vector3Int cellPosition = _ground.WorldToCell(inWorldPos);
        return cellPosition;
    }


    public Vector3 GetWorldPos(Vector3Int inCoord, Vector3 inOffset = default)
    {
        Vector3 worldPos = _ground.CellToWorld(inCoord) + new Vector3(0, +_ground.cellSize.y / 2, 0) + inOffset;
        return worldPos;
    }


    public bool CanGo(Vector3Int inCoord)
    {
        if (_bgTileDic.TryGetValue(inCoord, out var tile) == false)
            return false;

        return tile.IsBlock == false;
    }


    private Dictionary<Vector3Int, WorldTile> GetAllTilePositionsAndTileBases()
    {
        Dictionary<Vector3Int, WorldTile> tiles = new Dictionary<Vector3Int, WorldTile>();
        BoundsInt bounds = _ground.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = _ground.GetTile(cellPosition);

                if (tile is WorldTile worldTile)
                {
                    tiles.Add(cellPosition, worldTile);
                }
            }
        }

        return tiles;
    }


    public List<Vector3> GetBackgroundCanGoTileList()
    {
        List<Vector3> result = new List<Vector3>();
        BoundsInt bounds = _ground.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = _ground.GetTile(cellPosition);

                if (tile is WorldTile worldTile && worldTile.IsBlock == false)
                    result.Add(cellPosition);
            }
        }

        return result;
    }


    public void PickReward(int inWorldId)
    {
        var itemObj = GetWorldObjEnumerable<WorldItem>().FirstOrDefault(x => x.WorldId == inWorldId);

        if (itemObj == null)
        {
            Debug.LogError($"[{nameof(PickReward)}] itemObject is null or empty!");
            return;
        }

        itemObj.PlayGetItem(() => 
        {
            RemoveWorldObject(itemObj);
            _objectPoolController.StoreToPool(itemObj); 
        });
    }


    //-------------
    // Server
    //-------------

    public void OnRecvDropReward(S_DropReward inPacket)
    {
        var itemObj = _objectPoolController.SpawnFromPool<WorldItem>();

        if(itemObj == null)
        {
            Debug.LogError($"[{nameof(OnRecvDropReward)}] itemObj is null or empty!");
            return;
        }

        itemObj.SetInfo(inPacket.worldId, inPacket.pos.ConvertUnityVector3(), inPacket.rewardType);
        itemObj.PlayDropItem();

        AddWorldObject(itemObj);
    }

    public void OnRecvGetItem(S_GetItem inPacket)
    {
        PickReward(inPacket.worldId);
    }

    public void OnRecvGetGold(S_GetGold inPacket)
    {
        PickReward(inPacket.worldId);
    }


    #region Unity Editor 관련
#if UNITY_EDITOR

    public void LogTotalBounds()
    {
        BoundsInt bounds = _ground.cellBounds;
        int width = bounds.size.x;
        int height = bounds.size.y;
        int tileCount = width * height;

        Debug.Log("Width: " + width + ", Height: " + height + ", Tile Count: " + tileCount);
    }


    private void DrawCoord()
    {
        float textSize = 1.0f;
        Color textColor = Color.white;
        Vector3 textOffset = Vector3.up;

        int count = 0;

        if (_bgTileDic == null)
            _bgTileDic = GetAllTilePositionsAndTileBases();

        foreach (var coord in _bgTileDic.Keys)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = Mathf.FloorToInt(10 * textSize);
            style.normal.textColor = textColor;

            // Handles.Label(_background.CellToWorld(coord) + _background.tileAnchor + textOffset, $"({coord.x},{coord.y}),\n{GetWorldPos(coord)}", style);
            Handles.Label(_ground.CellToWorld(coord) + _ground.tileAnchor + textOffset, $"({coord.x},{coord.y})", style);

            count++;
        }
    }

    private void OnDrawGizmos()
    {
        // DrawCoord();
    }


    private void OnValidate()
    {
        _ground?.CompressBounds();
    }

#endif
    #endregion
}
