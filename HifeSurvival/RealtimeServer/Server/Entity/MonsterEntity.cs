using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public partial class MonsterEntity : Entity
    {
        public int monsterId;
        public int groupId;
        public int subId;

        StateMachine<MonsterEntity> _stateMachine;

        public override bool IsPlayer => false;

        public event Action<AttackParam> OnAttackHandler;

        public Action OnRespawnCallback;

        public MonsterEntity()
        {
            _stateMachine = new StateMachine<MonsterEntity>(
                new Dictionary<EStatus, IState<MonsterEntity>>()
                {
                    { EStatus.IDLE, new IdleState()},
                    { EStatus.FOLLOW_TARGET, new FollowTargetState()},
                    { EStatus.ATTACK, new AttackState()},
                    { EStatus.BACK_TO_SPAWN, new BackToSpawnState()},
                });
        }


        //----------------
        // overrides
        //----------------

        protected override void ChangeState<P>(EStatus inStatue, P inParam)
        {
            base.ChangeState(inStatue, inParam);

            _stateMachine.ChangeState(inStatue, this, inParam);
        }


        //-----------------
        // functions
        //-----------------

        public bool CanAttack(in Vec3 inPos)
        {
            return pos.DistanceTo(inPos) < stat.attackRange;
        }

        public bool OutOfSpawnRange()
        {
            return pos.DistanceTo(spawnPos) > 10;
        }

        public void OnDamaged(Entity inEntity)
        {
            var attackParam = new AttackParam()
            {
                target = inEntity
            };

            OnAttack(attackParam);
            OnAttackHandler?.Invoke(attackParam);
        }

        public void NotifyAttack(AttackParam inParam)
        {
            // NOTE@taeho.kang 현재 몬스터가 공격을 하고 있지 않는 상황에만 어그로를 끈다.
            if(Status != EStatus.ATTACK)
                OnAttack(inParam);
        }
    }



    //-----------------------
    // Monster StateMachine
    //-----------------------

    public partial class MonsterEntity
    {
        public class IdleState : IState<MonsterEntity>
        {
            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }

            public void Update<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }
        }

        public class AttackState : IState<MonsterEntity>
        {
            private bool _isRunning = false;

            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                if (inParam is AttackParam attack)
                {
                    var player = attack.target as PlayerEntity;

                    _isRunning = true;

                    UpdateAttack(inSelf, player);
                }
            }


            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                _isRunning = false;
            }


            public void Update<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }


            public void UpdateAttack(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    // 공격이 가능하다면
                    if (inSelf.CanAttack(inOther.pos) == true)
                    {
                        var attackVal = inSelf.stat.GetAttackValue();
                        var damagedVal = inOther.stat.GetDamagedValue(attackVal);

                        inOther.stat.AddCurrHp(-damagedVal);

                        CS_Attack attackPacket = new CS_Attack()
                        {
                            toIsPlayer = true,
                            toId = inOther.targetId,
                            fromIsPlayer = false,
                            fromId = inSelf.targetId,
                            attackValue = damagedVal,
                        };

                        inSelf.broadcaster.Broadcast(attackPacket);

                        JobTimer.Instance.Push(() => { UpdateAttack(inSelf, inOther); }, (int)(inOther.stat.attackSpeed * 1000));
                    }

                    // 공격을 못한다면 다시추격
                    else
                    {
                        inSelf.OnFollowTarget(new FollowTargetParam()
                        {
                            target = inOther,
                        });
                    }
                }
            }
        }

        public class FollowTargetState : IState<MonsterEntity>
        {
            private bool _isRunning = false;
            private const int UPDATE_TIME = 125;

            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                if (inParam is FollowTargetParam follow)
                {
                    _isRunning = true;
                    UpdateFollow(inSelf, follow.target as PlayerEntity);
                }
            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                _isRunning = false;

            }

            public void Update<U>(MonsterEntity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }


            public void UpdateFollow(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    // 공격범위를 벗어났다면..?
                    if(inSelf.OutOfSpawnRange() == true)
                    {
                        inSelf.OnBackToSpawn();
                    }

                    // 공격이 가능하다면?
                    else if (inSelf.CanAttack(inOther.pos) == true)
                    {
                        inSelf.OnAttack(new AttackParam()
                        {
                            target = inOther,
                        });
                    }

                    // 그것도 아니라면..? 이동해라
                    else
                    {
                        // 이동방향을 구한다.
                        var newDir = inOther.pos.SubtractVec3(inSelf.pos).NormalizeVec3();
                      
                        inSelf.OnMoveAndBroadcast(newDir, UPDATE_TIME * 0.001f);
                        JobTimer.Instance.Push(() => { UpdateFollow(inSelf, inOther); }, UPDATE_TIME);
                    }
                }
            }
        }

        public class BackToSpawnState : IState<MonsterEntity>
        {
            private const int UPDATE_TIME = 125;

            private bool    _isRunning;
            private float   _currDist;
            private float   _totalDist;
            private Vec3    _startPos;

            public void Enter<P>(MonsterEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                _isRunning  = true;
                _startPos   = inSelf.pos;
                _currDist = 0;
                _totalDist = _startPos.DistanceTo(inSelf.spawnPos);

                UpdateBackToSpawn(inSelf);
            }

            public void Exit<P>(MonsterEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                _isRunning = false;
            }

            public void Update<P>(MonsterEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {

            }

            public void UpdateBackToSpawn(MonsterEntity inSelf)
            {
                if(this != null && _isRunning == true && inSelf != null)
                {
                    _currDist += UPDATE_TIME * 0.001f * inSelf.stat.moveSpeed;
                    float ratio = _currDist / _totalDist;

                    if(ratio < 1)
                    {
                        inSelf.OnMoveLerpAndBroadcast(_startPos, inSelf.spawnPos, ratio);
                        JobTimer.Instance.Push(() => { UpdateBackToSpawn(inSelf); }, UPDATE_TIME);
                    }
                    else
                    {
                        inSelf.OnIdle();
                    }
                }
            }
        }

        public class DeadState : IState<MonsterEntity>
        {
            public void Enter<P>(MonsterEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                inSelf.OnRespawnCallback?.Invoke();
            }

            public void Exit<P>(MonsterEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {

            }

            public void Update<P>(MonsterEntity inSelf, in P inParam = default) where P : struct, IStateParam
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

        public int GroupId      { get; private set;}
        public int RespawnTime  { get; private set; }

        public MonsterGroup(int inGroupId, int inRespawnTime)
        {
            GroupId     = inGroupId;
            RespawnTime = inRespawnTime;
        }

        public void Add(MonsterEntity inEntity)
        {
            _monstersDict.Add(inEntity.targetId, inEntity);

            inEntity.OnRespawnCallback = OnSendRespawnGroup;
            RegisterAttackHandler(inEntity);
        }

        public void Remove(MonsterEntity inEntity)
        {
            _monstersDict.Remove(inEntity.targetId);

            inEntity.OnRespawnCallback = null;
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
            if(_monstersDict.TryGetValue(inTargetId, out var monster) == true && monster != null)
            {
                return monster;
            }

            HSLogger.GetInstance().Error("MonsterEntity is null or empty!");
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

        private void OnSendRespawnGroup()
        {
            if (IsAllDead() == false)
                return;

            JobTimer.Instance.Push(() =>
            {
                foreach (var entity in _monstersDict.Values)
                {
                    entity.OnIdle();
                    entity.stat.AddCurrHp(entity.stat.maxHp);

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