using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackMachine : MonoBehaviour
{
    [SerializeField] TriggerMachine _detectTrigger;

    private HashSet<EntityObject> _targetSet;

    private float _detectRange;
    private float _attackRange;

    private void SetDetectRange(float inRange)
    {
        _detectRange = inRange;
    }

    public void SetAttackRange(float inRange)
    {
        _attackRange = inRange;
    }

    public void SetTrigger(Func<Collider2D, EntityObject> inEnter, Func<Collider2D, EntityObject> inExit)
    {
        _detectTrigger.AddTriggerEnter(col =>
        {

        });

        _detectTrigger.AddTriggerExit(col =>
        {

        });
    }
}
