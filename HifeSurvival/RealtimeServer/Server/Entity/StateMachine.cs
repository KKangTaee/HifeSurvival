using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{

    public class StateMachine<T> where T : Entity
    {
        protected Entity.EStatus _status;
        protected IState<T>      _state;

        protected Dictionary<Entity.EStatus, IState<T>> _stateMachine;

        public StateMachine(Dictionary<Entity.EStatus, IState<T>> inStateMachine)
        {
            _stateMachine = inStateMachine;
        }


        public void ChangeState<P>(Entity.EStatus inStatue, T inEntity,  P inParam) where P : struct, IStateParam
        {
            if (_status == inStatue)
            {
                _state?.Update(inEntity, inParam);
            }
            else
            {
                _status = inStatue;
                _state?.Exit(inEntity, inParam);
                _state = _stateMachine[_status];
                _state?.Enter(inEntity, inParam);
            }
        }
    }


    public interface IState<T> where T : Entity
    {
        void Enter<P>(T inSelf, in P inParam = default) where P : struct, IStateParam;

        void Update<P>(T inSelf, in P inParam = default) where P : struct, IStateParam;

        void Exit<P>(T inSelf, in P inParam = default) where P : struct, IStateParam;
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
        public int    respawnTime;
        public Action respawnCallback;
    }

    public struct BackToSpawnParam :IStateParam
    {

    }
}