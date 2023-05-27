using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(CircleCollider2D))]
public class TriggerMachine : MonoBehaviour
{
    private event Action<Collider2D> _enterEvents;
    private event Action<Collider2D> _stayEvents;
    private event Action<Collider2D> _exitEvents;

    private CircleCollider2D _collider;

    //----------------
    // unity events
    //----------------

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        _enterEvents?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        _stayEvents?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _exitEvents?.Invoke(collision);
    }


    //------------------
    // functions
    //------------------


    public void AddTriggerEnter(Action<Collider2D> inCallback) =>
        _enterEvents = inCallback;

    public void AddTriggerStay(Action<Collider2D> inCallback) =>
        _stayEvents = inCallback;

    public void AddTriggerExit(Action<Collider2D> inCallback) =>
        _exitEvents = inCallback;

    public void SetBoxSize(float inRadius) =>
        _collider.radius = inRadius;

}
