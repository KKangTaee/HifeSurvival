using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : ControllerBase, TouchController.ITouchUpdate
{
    [SerializeField] private Player _playerPrefab;

    private Dictionary<string, Player> _playerDic = new Dictionary<string, Player>();

    private CameraController _cameraController;

    public Player Self { get; private set; }
    


    //------------------
    // unity events
    //------------------

    private void Awake()
    {
        Self = Instantiate(_playerPrefab);
        Self.Initialzie(true, new Vector3(0, -17.5f, 0));

        _playerDic.Add("me", Self);
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

        if(moveList == null)
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


    
}
