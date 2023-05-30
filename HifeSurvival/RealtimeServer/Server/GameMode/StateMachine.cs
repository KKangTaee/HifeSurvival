using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public interface IState
    {
        void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam;

        void Update<U>(in U inParam = default) where U : struct, IStateParam;

        void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam;
    }


    //-----------------------
    // Monster StateMachine
    //-----------------------

    public partial class MonsterEntity
    {
        public class IdleState : IState
        {
            public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }

            public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }

            public void Update<U>(in U inParam = default) where U : struct, IStateParam
            {
                throw new NotImplementedException();
            }
        }

        public class AttackState : IState
        {
            private bool _isRunning = false;


            public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                if (inParam is AttackParam attack)
                {
                    var monster = inSelf as MonsterEntity;
                    var player = attack.target as PlayerEntity;

                    _isRunning = true;
                    UpdateAttack(monster, player);
                }
            }


            public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                _isRunning = false;
            }


            public void Update<U>(in U inParam = default) where U : struct, IStateParam
            {

            }


            public void UpdateAttack(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true)
                {
                    // 공격이 가능하다면
                    if (inSelf.CanAttack() == true)
                    {
                        // 0.25초 마다 한번씩 호출

                        var attackVal = inSelf.stat.GetAttackValue();
                        var damagedVal = inOther.stat.GetDamagedValue(attackVal);

                        inOther.stat.AddHp(-damagedVal);

                        CS_Attack attackPacket = new CS_Attack()
                        {
                            damageValue = damagedVal,
                            fromId = inSelf.monsterId,
                            toIdIsPlayer = false,
                            toId = inOther.playerId
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

        public class FollowTargetState : IState
        {
            private bool _isRunning = false;
            private const int UPDATE_TIME = 500;

            public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                if (inParam is FollowTargetParam follow)
                {
                    _isRunning = true;

                    UpdateFollow(inSelf as MonsterEntity, follow.target as PlayerEntity);
                }
            }

            public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                _isRunning = false;

            }

            public void UpdateFollow(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true)
                {
                    if (inSelf.CanAttack() == true)
                    {
                        inSelf.OnAttack(new AttackParam()
                        {
                            attackValue = inSelf.GetAttackValue(),
                            target = inOther,
                        });
                    }
                    else
                    {
                        // 이동방향을 구한다.
                        var dir = inOther.pos.SubtractVec3(inSelf.pos).NormalizeVec3();
                        inSelf.dir = dir;

                        var addSpeed = inSelf.dir.MulitflyVec3(inSelf.stat.moveSpeed * UPDATE_TIME * 0.001f);
                        inSelf.pos = inSelf.pos.AddVec3(addSpeed);

                        CS_Move move = new CS_Move()
                        {
                            pos = inSelf.pos,
                            dir = inSelf.dir,
                            targetId = inSelf.monsterId,
                            isPlayer = false,
                            speed = inSelf.stat.moveSpeed,
                        };

                        inSelf.broadcaster.Broadcast(move);

                        JobTimer.Instance.Push(() => { UpdateFollow(inSelf, inOther); }, UPDATE_TIME);
                    }
                }
            }

            public void Update<U>(in U inParam = default) where U : struct, IStateParam
            {

            }
        }
    }


    //-----------------------
    // Player StateMachine
    //-----------------------
    public partial class PlayerEntity
    {
        public class IdleState : IState
        {
            public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }

            public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }

            public void Update<U>(in U inParam = default) where U : struct, IStateParam
            {

            }
        }

        public class AttackState : IState
        {
            public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                if (inParam is AttackParam attack)
                {
                    // 플레이어 -> 몬스터 공격
                    if(attack.target is MonsterEntity monster)
                    {
                        // 몬스터는 공격을 당했음으로, 플레이어를 공격한다.
                        monster.OnAttack(new AttackParam()
                        {
                            attackValue = monster.GetAttackValue(),
                            target = inSelf
                        });
                    }
                }
            }

            public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {

            }

            public void Update<U>(in U inParam = default) where U : struct, IStateParam
            {

            }
        }

        public class MoveState : IState
        {
            public PlayerEntity _self = null;
            private bool _isRunning = false;

            private const int UPDATE_TIME = 200;

            public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {

                _self = inSelf as PlayerEntity;

                if (inParam is MoveParam move)
                {
                    _self.dir = move.dir;
                    _self.pos = move.pos;

                    _isRunning = true;

                    UpdateMove(_self);
                }
            }

            public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
            {
                _isRunning = false;
            }

            public void Update<U>(in U inParam = default) where U : struct, IStateParam
            {
                // 상태값만 변경한다.
                if (inParam is MoveParam move)
                {
                    _self.dir = move.dir;
                    _self.pos = move.pos;
                }
            }

            public void UpdateMove(PlayerEntity inSelf)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    var addSpeed = inSelf.dir.MulitflyVec3(inSelf.stat.moveSpeed * UPDATE_TIME * 0.0001f);
                    inSelf.pos = inSelf.pos.AddVec3(addSpeed);

                    CS_Move packet = new CS_Move()
                    {
                        dir = inSelf.dir,
                        pos = inSelf.pos,
                        targetId = inSelf.playerId,
                        isPlayer = true,
                        speed = inSelf.stat.moveSpeed,
                    };

                    inSelf.broadcaster.Broadcast(packet);
                    
                    // 0.25초 마다 한번씩 호출
                    JobTimer.Instance.Push(() => { UpdateMove(inSelf); }, UPDATE_TIME);
                }
            }
        }
    }


    public class UseSkillState : IState
    {
        public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
        {

        }

        public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
        {

        }

        public void Update<U>(in U inParam = default) where U : struct, IStateParam
        {
            throw new NotImplementedException();
        }
    }

    public class DeadState : IState
    {
        public void Enter<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
        {

        }

        public void Exit<U>(Entity inSelf, in U inParam = default) where U : struct, IStateParam
        {

        }

        public void Update<U>(in U inParam = default) where U : struct, IStateParam
        {

        }
    }

    public interface IStateParam { }


    public struct AttackParam : IStateParam
    {
        public int attackValue;
        public Entity target;
    }


    public struct FollowTargetParam : IStateParam
    {
        public Entity target;
    }


    public struct IdleParam : IStateParam
    {
        public Vec3 pos;
        public Vec3 dir;
    }


    public struct MoveParam : IStateParam
    {
        public Vec3 pos;
        public Vec3 dir;
        public float speed;
    }

    public struct DeadParam : IStateParam
    {
        
    }
}