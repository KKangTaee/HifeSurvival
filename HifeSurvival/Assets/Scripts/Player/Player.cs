using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;



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


    //------------------
    // state machine
    //------------------

    public class FollowTargetState : IState<Player>
    {
        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is FollowTargetParam followTarget)
            {
                inSelf.OnFollowTarget(followTarget.target,
                                      followTarget.followDirCallback,
                                      followTarget.followDoneCallback);
            }
        }

        public void Exit()
        {

        }

        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {

        }
    }

    public class AttackState : IState<Player>
    {
        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                OnAttack(inSelf, attack);
        }

        public void Exit()
        {

        }

        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                OnAttack(inSelf, attack);
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

    public class DeadState : IState<Player>
    {
        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is DeadParam dead)
            {
                // TODO@taeho.kang 나중에 처리.
                inSelf.OnDead();
            }
        }

        public void Exit()
        {

        }

        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {

        }
    }

    public class IdleState : IState<Player>
    {
        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is IdleParam idleParam )
            {
                if (inSelf.IsSelf == false)
                    inSelf.OnMoveLerp(idleParam.pos, idleParam.dir, () => inSelf.OnIdle(idleParam.pos));

                else
                    inSelf.OnIdle(idleParam.pos);
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
                Move(move, inSelf);
        }


        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is MoveParam move)
                Move(move, inSelf);
        }


        public void Exit()
        {

        }


        public void Move(MoveParam inMoveParam, Player inPlayer)
        {
            if (inPlayer.IsSelf == false)
                inPlayer.OnMoveLerp(inMoveParam.pos, inMoveParam.dir, () => inPlayer.OnMove(inMoveParam.dir));

            else
                inPlayer.OnMove(inMoveParam.dir);
        }
    }


    private StateMachine<Player> _stateMachine;

   
    public override void ChangeState<P>(EStatus inStatus, in P inParam = default) where P : struct
    {
        base.ChangeState(inStatus, inParam);

        _stateMachine.ChangeState(inStatus, this, inParam);
    }


    //-----------------
    // unity events
    //-----------------

    private void Awake()
    {
        _stateMachine = new StateMachine<Player>(
            new Dictionary<EStatus, IState<Player>>()
            {
                {EStatus.IDLE, new IdleState()},
                {EStatus.MOVE, new MoveState()},
                {EStatus.DEAD, new DeadState()},
                {EStatus.ATTACK, new AttackState()},
                {EStatus.FOLLOW_TARGET, new FollowTargetState()},
            });

        _targetSet = new HashSet<EntityObject>();
    }


    private void Start()
    {
        _anim.PlayAnimation(HeroAnimator.AnimKey.KEY_IDLE);
    }


    //-----------------
    // functions
    //-----------------

    public override void Init(int inTargetId, EntityStat inStat, in Vector3 inPos)
    {
        base.Init(inTargetId, inStat, inPos);


        _detectTrigger.enabled = false;
        _playerTrigger.enabled = false;

        _playerTrigger.tag = TagName.PLAYER_OTHER;
        _playerTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.PLAYER_OTHER);

        _detectRange?.SetActive(false);
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




    //-------------------
    // Player State
    //-------------------

    public void OnMove(in Vector3 inDir)
    {
        _anim.OnWalk(inDir);

        MoveEntity(inDir);
    }


    public void OnMoveLerp(Vector3 inEndPos, Vector3 inDir, Action doneCallback)
    {
        SetPoint(inEndPos, Vector3.zero);

        _anim.OnWalk(inDir);
        
        MoveLerpEntity(() => inEndPos, 
                       dir => {  }, 
                       null, 
                       doneCallback);
    }


    public void OnIdle(in Vector3 inPos, in Vector3 inDir = default)
    {
        _anim.OnIdle();

        StopMoveEntity(inPos);

        RemovePoint();
    }


    public void OnFollowTarget(EntityObject inTarget, Action<Vector3> dirCallback, Action doneCallback)
    {
        MoveLerpEntity(() => inTarget.GetPos(),
                       dir =>
                       {
                           _anim.OnWalk(dir);
                           dirCallback?.Invoke(dir);
                       },
                       () => CanAttack(inTarget.GetPos())
                       , doneCallback);
    }


    public void OnAttack()
    {
        _anim.OnAttack();
        StopMoveEntity(GetPos());
    }


    public void OnDamaged(int inDamageValue)
    {
        _playerUI.DecreaseHP(inDamageValue);
    }


    public void OnDead()
    {
        OnDamaged(10000);
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


public interface IState<T> where T : EntityObject
{
    void Enter<P>(T inSelf, in P inParam) where P : struct;

    void Update<P>(T inSelf, in P inParam) where P : struct;

    void Exit();
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