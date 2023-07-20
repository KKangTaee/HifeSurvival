using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MonsterController : EntityObjectController<Monster>,
    IUpdateSpawnMonsterBroadcast, IUpdateStatBroadcast
{

    public const string PREFAB_PATH = "Prefabs/Monsters";

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
        var prefab = Resources.Load<Monster>($"{PREFAB_PATH}/Monster_{entity.monsterKey}");

        var inst = Instantiate(prefab, transform);
        inst.Init(entity, entity.pos.ConvertUnityVector3());
        inst.SetMonster();
        
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
