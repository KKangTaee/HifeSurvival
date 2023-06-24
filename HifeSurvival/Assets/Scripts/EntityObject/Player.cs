using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;



public partial class Player : EntityObject
{
    [SerializeField] private HeroAnimator _anim;
    [SerializeField] private PlayerUI _playerUI;
    [SerializeField] private TriggerMachine _playerTrigger;
    [SerializeField] private TriggerMachine _detectTrigger;

    [SerializeField] private GameObject _detectRange;

    public override bool IsPlayer => true;

    private HashSet<EntityObject> _targetSet;

    private EntityItem[] _itemSlot;

    private Action<int> _getItemCallback;

    private WorldMap _worldMap;

    private StateMachine<Player> _stateMachine;
    
    public bool IsSelf { get; private set; }

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

        _targetSet   = new HashSet<EntityObject>();
    }


    //-----------------
    // functions
    //-----------------

    public override void Init(Entity inEntity, in Vector3 inPos)
    {
        base.Init(inEntity, inPos);

        _detectTrigger.enabled = false;
        _playerTrigger.enabled = false;

        _playerTrigger.tag = TagName.PLAYER_OTHER;
        _playerTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.PLAYER_OTHER);

        _detectRange?.SetActive(false);
        _playerUI.Init(inEntity.stat.hp);

        _anim.OnIdle();
    }


    public void SetSelf(Action<int> inGetItemCallback)
    {
        IsSelf = true;

        SetTrigger();

        _playerTrigger.tag = TagName.PLAEYR_SELF;
        _detectTrigger.tag = TagName.DETECT_SELF;

        _playerTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.PLAYER_SELF);
        _detectTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.DETECT_SELF);

        _detectRange?.SetActive(true);
        _getItemCallback = inGetItemCallback;
    }


    public void SetTrigger()
    {
        _detectTrigger.enabled = true;
        _playerTrigger.enabled = true;

        _playerTrigger.AddTriggerEnter((col) =>
        {
            // 드랍된 아이템과 접촉했다면..?
            if(col.CompareTag(TagName.DROP_ITEM) == true)
            {
                var dropItem = col.GetComponent<WorldItem>();

                if (dropItem == null)
                    return;

                // 여기에 브로드캐스팅 처리
                _getItemCallback?.Invoke(dropItem.WorldId);
                // dropItem.PlayGetItem();
            }
        });

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
            if (col.CompareTag(TagName.PLAYER_OTHER) == true || col.CompareTag(TagName.MONSTER) == true)
            {
                var entityObj = col.GetComponent<EntityObject>();

                if (entityObj == null)
                {
                    Debug.LogWarning($"[{nameof(SetTrigger)}] col object is null or empty!");
                    return;
                }

                if (_targetSet.Contains(entityObj) == false)
                    _targetSet.Add(entityObj);
            }
        });

        _detectTrigger.AddTriggerExit((col) =>
        {
            if (col.CompareTag(TagName.PLAYER_OTHER) == true || col.CompareTag(TagName.MONSTER) == true)
            {
                var entityObj = col.GetComponent<EntityObject>();

                if (entityObj == null)
                {
                    Debug.LogWarning($"[{nameof(SetTrigger)}] col object is null or empty!");
                    return;
                }
                
                if (_targetSet.Contains(entityObj) == true)
                    _targetSet.Remove(entityObj);
            }
        });
    }


    public void SetItemSlot(EntityItem [] inItemSlot)
    {
        _itemSlot = inItemSlot;
    }


    //-------------------
    // Player State
    //-------------------

    public override void OnDamaged(int inDamageValue)
    {
        _anim.OnDamaged();
        _playerUI.DecreaseHP(inDamageValue);

        if (IsSelf == true)
            ActionDisplayUI.Show(ActionDisplayUI.ESpawnType.TAKE_DAMAGE, inDamageValue, GetPos() + Vector3.up);
    }


    public void OnMove(in Vector3 inDir)
    {
        _anim.OnWalk(inDir);

        MoveEntity(inDir);
    }


    
    public void OnMoveLerp(in Vector3 inCurrPos, in Vector3 inDestPos, float inSpeed, long inTimeStamp)
    {
        SetPoint(inDestPos, Vector3.zero);

        var dir = inDestPos - inCurrPos;

        _anim.OnWalk(dir);

        MoveLerpExpect(inCurrPos, inDestPos, inSpeed, inTimeStamp);
    }

    public void OnIdle(in Vector3 inPos, in Vector3 inDir = default)
    {
        _anim.OnIdle();

        StopMoveEntity(inPos);

        RemovePoint();
    }


    public void OnFollowTarget(EntityObject inTarget, Action<Vector3> dirCallback, Action doneCallback)
    {
        MoveLerpTarget(inTarget,
                       inTarget.TargetEntity.stat.moveSpeed,
                      ()=> CanAttack(inTarget.GetPos()),
                      doneCallback);
    }


    public void OnAttack(in Vector3 inDir)
    {
        _anim.OnAttack(inDir);

        StopMoveEntity(GetPos());
    }


    public void UpdateHp()
    {
        _playerUI.SetMaxHP(TargetEntity.stat.hp);
        _playerUI.SetHP(TargetEntity.stat.currHP);
    }


    public void UpdateItemSlot()
    {
        if (_itemSlot == null)
            return;

        foreach(var item in _itemSlot)
        {
            // TODO@taeho.kang 업데이트 처리
        }
    }


    public void OnDead()
    {
        _playerUI.SetHP(0);

        _anim.OnDead();

        _targetSet?.Clear();
    }


    public bool CanAttack(in Vector3 inPos)
    {
        return Vector3.Distance(inPos, transform.position) < TargetEntity.stat.attackRange;
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

    public void OnRespawn()
    {
        _anim.OnRespawn();
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


//------------------
// state machine
//------------------

public partial class Player
{
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

        private void OnAttack(Player inFrom, AttackParam inParam)
        {
            EntityObject inTo = inParam.target;

            // 내 클라의 전송값과 서버의 위치값이 크게 다르다면..?
            Attack(inParam, inFrom, inTo);
        }

        #region  Local Func
        void Attack(AttackParam inParam, Player inFrom, EntityObject inTo)
        {
            inFrom.OnAttack(inParam.fromDir);
            inTo.OnDamaged(inParam.attackValue);

            if (inFrom.IsSelf == true)
                ActionDisplayUI.Show(ActionDisplayUI.ESpawnType.ATTACK, inParam.attackValue, inTo.GetPos() + Vector3.up);
        }
        #endregion
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
        const float DISTANCE_DIFF = 2f;

        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is IdleParam idleParam)
            {
                 // 현재 서버좌표의 절대위치와 클라 동기화 위치의 간격이 꽤나 차이가 심하다면, 보간 후 정지 시킨다.
                if(inSelf.IsSelf == false && Vector3.Distance(inSelf.GetPos(), idleParam.pos) > DISTANCE_DIFF)
                {
                    inSelf.MoveLerpExpect(inSelf.GetPos(), 
                                        idleParam.pos, 
                                        idleParam.speed, 
                                        0,
                                        ()=>
                                        {
                                            inSelf.OnIdle(idleParam.pos);
                                        });
                }
                else
                {
                    inSelf.OnIdle(idleParam.pos);
                }
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
        public void Enter<P>(Player inTarget, in P inParam) where P : struct
        {
            if(inParam is MoveParam move)
                Move(move, inTarget);
        }


        public void Update<P>(Player inTarget, in P inParam) where P : struct
        {
            if(inParam is MoveParam move)
                Move(move, inTarget);
        }


        public void Exit()
        {

        }

        public void Move(MoveParam inMoveParam, Player inPlayer)
        {
            if (inPlayer.IsSelf == false)
                inPlayer.OnMoveLerp(inMoveParam.currPos,
                                    inMoveParam.destPos,
                                    inMoveParam.speed,
                                    inMoveParam.timeStamp);

            else
                inPlayer.OnMove(inMoveParam.dir);
        }
    }
}