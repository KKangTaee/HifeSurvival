using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : ControllerBase, TouchController.ITouchUpdate
{
    [SerializeField] private Player _playerPrefab;

    private Dictionary<int, Player> _playerDict = new Dictionary<int, Player>();

    private CameraController _cameraController;

    public Player Self { get; private set; }

    public void Start()
    {
        _cameraController = ControllerManager.Instance.GetController<CameraController>();

        if (_cameraController == null)
            return;
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

                    MoveMeAuto(worldMap, endPos);
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


    public void MoveMeAuto(WorldMap inWorldMap, Vector2 inEndPos)
    {
        var moveList = inWorldMap.GetMoveList(Self.transform.position, inEndPos);

        if (moveList == null)
        {
            Debug.LogWarning($"[{nameof(MoveMeAuto)}] EndTile is not going!");
            return;
        }

        Self.MoveAuto(moveList);

        _cameraController.FollowingTarget(Self.transform);
    }


    public void MoveMeManual(Vector2 inDir)
    {
        Self.MoveMenual(inDir);
    }
    

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

            inst.Initialzie(isSelf, spawnObj.GetSpawnWorldPos(pivotIdx));
            _playerDict.Add(entity.playerId, inst);
        }
    }
}
