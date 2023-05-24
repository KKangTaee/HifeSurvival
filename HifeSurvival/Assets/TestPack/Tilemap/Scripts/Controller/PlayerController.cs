using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : ControllerBase, TouchController.ITouchUpdate
{

    public const string OBJ_NAME = nameof(PlayerController);


    [SerializeField] private Player _playerPrefab;


    private Dictionary<string, Player> _playerDic = new Dictionary<string, Player>();


    private CameraController _cameraController;

    public Player Me { get; private set; }



    //------------------
    // unity events
    //------------------

    private void Awake()
    {
        Me = Instantiate(_playerPrefab);
        Me.Initialzie(true, new Vector3(0, -17.5f, 0));

        _playerDic.Add("me", Me);
    }


    public void Start()
    {
        _cameraController = ControllerManager.Instance.GetController<CameraController>();

        if (_cameraController == null)
            return;
    }


    //----------------
    // overrides
    //----------------


    public void OnTouchUpdate(TouchController.ETouchCommand inCommand, Vector2 [] inTouchPos, Collider2D inCollider2D = null)
    {
        switch (inCommand)
        {
            case TouchController.ETouchCommand.WORLD_MAP_TOUCH:

                var worldMap = inCollider2D.GetComponent<WorldMap>();

                if(worldMap != null)
                {
                    var touchPos = inTouchPos.FirstOrDefault();
                    var endPos = _cameraController.MainCamera.ScreenToWorldPoint(touchPos);

                    // 내 캐릭터 이동
                    MoveMeAuto(worldMap, endPos);
                }
           
                break;

            case TouchController.ETouchCommand.PLAYER_TOUCH:

                var player = inCollider2D.GetComponent<Player>();

                TouchPlayer(player);

                break;
        }
    }




    //-----------------
    // functions
    //-----------------
    

    public void MoveMeAuto(WorldMap inWorldMap, Vector2 inEndPos)
    {        
        var moveList = inWorldMap.GetMoveList(Me.transform.position, inEndPos);

        if(moveList == null)
        {
            Debug.LogWarning($"[{nameof(MoveMeAuto)}] EndTile is not going!");
            return;
        }

        Me.MoveAuto(moveList);

        _cameraController.FollowingTarget(Me.transform);
    }


    public void MoveMeManual(Vector2 inDir)
    {
        Me.MoveMenual(inDir);
    }

  
    public void TouchPlayer(Player inPlayer)
    {

    }


    public Player GetPlayer(string inId)
    {
        return _playerDic.TryGetValue(inId, out var player) == true ? player : null;
    }

}
