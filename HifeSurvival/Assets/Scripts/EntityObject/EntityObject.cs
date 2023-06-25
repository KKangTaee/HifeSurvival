using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MoveMachine))]
public abstract class EntityObject : MonoBehaviour
{
    public enum EStatus
    {
        IDLE,

        MOVE,

        ATTACK,

        FOLLOW_TARGET,

        USE_SKILL,

        DEAD,
    }

    [SerializeField] private MoveMachine _moveMachine;

    public Entity TargetEntity { get; protected set; }
    public int TargetId        { get; protected set;}
    public EStatus Status      { get; protected set; }
    
    // public EntityStat Stat   { get; protected set; }

    public abstract bool IsPlayer { get; }


    //-------------
    // virtual
    //-------------

    public virtual void Init(Entity inEntity, in Vector3 inPos)
    {
        TargetId = inEntity.targetId;
        TargetEntity = inEntity;
        
        // Stat = inStat;
        SetPos(inPos);
    }


    public virtual void ChangeState<P>(EStatus inStatus, in P inParam = default) where P : struct
    {
        Status = inStatus;
    }

    public virtual void OnDamaged(int inDamageValue)
    {
        // Empty
    }


    //----------------
    // 이동관련
    //----------------

    public void SetPos(in Vector3 inPos)
    {
        transform.position = inPos;
    }

    public Vector3 GetPos()
    {
        return transform.position;
    }

    public Vector3 GetDir()
    {
        return _moveMachine.CurrDir;
    }

    public void MoveEntity(in Vector3 inDir)
    {
        _moveMachine.MoveSelf(inDir, TargetEntity.stat.moveSpeed);
    }

    public void MoveLerpExpect(in Vector3 inCurrPos, in Vector3 inDestPos, float inSpeed, long inTimeStamp, Action doneCallback = null)
    {
        _moveMachine.StartMoveLerpExpect(inCurrPos, inDestPos, inSpeed, inTimeStamp, doneCallback);
    }

    public void MoveLerpTarget(EntityObject inTarget, float inSpeed, Func<bool> inStopFunc, Action<Vector3> dirCallback, Action doneCallback)
    {
        _moveMachine.StartMoveLerpTarget(inTarget, inSpeed, inStopFunc, dirCallback, doneCallback);
    }


    public void StopMoveEntity(in Vector3 inPos)
    {
        _moveMachine.MoveStopSelf(inPos);
    }

}

public interface IState<T> where T : EntityObject
{
    void Enter<P>(T inTarget, in P inParam) where P : struct;

    void Update<P>(T inTarget, in P inParam) where P : struct;

    void Exit();
}