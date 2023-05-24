using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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


    public void Init()
    {
        var controllerName = new string []
        {
            nameof(TouchController),

            nameof(CameraController),

            nameof(PlayerController),

            nameof(JoystickController),

            nameof(AIController)
        };


        foreach (var name in controllerName)
        {
            var prefab = Resources.Load<ControllerBase>($"{RESOURCES_PATH}/{name}");

            if (prefab == null)
            {
                Debug.LogError($"{name} object couldnt find!");
                return;
            }

            prefab = UnityEngine.Object.Instantiate(prefab);
            prefab.name = name;

            UnityEngine.Object.DontDestroyOnLoad(prefab);
            _controllerDic.Add(prefab.GetType(), prefab);
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
