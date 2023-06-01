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

    public int TargetId      { get; protected set; }
    public EStatus Status    { get; private set; }
    public EntityStat Stat   { get; protected set; }


    public virtual void Init(int inTargetId, EntityStat inStat, in Vector3 inPos)
    {
        TargetId = inTargetId;
        Stat = inStat;

        SetPos(inPos);
    }


    public virtual void ChangeState<P>(EStatus inStatus, in P inParam = default) where P : struct
    {
        Status = inStatus;
    }



    //----------------
    // �̵�����
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
        _moveMachine.Move(inDir, Stat.moveSpeed);
    }

    public void MoveLerpEntity(Func<Vector3> inEndFunc, Action<Vector3> inDirCallback, Func<bool> inForceStopFunc, Action inDoneCallback)
    {
        _moveMachine.MoveLerpV2(inSpeed: Stat.moveSpeed,
                                inEndPosFunc: inEndFunc,
                                inUpdateRatio: 0.25f,
                                updateCallback: inDirCallback,
                                forceStopFunc: inForceStopFunc,
                                doneCallback: inDoneCallback);
    }

    public void StopMoveEntity(in Vector3 inPos)
    {
        _moveMachine.MoveStop(inPos);
    }
}
