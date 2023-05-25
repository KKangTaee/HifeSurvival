using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : ControllerBase, TouchController.ITouchUpdate
{
    [SerializeField] private Player _playerPrefab;

    private Dictionary<int, Player> _playerDict = new Dictionary<int, Player>();

    private CameraController _cameraController;

    private GameMode _gameMode;

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

        var spawnObj = inWorldMap.GetWorldObject<WorldSpawn>().FirstOrDefault(x=>x.SpawnType == WorldSpawn.ESpawnType.PLAYER);

        if(spawnObj == null && spawnObj?.GetPivotCount() <= 0)
        {
            Debug.LogError($"[{nameof(LoadPlayer)}] spawnObj is null or empty!");
            return;
        }

        foreach (var entity in entitys)
        {
            int randIdx = Random.Range(0, randList.Count);
            int pivotIdx = randList[randIdx];

            randList.RemoveAt(randIdx);

            var inst = Instantiate(_playerPrefab, transform);

            bool isSelf = ServerData.Instance.UserData.user_id == entity.userId;

            if(isSelf == true)
               Self = inst;

            inst.Init(isSelf, spawnObj.GetSpawnWorldPos(pivotIdx));
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
        if(_playerDict.TryGetValue(inPlayerId, out var player) == true && player != null)
            return player;

        Debug.LogError("player is null or empty");
        return player;
    }


    public void SetMoveState(Player inTarget, in Vector3 inPos, in Vector3 inDir, float speed)
    {
        var moveParam = new MoveParam()
        {
            pos = inPos,
            dir = inDir,
            speed = speed,
        };

        // 계속 이동중에 신호를 받은거라면 사실 상태를 변경할 필요 없음.
        if (inTarget.Status == Player.EStatus.MOVE)
            inTarget.UpdateState(moveParam);

        else
            inTarget.ChangeState(Player.EStatus.MOVE, moveParam);
    }


    public void OnMoveSelf(in Vector3 inDir)
    {
        float angle = Vector3.Angle(Self.GetDir(), inDir);

        // 조이스틱의 방향전환이 이루어졌다면..?
        if(angle > 5f)
        {
            // 서버에 전송한다.
            _gameMode.OnSendMove(Self.GetPos(), inDir);
        }

        SetMoveState(Self, 
             Self.GetPos(), 
             inDir, 
             GameMode.Instance.EntitySelf.speed);
    }


    public void OnStopMoveSelf()
    {
        _gameMode.OnSendStopMove(Self.GetPos());
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
             inEntity.speed);
    }


    public void OnRecvStopMove(PlayerEntity inEntity)
    {
        var player = GetPlayer(inEntity.playerId);

        player.ChangeState(Player.EStatus.IDLE, new IdleParam()
        {
            isSelf = player == Self,
            pos = inEntity.pos.ConvertUnityVector3()
        });
    }
}
