using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : EntityObject
{
    public EntityObject.EStatus _status;

    private IState<T> _state = null;
    private Dictionary<EntityObject.EStatus, IState<T>> _stateMachine;

    public StateMachine(Dictionary<EntityObject.EStatus, IState<T>> inStateMachine)
    {
        _stateMachine = inStateMachine;
    }

    public void ChangeState<P>(EntityObject.EStatus inStatus, T inSelf, in P inParam = default) where P : struct
    {
        // 같은 상태가 다시 돌아왔다는 것은, 파라미터만 넘겨주는 상황일 수 있다.
        if (_status == inStatus)
        {
            _state.Update(inSelf, inParam);
        }
        else
        {
            _state?.Exit();

            _status = inStatus;

            _state = _stateMachine[_status];

            _state?.Enter(inSelf, inParam);
        }
    }
}
