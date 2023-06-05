using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldSpawn : WorldObjectBase
{
    public class SpawnData
    {
        public int spawnType;
        public int groupId;
        public List<Vector3> pivotList;
    }

    public enum ESpawnType
    {
        PLAYER,

        MONSTER,

        ITEM,

        NONE,
    }


    [SerializeField] private ESpawnType _spawnType;
    [SerializeField] private int _groupId;
    [SerializeField] Transform[] _pivotArr;


    public ESpawnType SpawnType { get => _spawnType; }
    public int GroupId { get => _groupId; }


    //-----------------
    // functions
    //-----------------


    public Vector3 GetSpawnWorldPos(int inIdx)
    {
        if (inIdx < 0 || inIdx >= _pivotArr.Length)
        {
            Debug.LogWarning("pivotArr is invalied");
            return default;
        }

        return _pivotArr[inIdx].position;
    }


    public int GetPivotCount()
    {
        return _pivotArr?.Length ?? 0;
    }


    public SpawnData GetSpawnData()
    {
        return new SpawnData()
        {
            spawnType = (int)_spawnType,
            groupId = _groupId,
            pivotList = _pivotArr.Select(x=>x.position).ToList(),
        };
    }
}
