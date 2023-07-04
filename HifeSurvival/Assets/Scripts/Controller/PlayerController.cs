using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

public sealed class PlayerController : EntityObjectController<Player>
{
    [SerializeField] private Player _playerPrefab;

    private CameraController _cameraController;
    private JoystickController _joystickController;
    private IDisposable _attackDelay;

    public Player Self { get; private set; }

    private DateTime _nextSendTime;

    //-----------------
    // override
    //-----------------

    public override void Init()
    {
        base.Init();

        _cameraController = ControllerManager.Instance.GetController<CameraController>();
        _joystickController = ControllerManager.Instance.GetController<JoystickController>();

        _gameMode.OnRecvUpdateStatHandler += OnRecvUpdateStat;
        _gameMode.OnRecvPickRewardHandler += OnRecvPickReward;

        LoadPlayer();
    }


    //-----------------
    // functions
    //-----------------

    /// <summary>
    /// 플레이어 오브젝트 로드
    /// </summary>
    /// <param name="inWorldMap"></param>
    public void LoadPlayer()
    {
        var entitys = GameMode.Instance.PlayerEntitysDict.Values;

        foreach (var entity in entitys)
        {
            var inst = Instantiate(_playerPrefab, transform);

            inst.Init(entity, entity.pos.ConvertUnityVector3());

            if (ServerData.Instance.UserData.user_id == entity.userId)
            {
                Self = inst;
                inst.SetSelf(OnGetItemSelf);
            }

            _entityObjectDict.Add(entity.id, inst);
        }

        _cameraController.SetCameraPos(Self.GetPos());
        _cameraController.FollowingTarget(Self.transform);

        Self.OnRespawn();
    }


    /// <summary>
    /// 플레이어 오브젝트 가져오기
    /// </summary>
    /// <param name="inPlayerId"></param>
    /// <returns></returns>
    public void OnMoveSelf(in Vector3 inDir)
    {
        OnStopAttackSelf();

        SendMove(inDir);

        SetMoveState(Self,
                    inDir,
                     Self.GetPos(),
                     default,
                     Self.TargetEntity.stat.moveSpeed,
                     0);
    }

    public void SendMove(in Vector3 inDir)
    {
        if (_nextSendTime < DateTime.Now)
        {
            float deltaTime = 0.25f;

            Vector3 destPos = Self.GetPos() + inDir * deltaTime * Self.TargetEntity.stat.moveSpeed;
            _gameMode.OnSendMoveRequest(Self.GetPos(), destPos);
            _nextSendTime = DateTime.Now.AddMilliseconds(deltaTime*1000);
        }
    }

    public void OnStopMoveSelf()
    {
        if (Self.Status == EntityObject.EStatus.DEAD)
            return;

        _gameMode.OnSendMoveRequest(Self.GetPos(), Self.GetPos());

        SetIdleState(Self,
                     Self.GetPos(),
                     Self.GetDir(),
                     Self.TargetEntity.stat.moveSpeed);

        // 타겟이 있는지 감지
        DetectTargetSelf();
    }


    public void DetectTargetSelf()
    {
        var target = Self.GetNearestTarget();

        if (target == null)
            return;

        if (Self.CanAttack(target.GetPos()) == true)
        {
            OnAttackSelf(target);
        }
        else
        {
            OnFollowTargetSelf(target);
        }
    }


    public void OnFollowTargetSelf(EntityObject inTarget)
    {
        var followTargetParam = new FollowTargetParam()
        {
            target = inTarget,

            followDirCallback = (dir) =>
            {
                SendMove(dir);
            },

            followDoneCallback = () =>
            {
                OnAttackSelf(inTarget);
            }
        };

        Self.ChangeState(EntityObject.EStatus.FOLLOW_TARGET, followTargetParam);
    }


    public void OnAttackSelf(EntityObject inTarget)
    {
        // 상대방의 방어력도 고려한다.
        var selfAttactValue = Self.TargetEntity.GetAttackValue();
        var damagedVal = inTarget.TargetEntity.GetDamagedValue(selfAttactValue);

        SetAttackState(inTarget, Self, Self.GetPos(), damagedVal);
        
        _gameMode.OnSendAttack(inTarget.id, damagedVal);

        _attackDelay = Observable.Timer(TimeSpan.FromSeconds(Self.TargetEntity.stat.attackSpeed))
                                        .Subscribe(_ =>
        {
            // 이미 내가 죽은 상태라면..?
            if (Self.Status == EntityObject.EStatus.DEAD)
            {
                OnStopAttackSelf();
                return;
            }

            // 이미 상대가 죽었다면
            if (inTarget.Status == EntityObject.EStatus.DEAD)
            {
                // 다른 상대방을 감지하고 찾아라.
                DetectTargetSelf();
            }
            // 공격이 가능하다면 다시 공격.
            else if (Self.CanAttack(inTarget.GetPos()) == true)
            {
                OnAttackSelf(inTarget);
            }
            // 아니라면, 다시 추격해라.
            else
            {
                OnFollowTargetSelf(inTarget);
            }
        });
    }


    public void OnStopAttackSelf()
    {
        _attackDelay?.Dispose();
    }

    public void OnGetItemSelf(int inWorldId)
    {
        _gameMode.OnSendPickReward(inWorldId);
    }


    //----------------
    // server
    //----------------

    public override void OnRecvDead(S_Dead inEntity)
    {
        base.OnRecvDead(inEntity);

        if (inEntity.id == Self.id)
        {
            // _joystickController.stop
            _joystickController.HideJoystick();
        }
    }


    public override void OnRecvRespawn(Entity inEntity)
    {
        base.OnRecvRespawn(inEntity);

        if (Self.id == inEntity.id)
        {
            Self.SetSelf(OnGetItemSelf);

            _joystickController.ShowJoystick();
            _cameraController.FollowingTarget(Self.transform);
        }
    }

    public override void OnUpdateLocation(UpdateLocationBroadcast inPacket)
    {
        // 타겟 그 자신이라면 리턴
        if(Self.id == inPacket.id)
           return;

        base.OnUpdateLocation(inPacket);
    }


    public void OnRecvUpdateStat(PlayerEntity inEntity)
    {
        var player = GetEntityObject(inEntity.id);
        player.UpdateHp();
    }


    public void OnRecvPickReward(PickRewardResponse packet)
    {
        if((ERewardType)packet.rewardType != ERewardType.ITEM)
            return;
            
        var player = GetEntityObject(packet.id);
        var entity = GameMode.Instance.GetPlayerEntity(packet.id);
        player.UpdateItemView(entity.itemSlot[packet.itemSlotId]);
    }
}
