using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ServerObject
{

}

public class ServerMonster : ServerObject
{
    public enum EStatus
    {
        IDLE,

        FOLLOW_TARGET,

        ATTACK,

        DAMAGED,

        BACK_TO_SPAWN,
    }

    public int monsterId;
    public int monsterType;
    public int groupId;
    public int subId;

    public Vec3 position;
    public Vec3 dir;
    public Stat stat;

    private AIEngine.IState<ServerMonster> _state;
    private Dictionary<EStatus, AIEngine.IState<ServerMonster>> _stateMachine;
    private EStatus _status;

    public void ChangeState(AIEngine.IState<ServerMonster> newState, ServerObject other)
    {
        _state?.Exit(this, other);
        _state = newState;
        _state?.Enter(this, other);
    }

    public void ChangeState(EStatus inStatus, ServerObject other)
    {
        _status = inStatus;
        ChangeState(_stateMachine[_status], other);
    }

    public void Update(double deltaTime)
    {
        if(_state is AIEngine.IUpdate<ServerMonster> update)
            update.Update(this, deltaTime);
    }

    public void OnAttack(ServerObject other)
    {
        ChangeState(EStatus.ATTACK, other);
    }

    public void OnFollowTarget(ServerObject other)
    {
        ChangeState(EStatus.FOLLOW_TARGET, other);
    }

    public void OnIdle(ServerObject other)
    {
        ChangeState(EStatus.IDLE, other);
    }
}

public class ServerPlayer : ServerObject
{
    public string userId;
    public int playerId;
    public int heroType;
    public bool isReady;
}


public struct Stat
{
    public readonly int maxHP;
    public readonly int maxEXP;
    public readonly int maxSTR;

    public int hp;
    public int exp;
    public int str;

    public Stat(int inMaxHP, int inMaxEXP, int inMaxSTR)
    {
        hp = maxHP = inMaxHP;
        exp = maxEXP = inMaxEXP;
        str = maxSTR = inMaxSTR;
    }
}