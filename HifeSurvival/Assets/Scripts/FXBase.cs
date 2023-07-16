using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FXBase : MonoBehaviour
{
    [SerializeField] ParticleSystem _fx;
    [SerializeField] [HideInInspector] EFX_ID _id;

    private Action _doneCallback;
    public   EFX_ID FX_ID { get => _id; }
    
    public void Play(Action doneCallback = null)
    {
        _doneCallback = doneCallback;
        StartCoroutine(nameof(Co_Update));
    }

    public void Stop()
    {
        _fx.Stop();
    }

    public void SetId(EFX_ID id)
    {
        _id = id;
    }

    IEnumerator Co_Update()
    {
        _fx.Play();

        while(_fx.IsAlive(true) == true)
            yield return null;
        
        _doneCallback?.Invoke();
        _doneCallback = null;
    }
}
