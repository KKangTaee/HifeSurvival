using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public interface IState<T> where T : Entity<T>
    {
        void Enter<U>(T inSelf, in U inParam = default) where U : IStateParam;

        void Exit<U>(T inSelf, in U inParam = default) where U : IStateParam;
    }


    //-----------------------
    // Monster StateMachine
    //-----------------------

    public partial class MonsterEntity
    {
        public class IdleState : IState<MonsterEntity>
        {
            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }
        }

        public class AttackState : IState<MonsterEntity>
        {
            private bool _isRunning = false;

            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                if(inParam is AttackParam<PlayerEntity> attackPlayer)
                {
                    AttackAndBroadcast(inSelf, attackPlayer.target);

                    _isRunning = true;
                    Update(inSelf, attackPlayer.target);
                }
            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                _isRunning = false;
            }

            public void Update(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true)
                {
                    if(inSelf.CanAttack() == true)
                    {
                        AttackAndBroadcast(inSelf, inOther);
                        JobTimer.Instance.Push(() => { Update(inSelf, inOther); }, 500);
                    }
                    else
                    {
                        inSelf.OnFollowTarget(new FollowTargetParam<PlayerEntity>()
                        {
                            target = inOther,
                        });
                    }
                }
            }

            public void AttackAndBroadcast(MonsterEntity inSelf, PlayerEntity inOther)
            {
                // 0.25초 마다 한번씩 호출
                inOther.stat.hp -= inSelf.Attack();

                CS_Attack attackPacket = new CS_Attack()
                {
                    damageValue = inSelf.Attack(),
                    fromId = inSelf.monsterId,
                    toIdIsPlayer = false,
                    toId = inOther.playerId
                };

                inSelf.broadcaster.Broadcast(attackPacket);
            }
        }

        public class FollowTargetState : IState<MonsterEntity>
        {
            private bool _isRunning = false;

            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                if(inParam is FollowTargetParam<PlayerEntity> follow)
                {
                    // 몬스터 -> 사람 추격한다.
                    _isRunning = true;
                    Update(inSelf, follow.target);
                }
            }


            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                _isRunning = false;
            }


            public void Update(MonsterEntity inSelf, PlayerEntity inOther)
            {
                if (this != null && _isRunning == true)
                {
                    if (inSelf.CanAttack() == true)
                    {
                        inSelf.OnAttack(new AttackParam<PlayerEntity>()
                        {
                            damageValue = inSelf.Attack(),
                            target = inOther,
                        });
                    }
                    else
                    {
                        // 이동방향을 구한다.
                        var dir = inOther.pos.SubtractVec3(inSelf.pos).NormalizeVec3();
                        inSelf.dir = dir;
            
                        var addSpeed = inSelf.dir.MulitflyVec3(inSelf.speed * 0.25f);
                        inSelf.pos.AddVec3(addSpeed);

                        CS_Move move = new CS_Move()
                        {
                            pos = inSelf.pos,
                            dir = inSelf.dir,
                            targetId = inSelf.monsterId,
                            isPlayer = false,
                            speed = inSelf.speed,
                        };

                        inSelf.broadcaster.Broadcast(move);

                        JobTimer.Instance.Push(() => { Update(inSelf, inOther); }, 250);
                    }
                }
            }
        }

        public class BackToSpawnState : IState<MonsterEntity>
        {
            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {

            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {

            }
        }
    }

    public partial class PlayerEntity
    {
        public class IdleState : IState<PlayerEntity>
        {
            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {

            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {

            }
        }

        public class AttackState : IState<PlayerEntity>
        {
            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // 플레이어 공격시
                if (inParam is AttackParam<PlayerEntity> attackPlayer)
                {
                    attackPlayer.target.stat.hp -= attackPlayer.damageValue;
                }
                // 몬스터 공격시
                else if(inParam is AttackParam<MonsterEntity> attackMonster)
                {
                    attackMonster.target.stat.hp -= attackMonster.damageValue;

                    if (attackMonster.target.CanAttack())
                    {
                        var p1 = new AttackParam<PlayerEntity>()
                        {
                            damageValue = 100,
                            target = inSelf,
                        };

                        attackMonster.target.OnAttack(p1);
                    }
                    else
                    {
                        var p2 = new FollowTargetParam<PlayerEntity>()
                        {
                            target = inSelf
                        };

                        attackMonster.target.OnFollowTarget(p2);
                    }
                }
            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {

            }
        }

        public class MoveState : IState<PlayerEntity>
        {
            private bool _isRunningUpdate = false;

            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                if (inParam is MoveParam move)
                {
                    inSelf.dir = move.dir;
                    inSelf.pos = move.pos;
                    inSelf.speed = move.speed;

                    _isRunningUpdate = true;         
                    Update(inSelf);
                }
            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                _isRunningUpdate = false;
            }


            public void Update(PlayerEntity inSelf)
            {
                if (this != null && _isRunningUpdate == true)
                {
                    var addSpeed = inSelf.dir.MulitflyVec3(inSelf.speed * 0.25f);
                    inSelf.pos.AddVec3(addSpeed);

                    CS_Move packet = new CS_Move()
                    {
                        dir = inSelf.dir,
                        pos = inSelf.pos,
                        targetId = inSelf.playerId,
                        isPlayer = true,
                        speed = inSelf.speed,
                    };

                    inSelf.broadcaster.Broadcast(packet);
                }

                // 0.25초 마다 한번씩 호출
                JobTimer.Instance.Push(() => { Update(inSelf); }, 250);
            }
        }
    }

    public class UseSkillState : IState<PlayerEntity>
    {
        public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
        {

        }

        public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
        {

        }
    }
}

public interface IStateParam { }


public struct AttackParam<T> : IStateParam
{
    public int damageValue;
    public T target;
}


public struct FollowTargetParam<T> : IStateParam
{
    public T target;
}


public struct DamagedParam<T> : IStateParam
{
    public int damageValue;
    public T target;
}


public struct IdleParam : IStateParam
{

}

public struct MoveParam : IStateParam
{
    public Vec3 pos;
    public Vec3 dir;
    public float speed;
    public Action<IPacket> boardcastCB;
}
