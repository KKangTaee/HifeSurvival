using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;


public class WorldMap : MonoBehaviour
{

    [SerializeField] private Tilemap    _background;
    [SerializeField] private Tilemap    _wall;
    [SerializeField] private Tilemap    _collider;
    [SerializeField] private Transform  _objectRoot;


    private Dictionary<Vector3Int, WorldTile> _bgTileDic;
    private Material _wallMat;
    private AStar    _aStar;


    Dictionary<Type, List<WorldObjectBase>> _worldObjDic;


    private void Awake()
    {
        Initialize();

        ControllerManager.Instance.Init();
    }

    private void Initialize()
    {
        // 타일맵 세팅
        SetupToTilemap();

        // Astar
        SetupToAStar();

        // 월드 오브젝트 생ㅓ
        SetupToWorldObject();
    }


    public void SetupToTilemap()
    {
        _wallMat = _wall.GetComponent<TilemapRenderer>()?.sharedMaterial;
        _bgTileDic = GetAllTilePositionsAndTileBases();
    }


    public void SetupToWorldObject()
    {
        _worldObjDic = new Dictionary<Type, List<WorldObjectBase>>();

        var typeArr = Assembly.GetAssembly(typeof(WorldObjectBase)).GetTypes();
        var derivedTypes = typeArr.Where(x => x.IsClass == true &&
                                              x.IsAbstract == false &&
                                              x.IsSubclassOf(typeof(WorldObjectBase)))
                                  .ToList();

        foreach(var type in derivedTypes)
        {
            _worldObjDic.Add(type, new List<WorldObjectBase>());
        }

        var stack = new Stack<Transform>();
        stack.Push(_objectRoot);


        while(stack.Count > 0)
        {
            var tr = stack.Pop();

            foreach(Transform child in tr)
            {
                var worldObj = child.GetComponent<WorldObjectBase>();

                if(worldObj != null)
                {
                    var type = worldObj.GetType();

                    if(_worldObjDic.ContainsKey(type) == true)
                       _worldObjDic[type].Add(worldObj);
                    
                }

                if(child.childCount > 0)
                    stack.Push(child);
            }
        }
    }

    public void SetupToAStar()
    {
        _aStar = new AStar((nextCoord) =>
        {
            if (_bgTileDic.TryGetValue(nextCoord, out var nextTile) == false)
                return false;

            if (nextTile.IsBlock == true)
                return false;

            return true;
        });
    }

    public IEnumerable<T> GetWorldObject<T>() where T : WorldObjectBase
    {
        return _worldObjDic.TryGetValue(typeof(T), out var list) ? list.Cast<T>() 
                                                                 : Enumerable.Empty<T>();
    }


    public void SetPosWallMask(Vector3 inWorldPos)
    {
        _wallMat?.SetVector("_StarPosition", inWorldPos);
    }


    public void UpdateWallMasking(Vector3 hitPoint, Vector3 inWorldPos)
    {
        // �浹�� ������ Ÿ�� ��ǥ��� ��ȯ�մϴ�.
        Vector3Int wallCoord   = _wall.WorldToCell(hitPoint);
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


    public Vector3Int GetCoord(Vector3 inWorldPos)
    {
        Vector3Int cellPosition = _background.WorldToCell(inWorldPos);
        return cellPosition;
    }


    public Vector3 GetWorldPos(Vector3Int inCoord, Vector3 inOffset = default)
    {
        Vector3 worldPos = _background.CellToWorld(inCoord) + new Vector3(0, +_background.cellSize.y / 2, 0) + inOffset;
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
        BoundsInt bounds = _background.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = _background.GetTile(cellPosition);

                if (tile is WorldTile worldTile)
                {
                    tiles.Add(cellPosition, worldTile);
                }
            }
        }

        return tiles;
    }



    public List<Vector3> GetMoveList(Vector3 inStartWorldPos, Vector3 inEndWorldPos)
    {
        Vector3Int startCoord = GetCoord(inStartWorldPos);
        Vector3Int endCoord = GetCoord(inEndWorldPos);

        if (CanGo(endCoord) == false)
            return null;

        var moveList = _aStar.Run(startCoord, endCoord);

        return moveList.Select(x => GetWorldPos(x)).ToList();
    }


    #region ��Ÿ
#if UNITY_EDITOR

    public void LogTotalBounds()
    {
        BoundsInt bounds = _background.cellBounds;

        // �ٿ���� �ʺ�� ���̸� ����Ͽ� Ÿ�� ���� ���ϱ�
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
            Handles.Label(_background.CellToWorld(coord) + _background.tileAnchor + textOffset, $"({coord.x},{coord.y})", style);

            count++;
        }
    }

    private void OnDrawGizmos()
    {
        // DrawCoord();
    }


    private void OnValidate()
    {
        _background?.CompressBounds();
    }

#endif
    #endregion
}
