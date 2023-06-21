﻿using System;
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

        public Action OnRespawnCallback;

        public string rewardDatas;

        public MonsterEntity()
        {
            var smdic = new Dictionary<EStatus, IState<MonsterEntity, IStateParam>>();
            smdic[EStatus.IDLE] = new IdleState();
            smdic[EStatus.FOLLOW_TARGET] = new FollowTargetState();
            smdic[EStatus.ATTACK] = new AttackState();
            smdic[EStatus.BACK_TO_SPAWN] = new BackToSpawnState();
            smdic[EStatus.DEAD] = new DeadState();

            _stateMachine = new StateMachine<MonsterEntity>(smdic);
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

        public bool CanAttack(in PVec3 inPos)
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
            // NOTE@taeho.kang 죽은상태에서는 어그로 노티파이가 되어선 안됨.
            if(Status == EStatus.DEAD)
               return;

            // NOTE@taeho.kang 현재 몬스터가 공격을 하고 있지 않는 상황에만 어그로를 끈다.
            if (Status != EStatus.ATTACK)
                OnAttack(inParam);
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
            private bool _isRunning = false;

            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is AttackParam attack)
                {
                    var player = attack.target as PlayerEntity;

                    _isRunning = true;

                    updateAttack(inSelf, player);
                }
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                _isRunning = false;
            }


            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                updateAttack(inSelf, (PlayerEntity)((AttackParam)inParam).target);
            }

            private void updateAttack(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    // 상대방이 이미 죽었다면..?
                    if (inOther.Status == EStatus.DEAD)
                    {
                        // 자리로 다시 돌아간다.
                        inSelf.OnBackToSpawn();
                    }
                    // 공격이 가능하다면
                    else if (inSelf.CanAttack(inOther.pos) == true)
                    {
                        var attackVal = inSelf.GetAttackValue();
                        var damagedVal = inOther.GetDamagedValue(attackVal);

                        inOther.stat.AddCurrHp(-damagedVal);

                        // 공격하던 플레이어가 사망했다면..?
                        if (inOther.stat.currHp <= 0)
                        {
                            S_Dead deadPacket = new S_Dead()
                            {
                                toIsPlayer = true,
                                toId = inOther.targetId,
                                fromIsPlayer = false,
                                fromId = inSelf.targetId,
                                respawnTime = 15,
                            };

                            inOther.OnDead();
                            inSelf.broadcaster.Broadcast(deadPacket);

                            // 다시 자리로 돌아간다.
                            inSelf.OnBackToSpawn();
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
                        inSelf.OnFollowTarget(new FollowTargetParam()
                        {
                            target = inOther,
                        });
                    }
                }
            }
        }

        public class FollowTargetState : IState<MonsterEntity, IStateParam>
        {
            private bool _isRunning = false;
            private const int UPDATE_TIME = 125;

            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is FollowTargetParam follow)
                {
                    _isRunning = true;
                    updateFollow(inSelf, follow.target as PlayerEntity);
                }
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
               
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                 _isRunning = false;
            }

            private void updateFollow(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    // 공격범위를 벗어났다면..?
                    if (inSelf.OutOfSpawnRange() == true)
                    {
                        inSelf.OnBackToSpawn();
                    }

                    // 공격이 가능하다면?
                    else if (inSelf.CanAttack(inOther.currentPos) == true)
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
                        var newDir = inOther.currentPos.SubtractPVec3(inSelf.pos).NormalizePVec3();

                        inSelf.OnMoveAndBroadcast(newDir, UPDATE_TIME * 0.001f);
                        JobTimer.Instance.Push(() => { updateFollow(inSelf, inOther); }, UPDATE_TIME);
                    }
                }
            }
        }

        public class BackToSpawnState : IState<MonsterEntity, IStateParam>
        {
            private const int UPDATE_TIME = 125;

            private bool _isRunning;
            private float _currDist;
            private float _totalDist;
            private PVec3 _startPos;

            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                _isRunning = true;
                _startPos = inSelf.pos;
                _currDist = 0;
                _totalDist = _startPos.DistanceTo(inSelf.spawnPos);

                updateBackToSpawn(inSelf);
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                _isRunning = false;
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                
            }

            private void updateBackToSpawn(MonsterEntity inSelf)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    _currDist += UPDATE_TIME * 0.001f * inSelf.stat.moveSpeed;
                    float ratio = _currDist / _totalDist;

                    if (ratio < 1)
                    {
                        inSelf.OnMoveLerpAndBroadcast(_startPos, inSelf.spawnPos, ratio);
                        JobTimer.Instance.Push(() => { updateBackToSpawn(inSelf); }, UPDATE_TIME);
                    }
                    else
                    {
                        inSelf.OnIdle();
                    }
                }
            }
        }

        public class DeadState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                inSelf.OnRespawnCallback?.Invoke();
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

        private void OnSendRespawnGroup()
        {
            if (IsAllDead() == false)
                return;

            JobTimer.Instance.Push(() =>
            {
                foreach (var entity in _monstersDict.Values)
                {
                    entity.OnIdle();
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