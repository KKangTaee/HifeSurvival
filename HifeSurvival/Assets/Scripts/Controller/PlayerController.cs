using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

public class PlayerController : ControllerBase, TouchController.ITouchUpdate
{
    [SerializeField] private Player _playerPrefab;

    private Dictionary<int, Player> _playerDict = new Dictionary<int, Player>();

    private CameraController _cameraController;

    private GameMode _gameMode;

    private IDisposable _attackDelay;

    public Player Self { get; private set; }


    //-----------------
    // untiy events
    //-----------------

    public void Start()
    {
        _cameraController = ControllerManager.Instance.GetController<CameraController>();

        _gameMode = GameMode.Instance;

        _gameMode.OnRecvMoveCB += OnRecvMove;
        _gameMode.OnRecvStopMoveCB += OnRecvStopMove;
    }


    //----------------
    // overrides
    //----------------

    public void OnTouchUpdate(TouchController.ETouchCommand inCommand, Vector2[] inTouchPos, Collider2D inCollider2D = null)
    {
        switch (inCommand)
        {
            case TouchController.ETouchCommand.WORLD_MAP_TOUCH:

                var worldMap = inCollider2D.GetComponent<WorldMap>();

                if (worldMap != null)
                {
                    var touchPos = inTouchPos.FirstOrDefault();
                    var endPos = _cameraController.MainCamera.ScreenToWorldPoint(touchPos);

                    // MoveMeAuto(worldMap, endPos);
                }

                break;

            case TouchController.ETouchCommand.PLAYER_TOUCH:

                var player = inCollider2D.GetComponent<Player>();

                break;
        }
    }


    //-----------------
    // functions
    //-----------------

    /// <summary>
    /// 플레이어 오브젝트 로드
    /// </summary>
    /// <param name="inWorldMap"></param>
    public void LoadPlayer(WorldMap inWorldMap)
    {
        var entitys = GameMode.Instance.PlayerEntitysDic.Values;

        var randList = Enumerable.Range(0, entitys.Count).ToList();

        var spawnObj = inWorldMap.GetWorldObject<WorldSpawn>().FirstOrDefault(x => x.SpawnType == WorldSpawn.ESpawnType.PLAYER);

        if (spawnObj == null && spawnObj?.GetPivotCount() <= 0)
        {
            Debug.LogError($"[{nameof(LoadPlayer)}] spawnObj is null or empty!");
            return;
        }

        int idx = 0;

        foreach (var entity in entitys)
        {
            var inst = Instantiate(_playerPrefab, transform);

            inst.Init(entity.playerId, spawnObj.GetSpawnWorldPos(idx++));

            if (ServerData.Instance.UserData.user_id == entity.userId)
            {
                Self = inst;
                inst.SetSelf(entity.stat.detectRange, entity.stat.attackRange);
            }

            _playerDict.Add(entity.playerId, inst);
        }

        _cameraController.FollowingTarget(Self.transform);
    }


    /// <summary>
    /// 플레이어 오브젝트 가져오기
    /// </summary>
    /// <param name="inPlayerId"></param>
    /// <returns></returns>
    public Player GetPlayer(int inPlayerId)
    {
        if (_playerDict.TryGetValue(inPlayerId, out var player) == true && player != null)
            return player;

        Debug.LogError("player is null or empty");
        return player;
    }


    public void OnMoveSelf(in Vector3 inDir)
    {
        OnStopAttackSelf();

        float angle = Vector3.Angle(Self.GetDir(), inDir);

        // 조이스틱의 방향전환이 이루어졌다면..?
        if (angle > 2f)
        {
            // 서버에 전송한다.
            _gameMode.OnSendMove(Self.GetPos(), inDir);
        }

        SetMoveState(Self,
                     Self.GetPos(),
                     inDir,
                     4);
    }


    public void OnStopMoveSelf()
    {
        _gameMode.OnSendStopMove(Self.GetPos(), Self.GetDir());

        SetIdleState(Self,
                     Self.GetPos(),
                     Self.GetDir(),
                     GameMode.Instance.EntitySelf.stat.speed);

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
            OnAttackSelf(target, 100);
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
                OnAttackSelf(inTarget, 100);
            }
        };

        Self.ChangeState(EntityObject.EStatus.FOLLOW_TARGET, followTargetParam);
    }


    public void OnAttackSelf(EntityObject inTarget, int inDamageVal)
    {
        _gameMode.OnSendAttack(true, inTarget.targetId, Self.targetId, 10);

        var attackParam = new AttackParam()
        {
            attackValue = inDamageVal,
            target = inTarget,
            attackDoneCallback = (isKill) =>
            {
                _attackDelay = Observable.Timer(TimeSpan.FromSeconds(1))
                                         .Subscribe(_=>
                {
                    // 상대방을 죽였다면..? 다른 타겟을 찾아라.
                    if(isKill == true)
                    {
                        DetectTargetSelf();
                    }
                    
                    // 아니라면..? 계속 공격
                    else
                    {
                        OnAttackSelf(inTarget, 100);
                    }
                });
            }
        };

        Self.ChangeState(EntityObject.EStatus.ATTACK, attackParam);
    }


    public void OnStopAttackSelf()
    {
        _attackDelay?.Dispose();
    }


    public void SetMoveState(Player inTarget, in Vector3 inPos, in Vector3 inDir, float speed)
    {
        var moveParam = new MoveParam()
        {
            pos = inPos,
            dir = inDir,
            speed = speed,
        };

        inTarget.ChangeState(Player.EStatus.MOVE, moveParam);
    }


    public void SetIdleState(Player inTarget, in Vector3 inPos, in Vector3 inDir, float inSpeed)
    {
        var idlePos = new IdleParam()
        {
            isSelf = inTarget == Self,
            pos = inPos,
            dir = inDir,
            speed = inSpeed,
        };

        inTarget.ChangeState(Player.EStatus.IDLE, idlePos);
    }


    //----------------
    // server
    //----------------

    public void OnRecvMove(PlayerEntity inEntity)
    {
        var player = GetPlayer(inEntity.playerId);

        SetMoveState(player,
             inEntity.pos.ConvertUnityVector3(),
             inEntity.dir.ConvertUnityVector3(),
             inEntity.stat.speed);
    }


    public void OnRecvStopMove(PlayerEntity inEntity)
    {
        var player = GetPlayer(inEntity.playerId);

        SetIdleState(player,
            inEntity.pos.ConvertUnityVector3(),
            inEntity.dir.ConvertUnityVector3(),
            inEntity.stat.speed);
    }


    public void OnRecvAttack(PlayerEntity inEntity)
    {
        var player = GetPlayer(inEntity.playerId);

        OnAttackSelf(player, 10);
    }
}
