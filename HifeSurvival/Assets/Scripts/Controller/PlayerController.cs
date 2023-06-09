using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

public sealed class PlayerController : EntityObjectController<Player>
{
    [SerializeField] private Player _playerPrefab;

    private CameraController    _cameraController;
    private JoystickController  _joystickController;
    private TouchController     _touchController;

    private IDisposable _attackDelay;

    public Player Self { get; private set; }


    //-----------------
    // override
    //-----------------

    public override void Init()
    {
        base.Init();

        _cameraController   = ControllerManager.Instance.GetController<CameraController>();
        _joystickController = ControllerManager.Instance.GetController<JoystickController>();
        _touchController    = ControllerManager.Instance.GetController<TouchController>();

        _gameMode.OnRecvUpdateStatCB += OnRecvUpdateStat;

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

            inst.Init(entity.targetId, entity.stat, entity.pos.ConvertUnityVector3());

            if (ServerData.Instance.UserData.user_id == entity.userId)
            {
                Self = inst;
                inst.SetSelf();
            }

            _entityObjectDict.Add(entity.targetId, inst);
        }

        _cameraController.FollowingTarget(Self.transform);
    }


    /// <summary>
    /// 플레이어 오브젝트 가져오기
    /// </summary>
    /// <param name="inPlayerId"></param>
    /// <returns></returns>
    public void OnMoveSelf(in Vector3 inDir)
    {
        OnStopAttackSelf();

        float angle = Vector3.Angle(Self.GetDir(), inDir);

        // 조이스틱의 방향전환이 이루어졌다면..?
        if (angle > 1f)
        {
            // 서버에 전송한다.
            _gameMode.OnSendMove(Self.GetPos(), inDir);
        }

        SetMoveState(Self,
                     Self.GetPos(),
                     inDir,
                     Self.Stat.moveSpeed);
    }


    public void OnStopMoveSelf()
    {
        _gameMode.OnSendStopMove(Self.GetPos(), Self.GetDir());

        SetIdleState(Self,
                     Self.GetPos(),
                     Self.GetDir(),
                     Self.Stat.moveSpeed);

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
                // 추격시 서버에 전송한다.
                _gameMode.OnSendMove(Self.GetPos(), dir);
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
        var selfAttactValue = Self.Stat.GetAttackValue();
        var damagedVal = inTarget.Stat.GetDamagedValue(selfAttactValue);

        SetAttackState(inTarget, Self, Self.GetPos(), Self.GetDir(), damagedVal);

        _gameMode.OnSendAttack(Self.GetPos(), 
                               Self.GetDir(), 
                               inTarget.IsPlayer, 
                               inTarget.TargetId, 
                               damagedVal);

        _attackDelay = Observable.Timer(TimeSpan.FromSeconds(Self.Stat.attackSpeed))
                                        .Subscribe(_ =>
        {
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


    //----------------
    // server
    //----------------

    public override void OnRecvDead(S_Dead inEntity)
    {
        base.OnRecvDead(inEntity);

        if (inEntity.toId == Self.TargetId)
        {
            _touchController.SetActive(true);
            _joystickController.HideJoystick();
        }
    }


    public override void OnRecvRespawn(Entity inEntity)
    {
        base.OnRecvRespawn(inEntity);

        if (Self.TargetId == inEntity.targetId)
        {
            Self.SetSelf();

            _touchController.SetActive(false);
            _joystickController.ShowJoystick();
        }
    }

    public void OnRecvUpdateStat(PlayerEntity inEntity)
    {
        var player = GetEntityObject(inEntity.targetId);
        player.UpdateHp();
    }
}
