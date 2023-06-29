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

        public void OnChangeState(EEntityStatus inStatue, T inEntity, IStateParam inParam)
        {
            if (_status == inStatue)
            {
                _state?.Update(inEntity, inParam);
            }
            else
            {
                _status = inStatue;
                _state?.Exit(inEntity, inParam);
                _state = _stateMachineDict[_status];
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