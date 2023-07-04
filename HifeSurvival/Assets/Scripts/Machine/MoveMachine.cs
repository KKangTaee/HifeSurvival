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
    private Action<Vector3> _dirCallback;
    private Func<bool> _stopFunc;

    public Vector3 CurrDir { get; private set; }

    public const int EXPECT_LERP_MILLISECONDS = 250;
    
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

    public void MoveStopSelf()
    {
        _inputDirection = Vector2.zero;
    }

    public void StartMoveLerpExpect(in Vector3 inCurrPos, in Vector3 inDestPos, float inSpeed, long inTimeStamp, Action doneCallback = null)
    {
        _currPos = inCurrPos;
        _destPos = inDestPos;
        _currSpeed = inSpeed;
        _timeStamp = inTimeStamp;
        _doneCallback = doneCallback;
        
        StartCoroutine(nameof(Co_MoveLerpExpect));
    }

    public void StartMoveLerpTarget(EntityObject inTarget, float inSpeed, Func<bool> inStopFunc, Action<Vector3> dirCallback, Action doneCallback)
    {
        _target = inTarget;
        _currSpeed = inSpeed;
        _stopFunc = inStopFunc;
        _dirCallback = dirCallback;
        _doneCallback = doneCallback;

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
            
            _dirCallback?.Invoke(dir);
            
            transform.position += (dir * _currSpeed * Time.deltaTime);

            yield return null;
        }
        
        _doneCallback?.Invoke();
    }


    IEnumerator Co_MoveLerpExpect()
    {
        //------------
        // 보간방식
        //------------

        // long utcNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // long expectMilliseocnds = _timeStamp + EXPECT_LERP_MILLISECONDS;

        // float currTick = 0;
        // float totalTick = expectMilliseocnds - utcNow;

        // CurrDir = Vector3.Normalize(_currPos - _destPos);

        // do
        // {
        //     currTick += Time.deltaTime;
        //     transform.position = Vector3.Lerp(_currPos, _destPos, currTick/totalTick);
        //     yield return null;
        // }
        // while(currTick < totalTick);

        CurrDir = GetDir(_destPos);

        // NOTE@taeho.kang 임시처리 : 서버에서의 시작점으로 하면 안됨 (순간이동생김)

        Vector3 startPos = transform.position;
        Vector3 nowPos = startPos;

        float currDist = 0;
        float totalDist = Vector3.Distance(startPos, _destPos);

        while(currDist < totalDist)
        {
            nowPos += _currSpeed * Time.deltaTime * CurrDir;

            currDist = Vector3.Distance(startPos, nowPos);
            transform.position = Vector3.Lerp(startPos, _destPos, currDist/totalDist);

            yield return null;
        }
    }
}