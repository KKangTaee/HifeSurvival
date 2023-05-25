using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(MoveMachine))]
public class Player : MonoBehaviour
{
    public enum EStatus
    {
        IDLE,

        MOVE,

        ATTACK,

        USE_SKILL,
    }

    [SerializeField] private HeroAnimator _anim;

    [SerializeField] private MoveMachine _moveMachine;
    [SerializeField] private TriggerMachine _triggerMachine;

    private WorldMap _worldMap;

    private IState<Player> _state;
    private Dictionary<EStatus, IState<Player>> _stateMachine;


    public EStatus Status { get; private set; }
    public bool IsSelf    { get; private set; }


    //-----------------
    // unity events
    //-----------------

    private void Awake()
    {
        _moveMachine = GetComponent<MoveMachine>();
        _stateMachine = new Dictionary<EStatus, IState<Player>>()
        {
            {EStatus.IDLE, new IdleState()},
            {EStatus.MOVE, new MoveState()}
        };
    }

    private void Start()
    {
        _anim.PlayAnimation(HeroAnimator.AnimKey.KEY_IDLE);
    }


    //-----------------
    // functions
    //-----------------

    public void Init(bool isSelf, in Vector3 inPos)
    {
        if (isSelf == true)
            SetTrigger();

        IsSelf = isSelf;

        SetPos(inPos);
    }


    public void SetPos(in Vector3 inPos)
    {
        transform.position = inPos;
    }

    public Vector3 GetPos()
    {
        return transform.position;
    }


    public void SetTrigger()
    {
        _triggerMachine.AddTriggerStay((sender, col) =>
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

        _triggerMachine.AddTriggerExit((sender, col) =>
        {
            if (col.CompareTag(TagName.WORLDMAP_WALL) == true)
            {
                if (_worldMap == null)
                    _worldMap = col.gameObject.GetComponentInParent<WorldMap>();

                _worldMap?.DoneWallMasking();
            }
        });
    }

    public Vector3 GetDir()
    {
        return _moveMachine.CurrDir;
    }


    public void OnMove(in Vector3 inDir, float inSpeed)
    {
        _anim.OnWalk(inDir);
        _moveMachine.Move(inDir, inSpeed);
    }

    public void OnIdle(in Vector3 inPos, bool isNeedLerp = false)
    {
        _anim.OnIdle();
        _moveMachine.StopMove(inPos, isNeedLerp);
    }

    public void ChangeState<P>(EStatus inStatus, in P inParam = default) where P : struct
    {
        // 같은 상태가 다시 돌아왔다는 것은, 파라미터만 넘겨주는 상황일 수 있다.
        _state?.Exit();

        Status = inStatus;

        _state = _stateMachine[Status];

        _state?.Enter(this, inParam);
    }


    public void UpdateState<P>(in P inParam = default) where P : struct
    {
        _state.Update(this, inParam);
    }
}


public interface IState<T>
{
    void Enter<P>(T inSelf, in P inParam) where P : struct;

    void Update<P>(T inSelf, in P inParam) where P : struct;
    
    void Exit();
}

public class IdleState : IState<Player>
{
    public void Enter<P>(Player inSelf, in P inParam) where P : struct
    {
        if (inParam is IdleParam idle)
        {
            inSelf.OnIdle(idle.pos, idle.isSelf == false);
        }
    }

    public void Update<P>(Player inSelf, in P inParam) where P : struct
    {

    }

    public void Exit()
    {

    }
}

public class MoveState : IState<Player>
{
    public void Enter<P>(Player inSelf, in P inParam) where P : struct
    {
        if (inParam is MoveParam move)
            inSelf.OnMove(move.dir, move.speed);
    }

    public void Update<P>(Player inSelf, in P inParam) where P : struct
    {
        if (inParam is MoveParam move)
            inSelf.OnMove(move.dir, move.speed);
    }

    public void Exit()
    {

    }
}

public class FollowTargetState : IState<Player>
{
    public void Enter<P>(Player inSelf, in P inParam) where P : struct
    {
        
    }

    public void Exit()
    {
        
    }

    public void Update<P>(Player inSelf, in P inParam) where P : struct
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
    public Vector3 pos;
    public bool isSelf;
}

public struct AttackParam
{
    public float damageValue;
}