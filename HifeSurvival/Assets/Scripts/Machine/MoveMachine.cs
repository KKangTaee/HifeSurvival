using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveMachine : MonoBehaviour
{
    private Vector3 _inputDirection;
    private Vector3 _currPos;
    private Vector3 _destPos;
    private float   _currSpeed;
    private long    _timeStamp;
    private EntityObject _target;
    

    private Action _doneCallback;
    private Action<Vector3> _updateDirCallback;
    private Func<bool> _stopFunc;

    public Vector3 CurrDir { get; private set; }

    
    //-----------------
    // unity events
    //-----------------

    private void FixedUpdate()
    {
        UpdateMove();
    }


    //-----------------
    // functions
    //-----------------

    public Vector3 GetDir(Vector3 inPos)
    {
        return Vector3.Normalize(inPos - transform.position);
    }

    public void MoveSelf(in Vector3 inDir, float inSpeed)
    {
        _inputDirection = inDir;
        _currSpeed = inSpeed;
    }

    public void MoveStopSelf(in Vector2 inPos)
    {
        _inputDirection = Vector2.zero;
    }

    public void StartMoveLerpExpect(in Vector3 inCurrPos, in Vector3 inDestPos, float inSpeed, long inTimeStamp)
    {
        _currPos = inCurrPos;
        _destPos = inDestPos;
        _currSpeed = inSpeed;
        _timeStamp = inTimeStamp;
    
        StartCoroutine(nameof(Co_MoveLerpExpect));
    }

    public void StartMoveLerpTarget(EntityObject inTarget, float inSpeed, Func<bool> inStopFunc, Action doneCallback)
    {
        _target = inTarget;
        _currSpeed = inSpeed;
        _stopFunc = inStopFunc;

        StartCoroutine(nameof(Co_MoveLerpTarget));
    }

    private void UpdateMove()
    {
        if (_inputDirection != Vector3.zero)
        {
            CurrDir = _inputDirection;
            transform.position += (CurrDir * _currSpeed * Time.fixedDeltaTime);
        }
    }

    public void StopMoveLerpExpect()
    {
        StopCoroutine(nameof(Co_MoveLerpExpect));
    }


    public void StopMoveLerpTarget()
    {
        StopCoroutine(nameof(Co_MoveLerpTarget));
    }


    //------------------
    // coroutine
    //-------------------


    IEnumerator Co_MoveLerpTarget()
    {
        while(_stopFunc?.Invoke() == false)
        {
            var dir = GetDir(_target.transform.position);
            transform.position += (dir * _currSpeed * Time.deltaTime);

            yield return null;
        }
        _doneCallback?.Invoke();
    }


    IEnumerator Co_MoveLerpExpect()
    {
        float currTick = 0;
        float totalTick = (_timeStamp + 250) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        CurrDir = Vector3.Normalize(_currPos - _destPos);

        while(currTick < totalTick)
        {
            currTick += Time.deltaTime;
            transform.position = Vector3.Lerp(_currPos, _destPos, currTick/totalTick);
            yield return null;
        }
    }

}