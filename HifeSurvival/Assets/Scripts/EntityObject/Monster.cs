using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class Monster : EntityObject
{
    [SerializeField] MonsterAnimator _anim;
    [SerializeField] MonsterUI _monsterUI;

    private StateMachine<Monster> _stateMachine;

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

    public override void Init(Entity inEntity, in Vector3 inPos)
    {
        base.Init(inEntity, inPos);

        _monsterUI.Init(inEntity.stat.hp);
        _monsterUI.gameObject.SetActive(true);
    }

    public override void ChangeState<P>(EStatus inStatus, in P inParam = default) where P : struct
    {
        base.ChangeState(inStatus, inParam);

        _stateMachine.ChangeState(inStatus, this, inParam);
    }

    public void OnMoveLerp(in Vector3 inCurrPos, in Vector3 inDestPos, float inSpeed, long inTimeStamp)
    {
        SetPoint(inDestPos, Color.magenta);
        
        var currDir = Vector3.Normalize(inDestPos - GetPos());    
        
        _anim.OnWalk(currDir);
        MoveLerpExpect(inCurrPos, inDestPos, inSpeed, inTimeStamp);
    }

    public void OnMove(in Vector3 dir)
    {
        _anim.OnWalk(dir);
        MoveEntity(dir);
    }

    public void OnAttack(in Vector3 dir)
    {
        _anim.OnAttack(dir);
        StopMoveEntity(GetPos());
    }

    public void OnDead()
    {
        _monsterUI.gameObject.SetActive(false);
        _anim.OnDead();
    }

    public void OnIdle(in Vector3 inPos, in Vector3 inDir = default)
    {
        SetPoint(inPos, Color.red);
        _anim.OnIdle();
        StopMoveEntity(inPos);
    }


    public override void OnDamaged(int inDamageValue)
    {
        _anim.OnDamaged();
        _monsterUI.DecreaseHP(inDamageValue);
    }

    public void SetMonster(int inMosterId)
    {
        _anim.SetTargetAnim(inMosterId);

        _monsterUI.transform.position = _anim.GetPosPivotUI();

        _anim.AddEventDeathCompleted(() =>
        {
            // NOTE@taeho.kang 사망 후, 연출이 끝나고 콜백처리를 여기서 한다.
            // 아마 오브젝트 풀에 넣는 작업으로 이루어질듯
            ControllerManager.Instance.GetController<ObjectPoolController>().StoreToPool(this);
        });
    }
}
