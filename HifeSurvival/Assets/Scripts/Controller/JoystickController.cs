using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(CanvasRenderer))]
[RequireComponent(typeof(GraphicRaycaster))]
public class JoystickController : ControllerBase, TouchController.ITouchUpdate
{
    [SerializeField] JoystickMachine _joystickMachine;
    [SerializeField] Canvas          _canvas;
    [SerializeField] RectTransform  RT_stickPivot;
    [SerializeField] RectTransform  RT_background;

    private PlayerController         _playerController;

    //------------------
    // unity events
    //------------------

    private void Reset()
    {
        if(_joystickMachine == null)
        {
            for(int i = 0; i< transform.childCount; i++)
            {
                var tr = transform.GetChild(i);

                var comp = tr.GetComponent<JoystickMachine>();

                if(comp != null)
                {
                    _joystickMachine = comp;
                    break;
                }
            }

            if(_joystickMachine == null)
            {
                var empty = new GameObject();

                empty.name = nameof(JoystickMachine);
                empty.transform.parent = transform;

                _joystickMachine = empty.AddComponent<JoystickMachine>();
            }
        }
    }



    //------------------
    // override
    //------------------

    public override void Init()
    {
        _canvas.worldCamera = ControllerManager.Instance.GetController<CameraController>().MainCamera;

        _playerController   = ControllerManager.Instance.GetController<PlayerController>();

        _joystickMachine.AddDragEvent((dir) => _playerController.OnMoveSelf(dir));

        _joystickMachine.AddPointUpEvent(() => _playerController.OnStopMoveSelf());

        ShowJoystick();
    }

    public void OnTouchUpdate(TouchController.ETouchCommand inCommand, Vector2[] inTouchPos, Collider2D collider = null)
    {
         Vector2 pos = inTouchPos.FirstOrDefault();

        switch(inCommand)
        {
            case TouchController.ETouchCommand.JOYSTICK_DOWN:

                RT_background.position = pos;
                _joystickMachine.OnPointerDownV2(pos);
                
                break;

            case TouchController.ETouchCommand.JOYSTICK_TOUCHING:
                
                _joystickMachine.OnDragV2(pos);

                break;

            case TouchController.ETouchCommand.JOYSTICK_UP:
                float pivotDifferenceY = RT_background.pivot.y - RT_stickPivot.pivot.y;

                // B의 크기를 고려합니다.
                float correctedPositionY = RT_stickPivot.position.y + pivotDifferenceY * RT_stickPivot.rect.height;

                // A의 위치로 B를 이동시킵니다. x좌표는 동일하게 유지합니다.
                RT_background.position = new Vector3(RT_stickPivot.position.x, correctedPositionY, RT_stickPivot.position.z);

                _joystickMachine.OnPointerUpV2(pos);
                break;
        }
    }

    //------------------
    // functions
    //------------------

    public void ShowJoystick()
    {
        _joystickMachine.gameObject.SetActive(true);
    }

    public void HideJoystick()
    {
        _joystickMachine.gameObject.SetActive(false);
    }

    public bool IsShowJoyStick()
    {
        return _joystickMachine.gameObject.activeInHierarchy;
    }
}
