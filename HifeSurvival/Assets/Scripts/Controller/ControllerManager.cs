using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx.Async;


public class ControllerManager
{
    private static ControllerManager _instance;

    public static ControllerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ControllerManager();
            }

            return _instance;
        }
    }


    public const string RESOURCES_PATH = "Controllers";

    private Dictionary<Type, ControllerBase> _controllerDic = new Dictionary<Type, ControllerBase>();


    public async UniTask InitAsync()
    {
        var controllerName = new string[]
        {
            nameof(TouchController),

            nameof(CameraController),

            nameof(PlayerController),

            nameof(JoystickController),

            nameof(AIController)
        };


        foreach (var name in controllerName)
        {
            ResourceRequest request = Resources.LoadAsync<ControllerBase>($"{RESOURCES_PATH}/{name}");
            await request;

            var prefab = request.asset as ControllerBase;

            if (prefab == null)
            {
                Debug.LogError($"{name} object couldn't be found!");
                return;
            }

            var inst = UnityEngine.Object.Instantiate(prefab);
            inst.name = name;

            UnityEngine.Object.DontDestroyOnLoad(inst);
            _controllerDic.Add(inst.GetType(), inst);
        }
    }


    public void Release()
    {
        foreach (var pair in _controllerDic)
            UnityEngine.Object.Destroy(pair.Value);

        _controllerDic.Clear();
        _instance = null;
    }


    public T GetController<T>() where T : ControllerBase
    {
        if (_controllerDic.TryGetValue(typeof(T), out var value) == true)
        {
            return value is T controller ? controller : null;
        }

        else
        {
            return null;
        }
    }
}
