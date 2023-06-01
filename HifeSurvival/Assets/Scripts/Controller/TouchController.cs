using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;


public class TouchController : ControllerBase
{

    //------------------
    // enums
    //------------------

    public enum ETouchState : uint
    {
        DOWN,

        TOUCHING,

        UP,

        NONE,
    }


    public enum ETouchCommand : uint
    {
        //------------
        // 카메라 관련
        //------------
        CAMERA_ZOOM = 0,            // 카메라 줌

        CAMERA_MOVE,                // 카메라 무브

        CAMERA_MOVE_DONE,           // 카메라 무빙끝남

        CAMERA_COMMAND_END = 100,


        //-------------
        // 입력 관련
        //-------------
        WORLD_MAP_TOUCH = 201,      // 맵 터치

        PLAYER_TOUCH,               // 플레이어 터치

        NONE,
    }



    //-----------------
    // structs
    //-----------------

    public struct TouchResult
    {
        public ETouchState state;
        public TouchPhase  phase;
        public Vector2[]   posArr;
        public float       touchPressure;

        public int touchCount => posArr?.Length ?? 0;
    }



    //-----------------
    // interface
    //-----------------

    public interface ITouchUpdate
    {
        void OnTouchUpdate(ETouchCommand inCommand, Vector2[] inTouchPos, Collider2D collider = null);
    }



    //------------------
    // variables
    //------------------

    private ETouchState      _eTouch   = ETouchState.NONE;
    private ETouchCommand    _eCommand = ETouchCommand.NONE;

    private CameraController _cameraController;
    private PlayerController _playerController;

    private Vector2          _prevMousePos;
    private float            _mouseWheelDelta = 50;

    private float            _touchingDelta;


    //------------------
    // unity events
    //------------------

    private void Update()
    {
        TouchResult result = default;

        if (OnTouchDown(out result) || OnTouch(out result) || OnTouchUp(out result))
        {
            UpdateTouch(result);
        }
    }


    //------------------
    // override
    //------------------

    public override void Init()
    {
        _cameraController = ControllerManager.Instance.GetController<CameraController>();

        _playerController = ControllerManager.Instance.GetController<PlayerController>();
    }



    //------------------
    // fuctions
    //------------------


    public bool OnTouchDown(out TouchResult inResult)
    {
        Vector2[] touchPosArr = null;

        if (_eTouch == ETouchState.NONE)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                touchPosArr = new Vector2[] { Input.mousePosition };
            }
#else
            if (Input.touchCount > 0)
            {
                var touchs = Input.touches;
                if (touchs.All(x => x.phase == TouchPhase.Began))
                {
                    touchPosArr = touchs.Select(x => x.position).ToArray();
                }
            }
#endif
        }

        if (touchPosArr?.Length > 0)
        {
            _eTouch = ETouchState.DOWN;
            _touchingDelta = 0;
            inResult = new TouchResult()
            {
                state = _eTouch,
                posArr = touchPosArr,
                phase = TouchPhase.Began,
                touchPressure = _touchingDelta,
            };

            return true;
        }

        inResult = default;
        return false;
    }


    public bool OnTouch(out TouchResult inResult)
    {
        Vector2[]   touchPosArr = null;
        TouchPhase  touchPhase = TouchPhase.Canceled;

#if UNITY_EDITOR
        // ���콺 �� ó��
        float mouseWheelInput = Input.mouseScrollDelta.y;

        if (Mathf.Abs(mouseWheelInput) > 0.01f)
        {
            _mouseWheelDelta += mouseWheelInput;
            _mouseWheelDelta = Mathf.Clamp(_mouseWheelDelta, 0, 100);

            touchPosArr = new Vector2[]
            {
                  Vector2.one *  _mouseWheelDelta,
                 -Vector2.one *  _mouseWheelDelta
            };
            touchPhase = TouchPhase.Moved;
        }
#endif


        if (_eTouch == ETouchState.DOWN || _eTouch == ETouchState.TOUCHING)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButton(0))
            {
                var currMousePos = Input.mousePosition;


                touchPosArr = new Vector2[] { Input.mousePosition };
                touchPhase  = Mathf.Abs(currMousePos.x - _prevMousePos.x) < 0.33f &&
                              Mathf.Abs(currMousePos.y - _prevMousePos.y) < 0.33f ? TouchPhase.Stationary
                                                                                  : TouchPhase.Moved;

                _prevMousePos = currMousePos;
               
            }
#else
            if (Input.touchCount > 0)
            {
                var touchs = Input.touches;
                if (touchs.All(x => x.phase == TouchPhase.Moved || x.phase == TouchPhase.Stationary))
                {
                    touchPosArr = touchs.Select(x => x.position).ToArray();
                    touchPhase  = touchs.All(x => x.phase == TouchPhase.Stationary) ? TouchPhase.Stationary
                                                                                    : TouchPhase.Moved;
                }
            }
#endif
        }

        if (touchPosArr?.Length > 0)
        {
            _eTouch = ETouchState.TOUCHING;
            _touchingDelta += Time.deltaTime;
            inResult = new TouchResult()
            {
                state = _eTouch,
                posArr = touchPosArr,
                phase = touchPhase,
                touchPressure = _touchingDelta,
            };

            return true;
        }

        inResult = default;
        return false;
    }



    public bool OnTouchUp(out TouchResult inResult)
    {
        Vector2[]  touchPosArr = null;

        if (_eTouch == ETouchState.DOWN || _eTouch == ETouchState.TOUCHING)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
            {
                touchPosArr = new Vector2[] { Input.mousePosition };
                _touchingDelta = 0;
            }
#else
            if (Input.touchCount > 0)
            {
                var touchs = Input.touches;
                if (touchs.All(x => x.phase == TouchPhase.Ended))
                {
                    touchPosArr = touchs.Select(x => x.position).ToArray();
                }
            }
#endif
        }

        if (touchPosArr?.Length > 0)
        {
            _touchingDelta = 0;
            inResult = new TouchResult()
            {
                state = ETouchState.UP,
                posArr = touchPosArr,
                phase = TouchPhase.Ended,
                touchPressure = _touchingDelta,
            };

            _eTouch = ETouchState.NONE;
            return true;
        }

        inResult = default;
        return false;
    }



    public void UpdateTouch(TouchResult inResult)
    {
        Collider2D col = null;

        switch (inResult.state)
        {
            case ETouchState.DOWN:

                break;

            case ETouchState.TOUCHING:

                if(inResult.touchPressure > 0.2f && inResult.phase == TouchPhase.Moved)
                {
                    _eCommand = inResult.touchCount == 1 ? ETouchCommand.CAMERA_MOVE
                                                         : ETouchCommand.CAMERA_ZOOM;
                }

                break;

            case ETouchState.UP:

                if (_eCommand == ETouchCommand.CAMERA_MOVE)
                {
                    _eCommand = ETouchCommand.CAMERA_MOVE_DONE;
                }
                else if(inResult.touchCount == 1)
                {
                    col = GetCollider2D(inResult.posArr.First());

                    if (col?.CompareTag(TagName.WORLDMAP_TOUCH) == true)
                    {
                        _eCommand = ETouchCommand.WORLD_MAP_TOUCH;
                    }
                    else if(col?.CompareTag(TagName.PLAYER_OTHER) == true)
                    {
                        _eCommand = ETouchCommand.PLAYER_TOUCH;
                    }
                    else
                    {
                        _eCommand = ETouchCommand.NONE;
                    }
    
                }
                else
                {
                    _eCommand = ETouchCommand.NONE;
                }

                break;

            default:
                _eCommand = ETouchCommand.NONE;
                break;
        }

        
        switch(_eCommand)
        {
            case ETouchCommand.CAMERA_ZOOM:
            case ETouchCommand.CAMERA_MOVE:
            case ETouchCommand.CAMERA_MOVE_DONE:
                _cameraController.OnTouchUpdate(_eCommand, inResult.posArr);
                break;
        }
    }


    public static Collider2D GetCollider2D(Vector2 inPos)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inPos);
        RaycastHit2D hitInfo = Physics2D.Raycast(mousePosition, Vector2.zero);

        return hitInfo.collider;
    }


}
