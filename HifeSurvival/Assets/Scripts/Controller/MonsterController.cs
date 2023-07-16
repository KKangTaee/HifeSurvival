using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MonsterController : EntityObjectController<Monster>,
    IUpdateSpawnMonsterBroadcast, IUpdateStatBroadcast
{
    [Serializable]
    public class MonsterPrefabs
    {
        public int grade;
        public Monster prefab;
    }

    [SerializeField] List<MonsterPrefabs> _prefabList;

    public override void Init()
    {
        base.Init();

        var eventHandler = _gameMode.GetEventHandler<IngamePacketEventHandler>();

        eventHandler.RegisterClient<UpdateSpawnMonsterBroadcast>(OnUpdateSpwanMonsterBroadcast);
        eventHandler.RegisterClient<UpdateStatBroadcast>(OnUpdateStatBroadcast);

        LoadMonster();
    }

    public void LoadMonster()
    {
        var entitys = GameMode.Instance.MonsterEntityDict.Values;

        foreach (var entity in entitys)
            CreateMonsterObject(entity);
    }

    public void OnUpdateSpwanMonsterBroadcast(UpdateSpawnMonsterBroadcast packet)
    {
        foreach(MonsterSpawn m in packet.monsterList)
        {
            var entity = _gameMode.GetMonsterEntity(m.id);
            CreateMonsterObject(entity);
        }
    }

    public void CreateMonsterObject(MonsterEntity entity)
    {
        var prefabData = _prefabList.FirstOrDefault(x => x.grade == entity.grade);

        if (prefabData == null)
        {
            Debug.LogError("prefab is null or empty!");
            return;
        }

        var inst = Instantiate(prefabData.prefab, transform);
        inst.Init(entity, entity.pos.ConvertUnityVector3());
        inst.SetMonster(entity.monsterId);

        _entityObjectDict.Add(inst.id, inst);
    }

    public void OnUpdateStatBroadcast(UpdateStatBroadcast packet)
    {
        if(ContainEntity(packet.id) == false)
           return;

        var target = GetEntityObject(packet.id);

        target.UpdateHp();
    }
}
