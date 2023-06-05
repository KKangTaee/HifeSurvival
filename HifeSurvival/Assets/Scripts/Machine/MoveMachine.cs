using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveMachine : MonoBehaviour
{
    private const float MOVE_REACHING_OFFSET = 1.2f;

    private Vector3 _currPos;
    private Vector3 _inputDirection;

    private Func<MoveMachine, Vector3>   _dirFunc;
    private Func<MoveMachine, bool>      _exitLoopFunc;
    private Func<Vector3>                _endPosFunc;
    private Func<bool>                  _forceStopFunc;

    private Action<Vector3>   _updateCallback;
    private Action          _doneCallback;

    private float   _currSpeed;
    private float   _updateRatio;
    private bool    _isRunningLerp;

    private AStar _astar;

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

    public bool IsReaching(Vector3 inPos)
    {
        return Mathf.Abs(transform.position.x - inPos.x) < MOVE_REACHING_OFFSET &&
               Mathf.Abs(transform.position.y - inPos.y) < MOVE_REACHING_OFFSET;
    }


    public Vector3 GetDir(Vector3 inPos)
    {
        return Vector3.Normalize(inPos - transform.position);
    }


    public void Move(in Vector3 inDir, float inSpeed)
    {
        StopMoveLerp();

        _inputDirection = inDir;
        _currSpeed = inSpeed;
    }


    public void MoveLerp(float inSpeed, Func<MoveMachine, Vector3> inDirFunc, Func<MoveMachine, bool> inExitLoop, Action doneCallback)
    {
        _inputDirection = Vector2.zero;
        _currSpeed = inSpeed;

        _dirFunc = inDirFunc;
        _exitLoopFunc = inExitLoop;
        _doneCallback = doneCallback;

        StartMoveLerp();
    }


    public void MoveLerpV2(float inSpeed, Func<Vector3> inEndPosFunc,  float inUpdateRatio, Action<Vector3> updateCallback, Func<bool> forceStopFunc,  Action doneCallback)
    {
        StopCoroutine(nameof(Co_MoveLerpV2));

        _inputDirection     = Vector2.zero;
        _currSpeed          = inSpeed;
        _doneCallback       = doneCallback;
        _endPosFunc         = inEndPosFunc;
        _updateCallback     = updateCallback;
        _updateRatio        = inUpdateRatio;
        _forceStopFunc      = forceStopFunc;

        StartCoroutine(nameof(Co_MoveLerpV2));
    }


    public void MoveStop(in Vector2 inPos)
    {
        _inputDirection = Vector2.zero;

       StopMoveLerp();
    }


    private void UpdateMove()
    {
        if (_inputDirection != Vector3.zero)
        {
            CurrDir = _inputDirection;
            transform.position += (CurrDir * _currSpeed * Time.fixedDeltaTime);
        }
    }


    public void StartMoveLerp()
    {
        if(_isRunningLerp == false)
        {
            _isRunningLerp = true;
            StartCoroutine(nameof(Co_MoveLerp));
        }
    }


    public void StopMoveLerp()
    {
        if(_isRunningLerp == true)
        {
            StopCoroutine(nameof(Co_MoveLerp));

            _isRunningLerp = false;

            _dirFunc = null;
            _doneCallback = null;
        }

        StopCoroutine(nameof(Co_MoveLerpV2));
    }


    //----------------
    // coroutines
    //----------------

    IEnumerator Co_MoveLerp()
    {
        while (_exitLoopFunc?.Invoke(this) == false)
        {
            CurrDir = _dirFunc?.Invoke(this) ?? Vector3.zero;

            transform.position += CurrDir * _currSpeed * Time.deltaTime;
            
            yield return null;
        }

        _doneCallback?.Invoke();
        _isRunningLerp = false;
    }


    IEnumerator Co_MoveLerpV2()
    {
        float lerpValue     = default;
        float updateRatio   = default;
        float distance      = default;

        Vector3 currPos     = default;
        Vector3 endPos      = default;

        if (Reset(out lerpValue, out updateRatio, out distance, out currPos, out endPos) == false)
            yield break;


        while(lerpValue / distance <= 1)
        {
            if (_forceStopFunc?.Invoke() ?? false)
                break;

          
            if(endPos != _endPosFunc?.Invoke())
            {
                if (Reset(out lerpValue, out updateRatio, out distance, out currPos, out endPos) == false)
                    yield break;
            }

            lerpValue += (_currSpeed * Time.deltaTime);
            float ratio = lerpValue / distance;

            CurrDir = GetDir(endPos);
            transform.position = Vector3.Lerp(currPos, endPos,  ratio);

            if (updateRatio < ratio)
            {
                _updateCallback?.Invoke(CurrDir);
                updateRatio += _updateRatio;
            }
            
            yield return null;
        }

        _doneCallback?.Invoke();

        #region Local Func
        bool Reset(out float inLerpVal, out float inUpdateRatio, out float inDistance, out Vector3 inCurrPos, out Vector3 inEndPos)
        {
            inLerpVal = 0;
            inUpdateRatio = _updateRatio;

            inCurrPos = transform.position;
            inEndPos = _endPosFunc?.Invoke() ?? inCurrPos;

            inDistance = Vector3.Distance(inCurrPos, inEndPos);

            return inCurrPos != inEndPos || inDistance < 0.1f;
        }
        #endregion
    }
}