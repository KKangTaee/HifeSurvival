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
    [SerializeField] private GameObject     _detectRange;

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

        this.tag = TagName.PLAYER_OTHER;
        _playerTrigger.gameObject.layer = LayerMask.NameToLayer(LayerName.PLAYER_OTHER);

        _detectRange?.SetActive(false);
        _playerUI.Init(inEntity.stat.hp);

        _anim.OnIdle();
    }


    public void SetSelf(Action<int> inGetItemCallback)
    {
        IsSelf = true;

        SetTrigger();

        this.tag = TagName.PLAEYR_SELF;
        // _playerTrigger.tag = TagName.PLAEYR_SELF;
        // _detectTrigger.tag = TagName.DETECT_SELF;

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
        SetPoint(inDestPos, Color.magenta);

        var dir = inDestPos - inCurrPos;

        _anim.OnWalk(dir);

        MoveLerpExpect(inCurrPos, inDestPos, inSpeed, inTimeStamp);
    }

    public void OnIdle(in Vector3 inPos, in Vector3 inDir = default)
    {
        // SetPoint(inPos, Color.red);

        _anim.OnIdle();

        StopMoveEntity(inPos);

        RemovePoint();
    }


    public void OnFollowTarget(EntityObject inTarget, Action<Vector3> dirCallback, Action doneCallback)
    {
        MoveLerpTarget(inTarget,
                       inTarget.TargetEntity.stat.moveSpeed,
                      ()=> CanAttack(inTarget.GetPos()),
                      (dir) => 
                      {
                        _anim.OnWalk(dir);
                        dirCallback?.Invoke(dir);
                      },
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



}