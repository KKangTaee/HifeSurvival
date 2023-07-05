using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FXBase : MonoBehaviour
{
    [SerializeField]  ParticleSystem _fx;
    private Action _doneCallback;

    public   EFX_ID Id { get; private set; }
    
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
        Id = id;
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
