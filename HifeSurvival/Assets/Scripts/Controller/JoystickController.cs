using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(CanvasRenderer))]
[RequireComponent(typeof(GraphicRaycaster))]
public class JoystickController : ControllerBase
{
    [SerializeField] JoystickMachine _joystickMachine;
    [SerializeField] Canvas          _canvas;

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


    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
    }


    private void Start()
    {
        _playerController = ControllerManager.Instance.GetController<PlayerController>();

        _joystickMachine.AddDragEvent((dir) => _playerController.OnMoveSelf(dir));

        _joystickMachine.AddPointUpEvent(() => _playerController.OnStopMoveSelf());
    }
}
