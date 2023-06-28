﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class MonsterGroup
    {
        private Dictionary<int, MonsterEntity> _monstersDict = new Dictionary<int, MonsterEntity>();

        public int GroupId { get; private set; }
        public int RespawnTime { get; private set; }

        public MonsterGroup(int inGroupId, int inRespawnTime)
        {
            GroupId = inGroupId;
            RespawnTime = inRespawnTime;
        }

        public void Add(MonsterEntity inEntity)
        {
            _monstersDict.Add(inEntity.id, inEntity);
        }

        public void OnPlayStart()
        {
            foreach (var m in _monstersDict)
            {
                m.Value.UpdateStat();
                m.Value.ExecuteAI();
            }
        }

        public void OnAttack(int damagedId, Entity attacker)
        {
            foreach (var m in _monstersDict.AsQueryable().Where(m => m.Value.id != damagedId && !m.Value.ExistAggro()))
                m.Value.Attack(new AttackParam() 
                { 
                    target = attacker, 
                });
        }

        public MonsterEntity GetMonsterEntity(int inTargetId)
        {
            if (_monstersDict.TryGetValue(inTargetId, out var monster) && monster != null)
            {
                return monster;
            }

            Logger.GetInstance().Error("MonsterEntity is null or empty!");
            return null;
        }

        public bool HaveEntityInGroup(int inTargetId)
        {
            return _monstersDict.ContainsKey(inTargetId);
        }

        public bool IsAllDead()
        {
            return _monstersDict.Values.All(x => x.IsDead());
        }

        public void SendRespawnGroup()
        {
            JobTimer.Instance.Push(() =>
            {
                foreach (var entity in _monstersDict.Values)
                {
                    entity.Idle();
                    entity.stat.AddCurrHp(entity.stat.maxHp);

                    S_Respawn respawn = new S_Respawn()
                    {
                        id = entity.id,
                        stat = entity.stat.ConvertToPStat(),
                    };
                }
            }, RespawnTime * DEFINE.SEC_TO_MS);
        }
    }
}
