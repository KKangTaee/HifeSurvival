using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityObjectController<T> : ControllerBase where T : EntityObject
{
   

    protected GameMode _gameMode;

    protected Dictionary<int, T> _entityObjectDict = new Dictionary<int, T>();


    //----------------
    // override
    //----------------

    public override void Init()
    {
        _gameMode = GameMode.Instance;

        _gameMode.OnRecvMoveCB      += OnRecvMove;
        _gameMode.OnRecvStopMoveCB  += OnRecvStopMove;
        _gameMode.OnRecvDeadCB      += OnRecvDead;
        _gameMode.OnRecvAttackCB    += OnRecvAttack;
        _gameMode.OnRecvRespawnCB   += OnRecvRespawn;
    }


    //----------------
    // virtaul
    //----------------

    public virtual void OnRecvMove(Entity inEntity)
    {
        var entityObj = GetEntityObject(inEntity.targetId);

        SetMoveState(entityObj,
                     inEntity.pos.ConvertUnityVector3(),
                     inEntity.dir.ConvertUnityVector3(),
                     inEntity.stat.moveSpeed);
    }

    public virtual void OnRecvStopMove(Entity inEntity)
    {
        var entityObj = GetEntityObject(inEntity.targetId);

        SetIdleState(entityObj,
                     inEntity.pos.ConvertUnityVector3(),
                     inEntity.dir.ConvertUnityVector3(),
                     inEntity.stat.moveSpeed);
    }

    public virtual void OnRecvDead(S_Dead inEntity)
    {
        var entityObj = GetEntityObject(inEntity.toId);

        SetDeadState(entityObj);
    }

    public virtual void OnRecvAttack(CS_Attack inPacket)
    {
        var toEntity    = GetEntityObject(inPacket.toId);
        var fromEntity  = GetEntityObject(inPacket.fromId);

        SetAttackState(toEntity,
                       fromEntity,
                       inPacket.fromPos.ConvertUnityVector3(),
                       inPacket.fromDir.ConvertUnityVector3(),
                       inPacket.attackValue);
    }

    public virtual void OnRecvRespawn(Entity inEntity)
    {
        var entityObj = GetEntityObject(inEntity.targetId);

        entityObj.Init(inEntity.targetId,
                       inEntity.stat,
                       inEntity.pos.ConvertUnityVector3());

    }


    //------------------
    // functions
    //------------------

    public T GetEntityObject(int inTargetId)
    {
        if (_entityObjectDict.TryGetValue(inTargetId, out var entityObj) == true && entityObj != null)
            return entityObj;

        Debug.LogError("player is null or empty");
        return entityObj;
    }


    public void SetMoveState(T inTarget, in Vector3 inPos, in Vector3 inDir, float speed)
    {
        var moveParam = new MoveParam()
        {
            pos = inPos,
            dir = inDir,
            speed = speed,
        };

        inTarget.ChangeState(EntityObject.EStatus.MOVE, moveParam);
    }


    public void SetIdleState(T inTarget, in Vector3 inPos, in Vector3 inDir, float inSpeed)
    {
        var idleParam = new IdleParam()
        {
            pos = inPos,
            dir = inDir,
            speed = inSpeed,
        };

        inTarget.ChangeState(EntityObject.EStatus.IDLE, idleParam);
    }


    public void SetDeadState(T inTarget)
    {
        var deadParam = new DeadParam()
        {

        };

        inTarget.ChangeState(EntityObject.EStatus.DEAD, deadParam);
    }


    public void SetAttackState(EntityObject inTo, EntityObject inFrom, in Vector3 inFromPos, in Vector3 inFromDir, int inDamageVal)
    {
        var attackParam = new AttackParam()
        {
            attackValue = inDamageVal,
            target = inTo,

            fromPos = inFromPos,
            fromDir = inFromDir,
        };

        inFrom.ChangeState(EntityObject.EStatus.ATTACK, attackParam);
    }
}