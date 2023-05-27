using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
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
    }

    [SerializeField] protected MoveMachine _moveMachine;

    public int targetId { get; protected set; }

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
}



public class Player : EntityObject
{

    [SerializeField] private HeroAnimator _anim;

    [SerializeField] private TriggerMachine _wallTrigger;
    [SerializeField] private TriggerMachine _targetTrigger;

    private WorldMap _worldMap;

    private HashSet<EntityObject> _targetSet;

    private IState _state;

    private Dictionary<EStatus, IState> _stateMachine;

    public EStatus Status { get; private set; }
    public bool IsSelf { get; private set; }


    //-----------------
    // unity events
    //-----------------

    private void Awake()
    {
        _moveMachine = GetComponent<MoveMachine>();

        _stateMachine = new Dictionary<EStatus, IState>()
        {
            {EStatus.IDLE, new IdleState()},
            {EStatus.MOVE, new MoveState()},
            {EStatus.FOLLOW_TARGET, new FollowTargetState()}
        };

        _targetSet = new HashSet<EntityObject>();
    }

    private void Start()
    {
        _anim.PlayAnimation(HeroAnimator.AnimKey.KEY_IDLE);
    }


    //-----------------
    // functions
    //-----------------

    public void Init(bool isSelf, int inPlayerId,  in Vector3 inPos)
    {
        if (isSelf == true)
            SetTrigger();

        IsSelf    = isSelf;
        targetId = inPlayerId;

        SetPos(inPos);
    }


    public void SetTrigger()
    {
        _wallTrigger.AddTriggerStay((col) =>
        {
            if (col.CompareTag(TagName.WORLDMAP_WALL) == true)
            {
                if (_worldMap == null)
                    _worldMap = col.gameObject.GetComponentInParent<WorldMap>();

                Vector3 playerPos = transform.position;
                Vector3 hitPoint = col.ClosestPoint(playerPos);

                _worldMap?.UpdateWallMasking(hitPoint, playerPos);
            }
        });

        _wallTrigger.AddTriggerExit((col) =>
        {
            if (col.CompareTag(TagName.WORLDMAP_WALL) == true)
            {
                if (_worldMap == null)
                    _worldMap = col.gameObject.GetComponentInParent<WorldMap>();

                _worldMap?.DoneWallMasking();
            }
        });


        _targetTrigger.AddTriggerEnter((col) =>
        {

        });

        _targetTrigger.AddTriggerExit((col) =>
        {

        });
    }


    public void OnMove(in Vector3 inDir, float inSpeed)
    {
        _anim.OnWalk(inDir);
        _moveMachine.Move(inDir, inSpeed);
    }

    public void OnMoveLerp(in Vector3 inEndPos, float inSpeed, Action doneCallback)
    {
        _anim.OnWalk(_moveMachine.GetDir(inEndPos));
        _moveMachine.MoveLerp(inEndPos, inSpeed, doneCallback);
    }

    public void OnIdle(in Vector3 inPos)
    {
        _anim.OnIdle();
        _moveMachine.MoveStop(inPos);
    }


    public void OnFollowTarget(EntityObject inTarget, Action doneCallback)
    {
        _anim.OnWalk(inTarget.GetDir());
        _moveMachine.MoveFollowTarget(inTarget, null, doneCallback);
    }

    public void OnAttack()
    {

    }

    public bool CanAttack()
    {
        return false;
    }

    public EntityObject GetNearestTarget()
    {
        float minDistance = float.MaxValue;
        EntityObject result = null;

        foreach (var target in _targetSet)
        {
            if (target == null)
                continue;

            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                result = target;
            }
        }

        return result;
    }


    public void ChangeState<P>(EStatus inStatus, in P inParam = default) where P : struct
    {
        // 같은 상태가 다시 돌아왔다는 것은, 파라미터만 넘겨주는 상황일 수 있다.
        if (Status == inStatus)
        {
            _state.Update(this, inParam);
        }
        else
        {
            _state?.Exit();

            Status = inStatus;

            _state = _stateMachine[Status];

            _state?.Enter(this, inParam);
        }
    }
}


public interface IState
{
    void Enter<P>(EntityObject inSelf, in P inParam) where P : struct;

    void Update<P>(EntityObject inSelf, in P inParam) where P : struct;

    void Exit();
}

public class IdleState : IState
{
    public void Enter<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if (inParam is IdleParam idle &&
            inSelf is Player player)
        {
            if(idle.isSelf == false)
            {
                player.OnMoveLerp(idle.pos, idle.speed, ()=>
                {
                    player.OnIdle(idle.pos);
                });
            }
            else
            {
                player.OnIdle(idle.pos);
            }
        }
    }

    public void Update<P>(EntityObject inSelf, in P inParam) where P : struct
    {

    }

    public void Exit()
    {

    }
}

public class MoveState : IState
{
    public void Enter<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if (inParam is MoveParam move && inSelf is Player player)
        {
            if(player.IsSelf == false)
            {
                player.OnMoveLerp(move.pos, move.speed,()=>
                {
                    player.OnMove(move.dir, move.speed);
                });
            }
            else
            {
                player.OnMove(move.dir, move.speed);
            }

        }
    }

    public void Update<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if (inParam is MoveParam move &&
            inSelf is Player player)
        {
            player.OnMove(move.dir, move.speed);
        }
    }

public void Exit()
    {

    }
}

public class FollowTargetState : IState
{
    public void Enter<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if(inParam is FollowTargetParam followTarget &&
           inSelf is Player player)
        {
            player.OnFollowTarget(followTarget.target, followTarget.followDoneCallback);
        }
    }

    public void Exit()
    {

    }

    public void Update<P>(EntityObject inSelf, in P inParam) where P : struct
    {

    }
}

public struct MoveParam
{
    public float speed;
    public Vector3 pos;
    public Vector3 dir;
}

public struct IdleParam
{
    public float speed;
    public Vector3 pos;
    public bool isSelf;
}

public struct AttackParam
{
    public int attackValue;
    public EntityObject target;
}

public struct FollowTargetParam
{
    public EntityObject target;
    public Action followDoneCallback;
}