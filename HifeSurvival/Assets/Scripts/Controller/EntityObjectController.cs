using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        
        var eventHandler = _gameMode.GetEventHandler<IngamePacketEventHandler>();

        eventHandler.RegisterClient<CS_Attack>(OnRecvAttack);
        eventHandler.RegisterClient<S_Dead>(OnRecvDead);
        eventHandler.RegisterClient<UpdateLocationBroadcast>(OnUpdateLocation);
        eventHandler.RegisterClient<S_Respawn>(OnRecvRespawn);
    }


    //----------------
    // virtaul
    //----------------

    public virtual void OnUpdateLocation(UpdateLocationBroadcast inPacket)
    {
        if(ContainEntity(inPacket.id) == false)
            return;

         var entityObj = GetEntityObject(inPacket.id);

        // 값이 동등한 경우. : 정지상태
        if(inPacket.currentPos.NearlyEqual(inPacket.targetPos))
        {
            SetIdleState(entityObj, 
                         inPacket.currentPos.ConvertUnityVector3(),
                         default,
                         inPacket.speed);
        }
        else
        {
          SetMoveState(entityObj,
                      default,
                      inPacket.currentPos.ConvertUnityVector3(),
                      inPacket.targetPos.ConvertUnityVector3(),
                      inPacket.speed,
                      inPacket.timestamp);
        }       
    }


    public virtual void OnRecvStopMove(Entity inEntity)
    {
        if(ContainEntity(inEntity.id) == false)
           return;

        var entityObj = GetEntityObject(inEntity.id);

        SetIdleState(entityObj,
                     inEntity.pos.ConvertUnityVector3(),
                     inEntity.dir.ConvertUnityVector3(),
                     inEntity.stat.moveSpeed);
    }

    public virtual void OnRecvDead(S_Dead inEntity)
    {
        if(ContainEntity(inEntity.id) == false)
           return;

        var entityObj = GetEntityObject(inEntity.id);

        SetDeadState(entityObj);
    }

    public virtual void OnRecvAttack(CS_Attack inPacket)
    {
        if(ContainEntity(inPacket.id) == false)
           return;

        EntityObject fromEntity  = GetEntityObject(inPacket.id);
        EntityObject toEntity    = null;

        switch(Entity.GetEntityType(inPacket.targetId))
        {
            case Entity.EEntityType.PLAYER:
                toEntity = ControllerManager.Instance.GetController<PlayerController>().GetEntityObject(inPacket.targetId);
                break;
            
            case Entity.EEntityType.MOSNTER:
                toEntity = ControllerManager.Instance.GetController<MonsterController>().GetEntityObject(inPacket.targetId);
                break;
        }

        if(toEntity == null)
        {
            Debug.LogError($"[{nameof(OnRecvAttack)}] toEntity is null or empty! : {inPacket.targetId}");
            return;
        }
        
        SetAttackState(toEntity,
                       fromEntity,
                       default,  //inPacket.fromPos.ConvertUnityVector3(),
                       inPacket.attackValue);
    }

    public virtual void OnRecvRespawn(S_Respawn packet)
    {
        if(ContainEntity(packet.id) == false)
           return;
           
        var entityObj = GetEntityObject(packet.id);

        //TODO@taeho.kang 수정필요.
        var entity = _gameMode.GetPlayerEntity(packet.id);
        entityObj.Init(entity, packet.pos.ConvertUnityVector3());
    }

    //------------------
    // functions
    //------------------

    public T GetEntityObject(int inTargetId)
    {
        if (_entityObjectDict.TryGetValue(inTargetId, out var entityObj) == true && entityObj != null)
            return entityObj;

        Debug.LogError($"[{nameof(GetEntityObject)}] entityObject null or empty!");

        return null;
    }

    public bool ContainEntity(int inTargetId)
    {
        return _entityObjectDict.ContainsKey(inTargetId);
    }


    public void SetMoveState(T inTarget, in Vector3 inDir, in Vector3 inCurrPos, in Vector3 inDestPos, float speed, long inTimestamp)
    {
        var moveParam = new MoveParam()
        {
            currPos = inCurrPos,
            destPos = inDestPos,
            timeStamp = inTimestamp,
            dir = inDir,
            speed = speed,
        };

        inTarget.ChangeState(EntityObject.EStatus.MOVE, moveParam);
    }

    public void SetIdleState(T inTarget, in Vector3 inCurrPos, in Vector3 inDir, float inSpeed)
    {
        var idleParam = new IdleParam()
        {
            pos = inCurrPos,
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


    public void SetAttackState(EntityObject inTo, EntityObject inFrom, in Vector3 fromPos, int damageVal)
    {
        var attackParam = new AttackParam()
        {
            attackValue = damageVal,
            target = inTo,
            fromPos = fromPos,
        };

        inFrom.ChangeState(EntityObject.EStatus.ATTACK, attackParam);
    }
}
