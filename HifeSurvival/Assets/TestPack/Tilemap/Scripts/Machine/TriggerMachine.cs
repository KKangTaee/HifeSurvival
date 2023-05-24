using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(BoxCollider2D))]
public class TriggerMachine : MonoBehaviour
{
    private event EventHandler<Collider2D> _enterEvents;
    private event EventHandler<Collider2D> _stayEvents;
    private event EventHandler<Collider2D> _exitEvents;

    private BoxCollider2D _collider;

    //----------------
    // unity events
    //----------------

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        _enterEvents?.Invoke(this, collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        _stayEvents?.Invoke(this, collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _exitEvents?.Invoke(this, collision);
    }


    //------------------
    // functions
    //------------------


    public void AddTriggerEnter(EventHandler<Collider2D> inCallback) =>
        _enterEvents = inCallback;

    public void AddTriggerStay(EventHandler<Collider2D> inCallback) =>
        _stayEvents = inCallback;

    public void AddTriggerExit(EventHandler<Collider2D> inCallback) =>
        _exitEvents = inCallback;

    public void SetBoxSize(Vector2 inSize) =>
        _collider.size = inSize;

}
