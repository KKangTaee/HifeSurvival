using ServerCore;
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

        public MonsterGroup(int groupId, int respawnTime)
        {
            GroupId = groupId;
            RespawnTime = respawnTime;
        }

        public void Add(MonsterEntity monsterEntity)
        {
            _monstersDict.Add(monsterEntity.id, monsterEntity);
        }

        public void OnPlayStart()
        {
            foreach (var m in _monstersDict)
            {
                m.Value.ExecuteAI();
            }
        }

        public void UpdateStat()
        {
            foreach (var m in _monstersDict)
            {
                m.Value.UpdateStat();
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

        public MonsterEntity GetMonsterEntity(int id)
        {
            if (_monstersDict.TryGetValue(id, out var monster) && monster != null)
            {
                return monster;
            }

            Logger.GetInstance().Error("MonsterEntity is null or empty!");
            return null;
        }

        public bool HaveEntityInGroup(int id)
        {
            return _monstersDict.ContainsKey(id);
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
                    entity.stat.AddCurrHp(entity.stat.MaxHp);

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
