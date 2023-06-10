using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Monster : EntityObject
{

    [SerializeField] MonsterUI _monsterUI;

    public class AttackState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            
        }

        public void Exit()
        {
           
        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
           
        }
    }

    public class DeadState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
           
        }

        public void Exit()
        {
            
        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
         
        }
    }

    public class IdleState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            
        }

        public void Exit()
        {
           
        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
           
        }
    }

    public class MoveState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            if(inParam is MoveParam move)
                inSelf.OnMoveLerp(move.pos, () => inSelf.OnMove(move.dir));
        }

        public void Exit()
        {
         
        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is MoveParam move)
                inSelf.OnMoveLerp(move.pos, () => inSelf.OnMove(move.dir));
        }
    }


    private StateMachine<Monster> _stateMachine;

    public override bool IsPlayer => false;

    private void Awake()
    {
        _stateMachine = new StateMachine<Monster>(
            new Dictionary<EStatus, IState<Monster>>()
            {
                {EStatus.IDLE,   new IdleState()},
                {EStatus.MOVE,   new MoveState()},
                {EStatus.DEAD,   new DeadState()},
                {EStatus.ATTACK, new AttackState()},
            });
    }


    //---------------
    // override
    //---------------

    public override void Init(int inTargetId, EntityStat inStat, in Vector3 inPos)
    {
        base.Init(inTargetId, inStat, inPos);
    }

    public override void ChangeState<P>(EStatus inStatus, in P inParam = default) where P : struct
    {
        base.ChangeState(inStatus, inParam);

        _stateMachine.ChangeState(inStatus, this, inParam);
    }


    public void OnMoveLerp(Vector3 inEndPos, Action doneCallback)
    {
        MoveLerpEntity(() => inEndPos,
                       dir =>
                       { 

                       },
                       null,
                       doneCallback);
    }


    public void OnMove(in Vector3 inDir)
    {
        MoveEntity(inDir);
    }

    public override void OnDamaged(int inDamageValue)
    {
        _monsterUI.DecreaseHP(inDamageValue);
    }
}
