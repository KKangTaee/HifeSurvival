using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{

    public class StateMachine<T> where T : Entity
    {
        protected EntityStatus _status;
        protected IState<T, IStateParam>      _state;

        protected Dictionary<EntityStatus, IState<T, IStateParam>> _stateMachine;

        public StateMachine(Dictionary<EntityStatus, IState<T, IStateParam>> inStateMachine)
        {
            _stateMachine = inStateMachine;
        }

        public void OnChangeState(EntityStatus inStatue, T inEntity, IStateParam inParam)
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
        public Entity killerTarget;
    }

    public struct UseSkillParam : IStateParam
    {

    }
}