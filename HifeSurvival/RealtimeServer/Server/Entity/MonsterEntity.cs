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
                AIController.AddAggro(attacker);
                Attack(new AttackParam()
                {
                    target = attacker,
                });
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
                if (inParam is AttackParam attack)
                {
                    updateAttack(inSelf, (PlayerEntity)attack.target);
                }
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }


            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is AttackParam attack)
                {
                    updateAttack(inSelf, (PlayerEntity)attack.target);
                }
            }

            private void updateAttack(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this == null || inSelf == null)
                    return;

                if(inSelf.AIController.ExecuteAttack(inOther, out int damageValue))
                {
                    CS_Attack attackPacket = new CS_Attack()
                    {
                        toIsPlayer = true,
                        toId = inOther.targetId,
                        fromIsPlayer = false,
                        fromId = inSelf.targetId,
                        attackValue = damageValue,
                    };
                    inSelf.broadcaster.Broadcast(attackPacket);
                }

                JobTimer.Instance.Push(() => {
                    inSelf.Attack(new AttackParam()
                    {
                        target = inOther,
                    });
                }, (int)(inOther.stat.attackSpeed * 1000));
            }
        }

        public class DeadState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                inSelf.DropItem();
                inSelf.StartRespawning();
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
                Logger.GetInstance().Info($"<Monster id {inSelf.targetId}> Move current : {param.currentPos.PrintPVec3()}, target : {param.targetPos.PrintPVec3()}");

                updateMove(inSelf, param);
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                var param = (MoveParam)inParam;
                Logger.GetInstance().Info($"<Monster id {inSelf.targetId}> Move current : {param.currentPos.PrintPVec3()}, target : {param.targetPos.PrintPVec3()}");

                updateMove(inSelf, param);
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            private void updateMove(MonsterEntity inSelf, MoveParam inParam)
            {
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