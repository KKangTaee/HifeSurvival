using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public partial class MonsterEntity : Entity
    {
        public int monsterId;
        public int groupId;
        public int subId;

        StateMachine<MonsterEntity> _stateMachine;

        public override bool IsPlayer => false;

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

        public void OnBackToSpawn( )
        {

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
                            attackValue = damagedVal,
                            fromId = inSelf.targetId,
                            toIdIsPlayer = false,
                            toId = inOther.targetId
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
                    if (inSelf.CanAttack(inOther.pos) == true)
                    {
                        inSelf.OnAttack(new AttackParam()
                        {
                            target = inOther,
                        });
                    }
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
            private float   _ratio;
            private Vec3    _startPos;

            public void Enter<P>(MonsterEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                _isRunning  = true;
                _startPos   = inSelf.pos;

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

                    JobTimer.Instance.Push(() => { UpdateBackToSpawn(inSelf); }, UPDATE_TIME);
                }
            }
        }
    }
}

