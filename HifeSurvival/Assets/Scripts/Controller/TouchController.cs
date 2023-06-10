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
        CAMERA_MOVE,                // 카메라 무브

        CAMERA_MOVE_DONE,           // 카메라 무빙끝남

        CAMERA_COMMAND_END = 100,


        //-------------
        // 조이스틱 관련
        //-------------
        JOYSTICK_DOWN,

        JOYSTICK_TOUCHING,

        JOYSTICK_UP,


        NONE,
    }



    //-----------------
    // structs
    //-----------------

    public struct TouchResult
    {
        public ETouchState state;
        public TouchPhase phase;
        public Vector2[] posArr;
        public float touchPressure;

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

    private ETouchState _eTouch = ETouchState.NONE;
    private ETouchCommand _eCommand = ETouchCommand.NONE;

    private CameraController _cameraController;
    private PlayerController _playerController;
    private JoystickController _joystickController;

    private Vector2 _prevMousePos;
    private float _mouseWheelDelta = 50;
    private float _touchingDelta;


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

        _joystickController = ControllerManager.Instance.GetController<JoystickController>();

        // SetActive(false);
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
        Vector2[] touchPosArr = null;
        TouchPhase touchPhase = TouchPhase.Canceled;

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
                touchPhase = Mathf.Abs(currMousePos.x - _prevMousePos.x) < 0.33f &&
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
        Vector2[] touchPosArr = null;

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
        switch (inResult.state)
        {
            case ETouchState.DOWN:
                // 여기서 조이스틱이 현재 화면에 노출중인지를 파악해야 함.
                if (_joystickController.IsShowJoyStick() == true)
                    _eCommand = ETouchCommand.JOYSTICK_DOWN;

                break;

            case ETouchState.TOUCHING:

                if (_joystickController.IsShowJoyStick() == true)
                {
                    _eCommand = ETouchCommand.JOYSTICK_TOUCHING;
                }
                else if (inResult.touchPressure > 0.2f && inResult.phase == TouchPhase.Moved)
                {
                    // _eCommand = inResult.touchCount == 1 ? ETouchCommand.CAMERA_MOVE
                    //                                      : ETouchCommand.CAMERA_ZOOM;


                    _eCommand = ETouchCommand.CAMERA_MOVE;
                }
                else
                {
                    _eCommand = ETouchCommand.NONE;
                }

                break;

            case ETouchState.UP:

                if (_eCommand == ETouchCommand.CAMERA_MOVE)
                    _eCommand = ETouchCommand.CAMERA_MOVE_DONE;

                else if (_eCommand == ETouchCommand.JOYSTICK_DOWN || _eCommand == ETouchCommand.JOYSTICK_TOUCHING)
                    _eCommand = ETouchCommand.JOYSTICK_UP;

                else
                    _eCommand = ETouchCommand.NONE;

                break;

            default:
                _eCommand = ETouchCommand.NONE;
                break;
        }


        switch (_eCommand)
        {
            case ETouchCommand.CAMERA_MOVE:
            case ETouchCommand.CAMERA_MOVE_DONE:
                _cameraController.OnTouchUpdate(_eCommand, inResult.posArr);
                break;

            case ETouchCommand.JOYSTICK_DOWN:
            case ETouchCommand.JOYSTICK_TOUCHING:
            case ETouchCommand.JOYSTICK_UP:
                _joystickController.OnTouchUpdate(_eCommand, inResult.posArr);
                break;
        }
    }


    public void SetActive(bool isTrue)
    {
        gameObject.SetActive(isTrue);
    }
}
