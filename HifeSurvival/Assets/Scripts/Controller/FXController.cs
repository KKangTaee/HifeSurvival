using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EFX_ID
{
    NONE = 0,
    HIT_A_DIRECTIONAL_RED = 1001, // 플레이어 일반 공격
}

public class FXController : ControllerBase
{
    private Dictionary<EFX_ID, Queue<FXBase>> _fxPoolDict = new Dictionary<EFX_ID, Queue<FXBase>>();

    public const string RESOURCE_LOAD_PATH = "Particles";


    public override void Init()
    {
        PreLoad();
    }

    private void PreLoad()
    {
        foreach(EFX_ID type in Enum.GetValues(typeof(EFX_ID)))
        {
            if(type == EFX_ID.NONE)
                continue;

            var fx = SpawnFX(type);
            StoreFX(fx);
        }
    }

    public void Play(EFX_ID etype, in Vector3 pos, Action doneCallback = null)
    {
        var fx = SpawnFX(etype);
        fx.transform.position = pos;
        fx.Play(()=>
        {
            doneCallback?.Invoke();
            StoreFX(fx);
        });
    }

    public FXBase SpawnFX(EFX_ID etype)
    {
        FXBase fx = null;

        if(_fxPoolDict.TryGetValue(etype, out var fxQueue) == true && fxQueue?.Count > 0)
        {
            fx = fxQueue.Dequeue();
        }
        else
        {
            // TODO@taeho.kang 에셋번들 관련 코드 수정
            fx = Resources.Load<FXBase>($"{RESOURCE_LOAD_PATH}/FXBase_{(int)etype}");
            
            Debug.Log($"{nameof(SpawnFX)} : {fx.FX_ID}");
            
            fx = Instantiate(fx);

            fx.transform.parent = this.transform;
        }

        fx.gameObject.SetActive(true);
        return fx;
    }

    public void StoreFX(FXBase fxBase)
    {
        Queue<FXBase> targetPool = null;

        Debug.Log($"[{nameof(StoreFX)}] : {fxBase.FX_ID}");

        if(_fxPoolDict.TryGetValue(fxBase.FX_ID, out var fxQueue) == true)
        {
            targetPool = fxQueue;
        }
        else
        {
            // TODO@taeho.kang 에셋번들 관련 코드 수정
            targetPool = new Queue<FXBase>();
            targetPool.Enqueue(fxBase);
            _fxPoolDict.Add(fxBase.FX_ID, targetPool);
        }       
    }
}