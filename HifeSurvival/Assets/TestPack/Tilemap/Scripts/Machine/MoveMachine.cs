using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveMachine : MonoBehaviour
{
    public enum EMoveStatus
    {
        AUTO,  // 자동

        MANUAL, // 수동
    
        NONE,
    }


    private const float    MOVE_SPEED = 10.0f;
    private const float    MOVE_REACHING_OFFSET = 0.2f;

    private Queue<Vector3>  _moveQueue;
    private Vector3         _nextPos;
    private Vector3         _inputDirection;

    private Action<bool>    _doneCallback;
    private Action<Vector2> _changeDirCallback;
    private EMoveStatus     _eStatus = EMoveStatus.NONE;
    private Vector3         _currDir;


    //-----------------
    // unity events
    //-----------------

    private void FixedUpdate()
    {
        UpdateMoveManual();
    }


    //-----------------
    // functions
    //-----------------


    private bool IsReaching(Vector3 inPos)
    {
        return Mathf.Abs(transform.position.x - inPos.x) < MOVE_REACHING_OFFSET &&
               Mathf.Abs(transform.position.y - inPos.y) < MOVE_REACHING_OFFSET;
    }


    private Vector3 GetDir(Vector3 inPos)
    {
        return Vector3.Normalize(inPos - transform.position);
    }


    public void MoveAuto(List<Vector3> inMoveList, Action<Vector2> changeDirCallback = null, Action<bool> doneCallback = null)
    {
        MoveStop();

        _moveQueue = new Queue<Vector3>(inMoveList);

        _doneCallback = doneCallback;
        _changeDirCallback = changeDirCallback;

        StartCoroutine(nameof(Co_Move));
    }


    public void MoveStop()
    {
        _moveQueue    = null;
        _doneCallback = null;

        StopCoroutine(nameof(Co_Move));
    }


    public void MoveManual(in Vector3 inPos)
    {
        if (_eStatus == EMoveStatus.AUTO)
            MoveStop();

        _inputDirection = inPos;
        _eStatus = EMoveStatus.MANUAL;
    }

    
    private void UpdateMoveManual()
    {
        if (_inputDirection != Vector3.zero)
        {
            OnChangeDir(_inputDirection);

            transform.position += (_inputDirection * MOVE_SPEED * Time.fixedDeltaTime);   
            _inputDirection = Vector3.zero;
        }
    }


    //-----------------
    // coroutines
    //-----------------

    private IEnumerator Co_Move()
    {
        while (_moveQueue?.Count > 0)
        {
            _nextPos = _moveQueue.Dequeue();

            while (!IsReaching(_nextPos))
            {
                OnChangeDir(GetDir(_nextPos));
               
                transform.position += _currDir * MOVE_SPEED * Time.deltaTime;
                
                yield return null;
            }

            if (_moveQueue.Count == 0)
            {
                _doneCallback?.Invoke(true);
                _doneCallback = null;
            }
        }
    }

    private void OnChangeDir(Vector3 inDir)
    {
        if(_currDir != inDir)
        {
            _changeDirCallback?.Invoke(inDir);
            _currDir = inDir;
        }
    }
}