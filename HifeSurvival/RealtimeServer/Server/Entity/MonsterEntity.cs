using System;
using System.Collections.Generic;
using System.Linq;
using Server.Helper;
using ServerCore;

namespace Server
{
    public partial class MonsterEntity : Entity
    {
        public int monsterId;
        public int groupId;
        public int grade;
        public string rewardDatas;

        StateMachine<MonsterEntity> _stateMachine;

        public override bool IsPlayer => false;

        
        private MonsterAIController AIController { get; set; }
        private event Action<string, PVec3> dropItemDelegate;
        private MonsterGroup group;
        

        public MonsterEntity(MonsterGroup group, Action<string, PVec3> dropItem)
        {
            var smdic = new Dictionary<EStatus, IState<MonsterEntity, IStateParam>>();
            smdic[EStatus.IDLE] = new IdleState();
            smdic[EStatus.ATTACK] = new AttackState();
            smdic[EStatus.MOVE] = new MoveState();
            smdic[EStatus.USE_SKILL] = new UseSkillState();
            smdic[EStatus.DEAD] = new DeadState();

            _stateMachine = new StateMachine<MonsterEntity>(smdic);

            if(group != null)
            {
                this.group = group;
            }

            AIController = new MonsterAIController(this);
            dropItemDelegate += dropItem;
        }


        //----------------
        // overrides
        //----------------

        protected override void ChangeState<P>(EStatus inStatue, P inParam)
        {
            _stateMachine.OnChangeState(inStatue, this, inParam);
        }

        public override void OnDamaged(in Entity attacker)
        {
            if(IsDead())
            {
                Dead(new DeadParam()
                {
                    killerTarget = attacker,
                });

                return;
            }

            if (attacker.IsPlayer)
            {
                foreach (var monster in group.GetMonsterGroupIter())
                {
                    monster.Value.AIController.AddAggro(attacker);

                    monster.Value.Attack(new AttackParam()
                    {
                        target = attacker,
                    });
                }
            }
        }


        //-----------------
        // functions
        //-----------------


        //battle
        public bool CanAttack(in Entity target)
        {
            return currentPos.DistanceTo(target.currentPos) < stat.attackRange;
        }

        //battle
        public bool OutOfSpawnRange()
        {
            return currentPos.DistanceTo(spawnPos) > 10;
        }

        public bool IsGroupAllDead()
        {
            return group.IsAllDead();
        }

        public void StartRespawning()
        {
            group.SendRespawnGroup();
        }

        public void DropItem()
        {
            if (dropItemDelegate == null)
                return;

            dropItemDelegate(rewardDatas, currentPos);
            dropItemDelegate = null;
            return;
        }
    }



    //-----------------------
    // Monster StateMachine
    //-----------------------

    public partial class MonsterEntity
    {
        public class IdleState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                inSelf.AIController.UpdateNextMove(null);
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                
            }
        }

        public class AttackState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                inSelf.AIController.AttackRoutine();
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }


            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
            }
        }

        public class DeadState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is DeadParam deadParam)
                {
                    inSelf.AIController.OnMonsterDead();

                    S_Dead deadPacket = new S_Dead()
                    {
                        toIsPlayer = true,
                        toId = inSelf.targetId,
                        fromIsPlayer = false,
                        fromId = deadParam.killerTarget.targetId,
                        respawnTime = 15,
                    };
                    inSelf.broadcaster.Broadcast(deadPacket);

                    inSelf.DropItem();

                    if(inSelf.IsGroupAllDead())
                    {
                        inSelf.StartRespawning();
                    }
                }
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class MoveState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                var param = (MoveParam)inParam;
                updateMove(inSelf, param);
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                var param = (MoveParam)inParam;
                updateMove(inSelf, param);
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                
            }

            private void updateMove(MonsterEntity inSelf, MoveParam inParam)
            {
                Logger.GetInstance().Debug("UpdateMove");
                    
                inSelf.AIController.UpdateNextMove(inParam);
                inSelf.AIController.MoveRoutine();

                UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                {
                    targetId = inSelf.targetId,
                    isPlayer = inSelf.IsPlayer,
                    currentPos = inParam.currentPos,
                    targetPos = inParam.targetPos,
                    speed = inParam.speed,
                    timestamp = inParam.timestamp,
                };

                inSelf.broadcaster.Broadcast(move);
            }
        }

        public class UseSkillState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }
    }



    //-------------------
    // MonsterGroup
    //-------------------

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
            _monstersDict.Add(inEntity.targetId, inEntity);
        }

        public IEnumerable<KeyValuePair<int, MonsterEntity>> GetMonsterGroupIter()
        {
            foreach (KeyValuePair<int, MonsterEntity> kvp in _monstersDict)
            {
                yield return kvp;
            }
        }
        public MonsterEntity GetMonsterEntity(int inTargetId)
        {
            if (_monstersDict.TryGetValue(inTargetId, out var monster) == true && monster != null)
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
            return _monstersDict.Values.All(x => x.Status == Entity.EStatus.DEAD);
        }

        public void SendRespawnGroup()
        {
            JobTimer.Instance.Push(() =>
            {
                foreach (var entity in _monstersDict.Values)
                {
                    entity.Idle();
                    entity.stat.AddCurrHp(entity.stat.hp);

                    S_Respawn respawn = new S_Respawn()
                    {
                        targetId = entity.targetId,
                        isPlayer = false,
                        stat = entity.stat.ConvertStat(),
                    };
                }
            }, RespawnTime * 1000);
        }
    }
}