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

        StateMachine<MonsterEntity> _stateMachine;

        public override bool IsPlayer => false;

        public event Action<AttackParam> OnAttackHandler;

        public string rewardDatas;


        private Stack<int/*Id*/> aggroStack = new Stack<int>();
        private MonsterGroup group;


        public MonsterEntity(MonsterGroup group)
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
        }


        //----------------
        // overrides
        //----------------

        protected override void ChangeState<P>(EStatus inStatue, P inParam)
        {
            _stateMachine.OnChangeState(inStatue, this, inParam);
        }

        public override void OnDamaged(in Entity inEntity)
        {
            if (inEntity.IsPlayer)
            {
                var attackParam = new AttackParam()
                {
                    target = inEntity
                };

                Attack(attackParam);
                OnAttackHandler?.Invoke(attackParam);
            }
        }

        //-----------------
        // functions
        //-----------------


        //battle
        public bool CanAttack(in PVec3 inPos)
        {
            return pos.DistanceTo(inPos) < stat.attackRange;
        }

        //battle
        public bool OutOfSpawnRange()
        {
            return pos.DistanceTo(spawnPos) > 10;
        }

        public void AddAggro(int id)
        {
            aggroStack.Push(id);
        }

        public void ClearAggro()
        {
            aggroStack.Clear();
        }

        public int PopBackAggroID()
        {
            return aggroStack.Pop();
        }

        public bool IsGroupAllDead()
        {
            return group.IsAllDead();
        }

        public void StartRespawning()
        {
            group.SendRespawnGroup();
        }

        public void NotifyAttack(AttackParam inParam)
        {
            // NOTE@taeho.kang 죽은상태에서는 어그로 노티파이가 되어선 안됨.
            if(Status == EStatus.DEAD)
               return;

            // NOTE@taeho.kang 현재 몬스터가 공격을 하고 있지 않는 상황에만 어그로를 끈다.
            if (Status != EStatus.ATTACK)
                Attack(inParam);
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
                    inSelf.AddAggro(attack.target.targetId);
                    updateAttack(inSelf, (PlayerEntity)attack.target);
                }
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }


            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                //updateAttack(inSelf, (PlayerEntity)((AttackParam)inParam).target);
            }

            private void updateAttack(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this == null || inSelf == null)
                    return;

                if (inOther.Status == EStatus.DEAD)
                {
                    inSelf.ClearAggro();
                    inSelf.MoveToRespawn();
                }
                else if (inSelf.CanAttack(inOther.currentPos))
                {
                    var attackVal = inSelf.GetAttackValue();
                    var damagedVal = inOther.GetDamagedValue(attackVal);

                    inOther.ReduceHP(damagedVal);

                    // 공격하던 플레이어가 사망했다면..?
                    if (inOther.stat.currHp <= 0)
                    {
                        inSelf.ClearAggro(); 

                        S_Dead deadPacket = new S_Dead()
                        {
                            toIsPlayer = true,
                            toId = inOther.targetId,
                            fromIsPlayer = false,
                            fromId = inSelf.targetId,
                            respawnTime = 15,
                        };

                        inOther.Dead();
                        inSelf.broadcaster.Broadcast(deadPacket);

                        // 다시 자리로 돌아간다.
                        inSelf.MoveToRespawn();
                    }
                    else
                    {
                        CS_Attack attackPacket = new CS_Attack()
                        {
                            toIsPlayer = true,
                            toId = inOther.targetId,
                            fromIsPlayer = false,
                            fromId = inSelf.targetId,
                            attackValue = damagedVal,
                        };

                        inSelf.broadcaster.Broadcast(attackPacket);

                        JobTimer.Instance.Push(() => { updateAttack(inSelf, inOther); }, (int)(inOther.stat.attackSpeed * 1000));
                    }

                }
                // 공격을 못한다면 다시추격
                else
                {
                    inSelf.MoveToTarget(inOther.currentPos);
                }
            }
        }

        public class DeadState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
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
                    status = (int)EStatus.MOVE,
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

            RegisterAttackHandler(inEntity);
        }

        public void Remove(MonsterEntity inEntity)
        {
            _monstersDict.Remove(inEntity.targetId);

            UnRegisterAttackHandler(inEntity);
        }

        private void RegisterAttackHandler(MonsterEntity inEntity)
        {
            foreach (var otherEntity in _monstersDict.Values)
            {
                if (inEntity == otherEntity)
                    continue;

                inEntity.OnAttackHandler += otherEntity.NotifyAttack;
                otherEntity.OnAttackHandler += inEntity.NotifyAttack; // Assuming bi-directional attack
            }
        }

        private void UnRegisterAttackHandler(MonsterEntity inEntity)
        {
            foreach (var fromEntity in _monstersDict.Values)
            {
                if (fromEntity == inEntity)
                    continue;

                fromEntity.OnAttackHandler -= inEntity.NotifyAttack;
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