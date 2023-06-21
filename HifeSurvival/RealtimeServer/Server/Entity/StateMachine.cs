using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{

    public class StateMachine<T> where T : Entity
    {
        protected Entity.EStatus _status;
        protected IState<T, IStateParam>      _state;

        protected Dictionary<Entity.EStatus, IState<T, IStateParam>> _stateMachine;

        public StateMachine(Dictionary<Entity.EStatus, IState<T, IStateParam>> inStateMachine)
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


    public interface IState<T, P> where T : Entity where P : notnull,  IStateParam
    {
        void Enter(T inSelf, in P inParam = default) ;

        void Update(T inSelf, in P inParam = default);

        void Exit(T inSelf, in P inParam = default);
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
        public PVec3 currentPos;
        public long timestamp;
    }


    public struct MoveParam : IStateParam
    {
        public PVec3 currentPos;
        public PVec3 targetPos;
        public float speed;
        public long timestamp;
    }

    public struct DeadParam : IStateParam
    {
        public int    respawnTime;
        public Action respawnCallback;
    }

    public struct UseSkillParam : IStateParam
    {

    }

    public struct BackToSpawnParam :IStateParam
    {

    }
}