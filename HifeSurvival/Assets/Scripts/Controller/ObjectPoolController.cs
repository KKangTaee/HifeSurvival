using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class ObjectPoolAttribute : System.Attribute
{
    public string PATH_IN_NOT_RESOURCES_FOLDER;     // 리소스폴더 아닌 곳의 경로 (번들용도)
    public string PATH_IN_RESOURCES_FOLDER;         // 리소스폴더
    public bool   IN_RESOURCES_FORLDER;             // 해당 팝업이 어디 경로에 있는지 유무 체크  
}

public class ObjectPoolController : ControllerBase
{
    private Dictionary<Type, Queue<Component>> _objectPoolDicts = new Dictionary<Type, Queue<Component>>();


    //------------------
    // overrides
    //------------------

    public override void Init()
    {

    }


    public T SpawnFromPool<T>() where T : Component
    {
        T inst = null;

        if (_objectPoolDicts.TryGetValue(typeof(T), out var poolQueue) == true && poolQueue.Count > 0)
        {
            inst = poolQueue.Dequeue() as T;
        }
        else
        {
            inst = LoadAsset<T>();           
        }

        inst.transform.SetParent(null);
        inst.gameObject.SetActive(true);

        return inst;
    }

    public void StoreToPool<T>(T inObj) where T : Component
    {
        inObj.gameObject.SetActive(false);
        inObj.transform.SetParent(this.transform);
        inObj.transform.position = new Vector3(-9999, -9999, -9999);

        if (_objectPoolDicts.ContainsKey(typeof(T)) == true)
        {
            _objectPoolDicts[typeof(T)].Enqueue(inObj);
        }
        else
        {
            var poolQueue = new Queue<Component>();
            poolQueue.Enqueue(inObj);

            _objectPoolDicts.Add(typeof(T), poolQueue);
        }
    }

    public T LoadAsset<T>() where T : Component
    {
        Type assetType = typeof(T);

        T asset = null;

        foreach (var attr in assetType.GetCustomAttributes(true))
        {
            if (attr is ObjectPoolAttribute poolAttr)
            {
                if (poolAttr.IN_RESOURCES_FORLDER == true)
                {
                    asset = Resources.Load<T>(poolAttr.PATH_IN_RESOURCES_FOLDER);
                }
                else
                {
                    // TODO@taeho.kang 에셋번들에서 로드
                }
            }
        }

        if(asset == null)
        {
            Debug.LogError($"[{nameof(T)}] is null or empty!");
            return null;
        }

        var inst = Instantiate(asset);

        return inst; 
    }
}