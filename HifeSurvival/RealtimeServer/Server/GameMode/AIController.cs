using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public interface IUpdate<T> where T : Entity<T>
    {
        void Update(T inSelf, double deltaTime);
    }

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

        public class AttackState : IState<MonsterEntity>, IUpdate<MonsterEntity>
        {
            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Update(MonsterEntity inSelf, double deltaTime)
            {
                // to do something after
            }
        }

        public class FollowTargetState : IState<MonsterEntity>, IUpdate<MonsterEntity>
        {
            private PlayerEntity player;

            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Update(MonsterEntity inSelf, double deltaTime)
            {
                // to do something after
            }
        }

        public class DamagedState : IState<MonsterEntity>
        {
            public void Enter<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                if (inParam is AttackParam<PlayerEntity> attack)
                {
                    inSelf.stat.hp -= attack.damageValue;

                    // 공격이 가능하다면 공격
                    if (inSelf.CanAttack() == true)
                    {
                        var attackParam = new AttackParam<PlayerEntity>()
                        {
                            damageValue = 100,
                            target = attack.target,
                        };

                        // 공격 (몬스터 -> 플레이어)
                        inSelf.OnAttack(attackParam);
                    }

                    // 공격이 가능하지 않다면..? 추적
                    else
                    {
                        var followTargetParam = new FollowTargetParam<PlayerEntity>()
                        {
                            target = attack.target
                        };

                        inSelf.OnFollowTarget(followTargetParam);
                    }

                    // 1.여기서 브로드 캐스팅 처리?
                    // 브로드 캐스팅을 하려면 GameRoom 객체를 가져와야하는데,
                }
            }

            public void Exit<U>(MonsterEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }
        }

        public class BackToSpawnState : IState<MonsterEntity>
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
    }

    public partial class PlayerEntity
    {
        public class IdleState : IState<PlayerEntity>
        {
            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }
        }

        public class AttackState : IState<PlayerEntity>, IUpdate<PlayerEntity>
        {
            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                if (inParam is AttackParam<PlayerEntity> attack)
                {
                    // 공격처리를 여기서 할꺼임.
                }
            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Update(PlayerEntity inSelf, double deltaTime)
            {
                // to do something after
            }
        }

        public class DamagedState : IState<PlayerEntity>
        {
            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {

            }
        }

        public class MoveState : IState<PlayerEntity>
        {
            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
               // to do something after
            }
        }

        public class UseSkillState : IState<PlayerEntity>
        {
            public void Enter<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
            }

            public void Exit<U>(PlayerEntity inSelf, in U inParam = default) where U : IStateParam
            {
                // to do something after
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

}
