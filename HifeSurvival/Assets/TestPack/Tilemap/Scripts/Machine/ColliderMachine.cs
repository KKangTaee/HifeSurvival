using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CircleCollider2D))]
public class ColliderMachine : MonoBehaviour
{
    private event EventHandler<Collision2D> _enterEvents;
    private event EventHandler<Collision2D> _stayEvents;
    private event EventHandler<Collision2D> _exitEvents;

    private CircleCollider2D _collider;

    //----------------
    // unity events
    //----------------

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        _enterEvents?.Invoke(this, collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        _stayEvents?.Invoke(this, collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _exitEvents?.Invoke(this, collision);
    }


    //------------------
    // functions
    //------------------


    public void AddCollisionEnter(EventHandler<Collision2D> inCallback) =>
        _enterEvents = inCallback;

    public void AddCollisionEStay(EventHandler<Collision2D> inCallback) =>
        _stayEvents = inCallback;

    public void AddCollisionEExit(EventHandler<Collision2D> inCallback) =>
        _exitEvents = inCallback;

    public void SetCircleRadius(float inRadius) =>
        _collider.radius = inRadius;
}
