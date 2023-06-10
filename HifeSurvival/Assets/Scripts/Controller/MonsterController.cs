using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MonsterController : EntityObjectController<Monster>
{
    [Serializable]
    public class MonsterPrefabs
    {
        public int id;
        public Monster prefab;
    }

    [SerializeField] List<MonsterPrefabs> _prefabList;

    public override void Init()
    {
        base.Init();

        LoadMonster();
    }

    public void LoadMonster()
    {
        var entitys = GameMode.Instance.MonsterEntityDict.Values;

        foreach (var entity in entitys)
        {
            var prefabData = _prefabList.FirstOrDefault(x=>x.id == entity.monsterId);

            if(prefabData == null)
            {
                Debug.LogError("prefab is null or empty!");
                return;
            }

            var inst = Instantiate(prefabData.prefab, transform);
            inst.Init(entity.targetId, entity.stat, entity.pos.ConvertUnityVector3());

            _entityObjectDict.Add(inst.TargetId, inst);
        }
    }
}
