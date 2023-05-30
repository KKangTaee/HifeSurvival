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

        DEAD,
    }

    [SerializeField] protected MoveMachine _moveMachine;

    private IState _state;
    protected Dictionary<EStatus, IState> _stateMachine;

    public int targetId { get; protected set; }
    public EStatus Status { get; protected set; }
    public EntityStat Stat { get; protected set; }


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



public class Player : EntityObject
{
    [SerializeField] private HeroAnimator _anim;
    [SerializeField] private PlayerUI _playerUI;
    [SerializeField] private TriggerMachine _playerTrigger;
    [SerializeField] private TriggerMachine _detectTrigger;

    [SerializeField] private GameObject _detectRange;

    private WorldMap _worldMap;

    private HashSet<EntityObject> _targetSet;

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
            {EStatus.DEAD, new DeadState()},
            {EStatus.ATTACK, new AttackState()},
            {EStatus.FOLLOW_TARGET, new FollowTargetState()},
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

    public void Init(int inPlayerId, EntityStat inStat, in Vector3 inPos)
    {
        targetId = inPlayerId;

        SetPos(inPos);

        _detectTrigger.enabled = false;
        _playerTrigger.enabled = false;

        _playerTrigger.tag = TagName.PLAYER_OTHER;
        _playerTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.PLAYER_OTHER);

        _detectRange?.SetActive(false);

        Stat = inStat;

        _playerUI.Init(Stat.hp);
    }


    public void SetSelf()
    {
        IsSelf = true;

        SetTrigger();

        _playerTrigger.tag = TagName.PLAEYR_SELF;
        _detectTrigger.tag = TagName.DETECT_SELF;

        _playerTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.PLAYER_SELF);
        _detectTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.DETECT_SELF);

        _detectRange?.SetActive(true);
    }


    public void SetTrigger()
    {
        _detectTrigger.enabled = true;
        _playerTrigger.enabled = true;

        _playerTrigger.AddTriggerStay((col) =>
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

        _playerTrigger.AddTriggerExit((col) =>
        {
            if (col.CompareTag(TagName.WORLDMAP_WALL) == true)
            {
                if (_worldMap == null)
                    _worldMap = col.gameObject.GetComponentInParent<WorldMap>();

                _worldMap?.DoneWallMasking();
            }
        });

        _detectTrigger.AddTriggerEnter((col) =>
        {
            if (col.CompareTag(TagName.PLAYER_OTHER) == true)
            {
                var player = col.GetComponentInParent<Player>();

                if (_targetSet.Contains(player) == false)
                    _targetSet.Add(player);
            }
        });

        _detectTrigger.AddTriggerExit((col) =>
        {
            if (col.CompareTag(TagName.PLAYER_OTHER) == true)
            {
                var player = col.GetComponentInParent<Player>();

                if (_targetSet.Contains(player) == true)
                    _targetSet.Remove(player);
            }
        });
    }

    public void OnMove(in Vector3 inDir, float inSpeed)
    {
        _anim.OnWalk(inDir);
        _moveMachine.Move(inDir, inSpeed);
    }

    public void OnMoveLerp(Vector3 inEndPos, float inSpeed, Action doneCallback)
    {
        _moveMachine.MoveLerp(
            inSpeed * 1.5f,
            move =>
            {
                var dir = move.GetDir(inEndPos);
                _anim.OnWalk(dir);
                return dir;
            },
            move => move.IsReaching(inEndPos),
            doneCallback);
    }


    public void OnIdle(in Vector3 inPos, in Vector3 inDir = default)
    {
        _anim.OnIdle();
        _moveMachine.MoveStop(inPos);

        RemovePoint();
    }


    public void OnFollowTarget(EntityObject inTarget, Action doneCallback)
    {
        _moveMachine.MoveLerp(
            4,
            move =>
            {
                var dir = move.GetDir(inTarget.GetPos());
                _anim.OnWalk(dir);

                return dir;
            },
            move => CanAttack(inTarget.GetPos()),
            doneCallback
        );
    }


    public void OnAttack()
    {
        _anim.OnAttack();
        _moveMachine.MoveStop(GetPos());
    }


    public void OnDamaged(int inDamageValue)
    {
        _playerUI.DecreaseHP(inDamageValue);
    }


    public void OnDead()
    {
        OnDamaged(Stat.hp);
        _anim.OnDead();
    }


    public bool CanAttack(in Vector3 inPos)
    {
        return Vector3.Distance(inPos, transform.position) < Stat.attackRange;
    }


    public EntityObject GetNearestTarget()
    {
        float minDistance = float.MaxValue;
        EntityObject result = null;

        foreach (var target in _targetSet)
        {
            if (target == null)
                continue;

            if (target.Status == EntityObject.EStatus.DEAD)
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




    [SerializeField] private GameObject _pointPrefab;
    private List<GameObject> _pointList = new List<GameObject>();


    public void SetPoint(Vector3 inPos, Vector3 inDir)
    {
        var inst = Instantiate(_pointPrefab);
        inst.transform.position = inPos;

        Vector2 direction = new Vector2(inDir.x, inDir.y); // 각도를 계산하려는 벡터
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        inst.transform.localRotation = Quaternion.Euler(0, 0, angle);

        _pointList.Add(inst);
    }

    public void RemovePoint()
    {
        foreach (var point in _pointList)
            Destroy(point);

        _pointList.Clear();
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
            if (idle.isSelf == false)
            {
                player.OnMoveLerp(idle.pos, idle.speed, () =>
                {
                    player.OnIdle(idle.pos, idle.dir);
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
        if (inParam is MoveParam move && inSelf is Player self)
        {
            if (self.IsSelf == false)
            {
                self.SetPoint(move.pos, move.dir);

                self.OnMoveLerp(move.pos, move.speed, () =>
                {
                    self.OnMove(move.dir, move.speed);
                });
            }
            else
            {
                self.OnMove(move.dir, move.speed);
            }

        }
    }

    public void Update<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if (inParam is MoveParam move &&
            inSelf is Player self)
        {
            if (self.IsSelf == false)
            {
                self.SetPoint(move.pos, move.dir);

                self.OnMoveLerp(move.pos, move.speed, () =>
                {
                    self.OnMove(move.dir, move.speed);
                });
            }
            else
            {
                self.OnMove(move.dir, move.speed);
            }
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
        if (inParam is FollowTargetParam followTarget &&
            inSelf is Player self)
        {
            self.OnFollowTarget(followTarget.target, followTarget.followDoneCallback);
        }
    }

    public void Exit()
    {

    }

    public void Update<P>(EntityObject inSelf, in P inParam) where P : struct
    {

    }
}


public class AttackState : IState
{
    public void Enter<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if (inParam is AttackParam attack && inSelf is Player self)
            OnAttack(self, attack);
    }

    public void Exit()
    {

    }

    public void Update<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if (inParam is AttackParam attack && inSelf is Player self)
            OnAttack(self, attack);
    }

    private void OnAttack(Player inPlayer, AttackParam inParam)
    {
        inPlayer.OnAttack();

        if (inParam.target is Player player)
        {
            player.OnDamaged(inParam.attackValue);
        }
    }
}

public class DeadState : IState
{
    public void Enter<P>(EntityObject inSelf, in P inParam) where P : struct
    {
        if (inSelf is Player self && inParam is DeadParam dead)
        {
            // TODO@taeho.kang 나중에 처리.
            self.OnDead();
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
    public Vector3 dir;
    public bool isSelf;
}

public struct AttackParam
{
    public int attackValue;
    public EntityObject target;

    public Action<bool> attackDoneCallback;
}

public struct FollowTargetParam
{
    public EntityObject target;
    public Action<Vector3> followDirCallback;
    public Action followDoneCallback;
}

public struct DeadParam
{

}