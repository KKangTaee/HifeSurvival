using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpawn : WorldObjectBase
{
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
    // unity events
    //-----------------




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


#if UNITY_EDITOR

    int _prevPivotCount = 0;

    private void OnValidate()
    {
        if (_prevPivotCount != _pivotArr?.Length)
        {
            for (int i = 0; i < _pivotArr.Length; i++)
            {
                if (_pivotArr[i] == null)
                {
                    GameObject newPivot = new GameObject($"pivot_{i}");
                    newPivot.transform.parent = transform;
                    _pivotArr[i] = newPivot.transform;
                }
            }
        }
    }

#endif
}
