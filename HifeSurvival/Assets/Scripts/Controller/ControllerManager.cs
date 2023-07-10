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

    public const string RESOURCES_PATH = "Prefabs/Controllers";

    private Dictionary<Type, ControllerBase> _controllerDict = new Dictionary<Type, ControllerBase>();

    public async UniTask InitAsync()
    {
        var controllerName = new string[]
        {
            nameof(TouchController),

            nameof(CameraController),

            nameof(JoystickController),

            nameof(ObjectPoolController),

            nameof(PlayerController),

            nameof(MonsterController),

            nameof(FXController),
        };

        foreach (var name in controllerName)
        {
            string path = $"{RESOURCES_PATH}/{name}";

            ResourceRequest request = Resources.LoadAsync<ControllerBase>(path);
            await request;

            var prefab = request.asset as ControllerBase;

            if (prefab == null)
            {
                Debug.LogError($"{name} object couldn't be found! path : {path}");
                return;
            }

            var inst = UnityEngine.Object.Instantiate(prefab);
            inst.name = name;

            UnityEngine.Object.DontDestroyOnLoad(inst);

            _controllerDict.Add(inst.GetType(), inst);
        }

        // 초기화
        foreach(var controller in _controllerDict.Values)
            controller.Init();
    }

    public void Release()
    {
        foreach (var pair in _controllerDict)
            UnityEngine.Object.Destroy(pair.Value);

        _controllerDict.Clear();
        _instance = null;
    }

    public T GetController<T>() where T : ControllerBase
    {
        if (_controllerDict.TryGetValue(typeof(T), out var value) == true)
        {
            return value is T controller ? controller : null;
        }
        else
        {
            return null;
        }
    }
}
