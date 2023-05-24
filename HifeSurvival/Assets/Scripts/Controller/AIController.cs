using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIController : ControllerBase
{
    [SerializeField] private MonsterBase _monsterPrefab;

    private Dictionary<int, MonsterBase> _monsterDic = new Dictionary<int, MonsterBase>();

    private WorldMap _worldMap;

   
    private void Start()
    {
        _worldMap = GameObject.Find(nameof(WorldMap))?.GetComponent<WorldMap>();

        if (_worldMap == null)
            return;
    }


    public void OnCommand(IPacket inPacket)
    {

    }
}

