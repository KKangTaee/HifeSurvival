using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class MonsterGroup
    {
        public int GroupId { get; private set; }
        public int RespawnTime { get; private set; }

        private Dictionary<int, MonsterEntity> _monstersDict = new Dictionary<int, MonsterEntity>();
        private GameRoom _room;

        public MonsterGroup(GameRoom room, int groupId, int respawnTime)
        {
            _room = room;
            GroupId = groupId;
            RespawnTime = respawnTime;
        }

        public void Add(MonsterEntity monsterEntity)
        {
            _monstersDict.Add(monsterEntity.ID, monsterEntity);
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
            foreach (var m in _monstersDict.AsQueryable().Where(m => m.Value.ID != damagedId && !m.Value.ExistAggro()))
            {
                m.Value.Attack(new AttackParam()
                {
                    target = attacker,
                });
            }
        }

        public MonsterEntity GetMonsterEntity(int id)
        {
            if (_monstersDict.TryGetValue(id, out var monster) && monster != null)
            {
                return monster;
            }

            Logger.Instance.Error("MonsterEntity is null or empty!");
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
            _room.Worker.Push(() =>
            {
                foreach (var entity in _monstersDict.Values)
                {
                    entity.Idle();
                    entity.Stat.AddCurrHp(entity.Stat.MaxHp);

                    S_Respawn respawn = new S_Respawn()
                    {
                        id = entity.ID,
                        stat = entity.Stat.ConvertToPStat(),
                    };
                }
            }, RespawnTime * DEFINE.SEC_TO_MS);
        }
    }
}
