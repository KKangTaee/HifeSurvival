using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class PacketEventHandlerBase
{
    protected GameMode _gameMode;

    protected Dictionary<Type, Delegate> _onEventHanderGameModeDict;
    protected Dictionary<Type, Delegate> _onEventHandlerClientDict = new Dictionary<Type, Delegate>();

    public PacketEventHandlerBase(GameMode gameMode)
    {
        _gameMode = gameMode;
    }

    public void NotifyGameMode<T>(T packet) where T : IPacket
    {
        Type packetType = packet.GetType();

        if (_onEventHanderGameModeDict.TryGetValue(packetType, out var eventHandler))
        {
            Debug.LogWarning($"[{nameof(NotifyGameMode)}] {packet.GetType()} is Called!");
            var typedAction = eventHandler as Action<T>;
            typedAction?.Invoke(packet);
        }
    }


    public void RegisterClient<T>(Action<T> action) where T : IPacket
    {
        Type key = typeof(T);
        if (_onEventHandlerClientDict.ContainsKey(key))
        {
            _onEventHandlerClientDict[key] = Delegate.Combine(_onEventHandlerClientDict[key], action);
        }
        else
        {
            _onEventHandlerClientDict[key] = action;
        }
    }

    public void UnregisterClient<T>(Action<T> action) where T : IPacket
    {
        Type key = typeof(T);
        if (_onEventHandlerClientDict.ContainsKey(key))
        {
            _onEventHandlerClientDict[key] = Delegate.Remove(_onEventHandlerClientDict[key], action);
        }
    }

    public void NotifyClient<T>(T packet) where T : IPacket
    {
        Type packetType = packet.GetType();

        if (_onEventHandlerClientDict.TryGetValue(packetType, out var eventHandler))
        {
            var typedAction = eventHandler as Action<T>;
            typedAction?.Invoke(packet);
        }
    }
}