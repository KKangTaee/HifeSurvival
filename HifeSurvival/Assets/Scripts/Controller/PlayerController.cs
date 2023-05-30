using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

public class PlayerController : ControllerBase
{
    [SerializeField] private Player _playerPrefab;

    private Dictionary<int, Player> _playerDict = new Dictionary<int, Player>();

    private CameraController    _cameraController;
    private JoystickController  _joystickController;

    private GameMode _gameMode;

    private IDisposable _attackDelay;

    public Player Self { get; private set; }


    //-----------------
    // untiy events
    //-----------------

    public void Start()
    {
        _cameraController   = ControllerManager.Instance.GetController<CameraController>();
        _joystickController = ControllerManager.Instance.GetController<JoystickController>();

        _gameMode = GameMode.Instance;

        _gameMode.OnRecvMoveCB      += OnRecvMove;
        _gameMode.OnRecvStopMoveCB  += OnRecvStopMove;
        _gameMode.OnRecvDeadCB      += OnRecvDead;
        _gameMode.OnRecvAttackCB    += OnRecvAttack;
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

            inst.Init(entity.playerId, entity.stat, spawnObj.GetSpawnWorldPos(idx++));

            if (ServerData.Instance.UserData.user_id == entity.userId)
            {
                Self = inst;
                inst.SetSelf();
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
                     Self.Stat.moveSpeed);
    }


    public void OnStopMoveSelf()
    {
        _gameMode.OnSendStopMove(Self.GetPos(), Self.GetDir());

        SetIdleState(Self,
                     Self.GetPos(),
                     Self.GetDir(),
                     GameMode.Instance.EntitySelf.stat.moveSpeed);

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

        _gameMode.OnSendAttack(true, inTarget.targetId, Self.targetId, damagedVal);

        var attackParam = new AttackParam()
        {
            attackValue = damagedVal,
            target = inTarget,
        };

        Self.ChangeState(EntityObject.EStatus.ATTACK, attackParam);

        _attackDelay = Observable.Timer(TimeSpan.FromSeconds(Self.Stat.attackSpeed))
                                        .Subscribe(_ =>
        {
            
            // 이미 상대가 죽었다면
            if(inTarget.Status == EntityObject.EStatus.DEAD)
            {
                // 다른 상대방을 감지하고 찾아라.
                DetectTargetSelf();
            }
            // 공격이 가능하다면 다시 공격.
            else if(Self.CanAttack(inTarget.GetPos()) == true)
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


    public void SetMoveState(Player inTarget, in Vector3 inPos, in Vector3 inDir, float speed)
    {
        var moveParam = new MoveParam()
        {
            pos = inPos,
            dir = inDir,
            speed = speed,
        };

        inTarget.ChangeState(EntityObject.EStatus.MOVE, moveParam);
    }


    public void SetIdleState(Player inTarget, in Vector3 inPos, in Vector3 inDir, float inSpeed)
    {
        var idleParam = new IdleParam()
        {
            isSelf = inTarget == Self,
            pos = inPos,
            dir = inDir,
            speed = inSpeed,
        };

        inTarget.ChangeState(EntityObject.EStatus.IDLE, idleParam);
    }


    public void SetDeadState(Player inTarget)
    {
        var deadParam = new DeadParam()
        {

        };

        inTarget.ChangeState(EntityObject.EStatus.DEAD, deadParam);
    }


    public void SetAttackState(Player inToId, Player inFromId, int inDamageVal)
    {
        var attackParam = new AttackParam()
        {
            attackValue = inDamageVal,
            target = inToId,
        };

        inFromId.ChangeState(EntityObject.EStatus.ATTACK, attackParam);
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
                     inEntity.stat.moveSpeed);
    }


    public void OnRecvStopMove(PlayerEntity inEntity)
    {
        var player = GetPlayer(inEntity.playerId);

        SetIdleState(player,
                     inEntity.pos.ConvertUnityVector3(),
                     inEntity.dir.ConvertUnityVector3(),
                     inEntity.stat.moveSpeed);
    }


    public void OnRecvDead(S_Dead inEntity)
    {
        var player = GetPlayer(inEntity.toId);

        if(player == Self)
           _joystickController.HideJoystick();

        SetDeadState(player);
    }


    public void OnRecvAttack(CS_Attack inPacket)
    {
        var toPlayer   = GetPlayer(inPacket.toId);
        var fromPlayer = GetPlayer(inPacket.fromId);

        SetAttackState(toPlayer, fromPlayer, inPacket.damageValue);
    }

    
}
