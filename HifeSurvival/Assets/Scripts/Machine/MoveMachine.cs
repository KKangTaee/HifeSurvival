using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveMachine : MonoBehaviour
{
    private const float MOVE_REACHING_OFFSET = 0.2f;

    private Vector3 _currPos;
    private Vector3 _endPos;
    private Vector3 _inputDirection;

    private Action _doneCallback;
    private Action<Vector2> _changeDirCallback;
    private float _currSpeed;
    private bool _isRunningLerp;

    private AStar _astar;
    private EntityObject _followTarget;

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

    private bool IsReaching(Vector3 inPos)
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


    public void MoveLerp(in Vector2 inPos, float inSpeed, Action doneCallback)
    {
        _inputDirection = Vector2.zero;

        _endPos = inPos;
        _currSpeed = inSpeed;
        
        _doneCallback = doneCallback;

        StartMoveLerp();
    }

    public void MoveStop(in Vector2 inPos)
    {
        _inputDirection = Vector2.zero;

       StopMoveLerp();
    }


    public void MoveFollowTarget(EntityObject inTarget, Action<Vector3> onChangeDirCB, Action doneCallbackk)
    {
        _followTarget = inTarget;
        StartCoroutine(nameof(Co_MoveFollowTarget));
    }


    private void UpdateMove()
    {
        if (_inputDirection != Vector3.zero)
        {
            OnChangeDir(_inputDirection);
            transform.position += (_inputDirection * _currSpeed * Time.fixedDeltaTime);
        }
    }


    private void OnChangeDir(Vector3 inDir)
    {
        if (CurrDir != inDir)
        {
            _changeDirCallback?.Invoke(inDir);
            CurrDir = inDir;
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
        }
    }



    //----------------
    // coroutines
    //----------------

    IEnumerator Co_MoveLerp()
    {
        while (IsReaching(_endPos) == false)
        {
            var dir = GetDir(_endPos);

            transform.position += dir * _currSpeed * Time.deltaTime;
            yield return null;
        }

        _doneCallback?.Invoke();
        _isRunningLerp = false;
    }

    IEnumerator Co_MoveFollowTarget()
    {
        yield return null;
    }
}