using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{

    public class StateMachine<T> where T : Entity
    {
        protected EEntityStatus _status;
        protected IState<T, IStateParam>      _state;

        protected Dictionary<EEntityStatus, IState<T, IStateParam>> _stateMachineDict;

        public StateMachine(Dictionary<EEntityStatus, IState<T, IStateParam>> inStateMachineDict)
        {
            _stateMachineDict = inStateMachineDict;
        }

        public void OnChangeState(EEntityStatus status, T entity, IStateParam param)
        {
            if (_status == status)
            {
                _state?.Update(entity, param);
            }
            else
            {
                _status = status;
                _state?.Exit(entity, param);
                _state = _stateMachineDict[_status];
                _state?.Enter(entity, param);
            }
        }
    }

    public interface IState<T, P> where T : Entity where P : notnull,  IStateParam
    {
        void Enter(T self, in P param = default) ;

        void Update(T self, in P param = default);

        void Exit(T self, in P param = default);
    }


    public interface IStateParam { }

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

    public struct AttackParam : IStateParam
    {
        public Entity target;
    }

    public struct UseSkillParam : IStateParam
    {

    }

    public struct DeadParam : IStateParam
    {
        public Entity killerTarget;
    }
}