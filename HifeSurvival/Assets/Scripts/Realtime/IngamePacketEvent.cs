using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class IngamePacketEvent : PacketEventBase,
    IUpdateDeadBroadcast, IUpdateAttackBroadcast, IUpdateRewardBroadcast, IResponseIncreaseStat, IResponsePickReward,
    IUpdateLocationBroadcast, IUpdateInvenItemSingle, IUpdatePlayerCurrencySingle
{
    public IngamePacketEvent(GameMode gameMode) : base(gameMode)
    {
        _onEventHanderServerDict = new Dictionary<Type, Delegate>()
        {
            { typeof(IncreaseStatResponse),     (Action<IncreaseStatResponse>)OnResponseIncreaseStat },
            
            { typeof(PickRewardResponse),       (Action<PickRewardResponse>)OnResponsePickReward },

            { typeof(S_Dead),                   (Action<S_Dead>)OnUpdateDeadBroadcast },

            { typeof(CS_Attack),                (Action<CS_Attack>)OnUpdateAttackBroadcast },

            { typeof(UpdateInvenItem),          (Action<UpdateInvenItem>)OnUpdateInvenItemSingle },

            { typeof(UpdateLocationBroadcast),  (Action<UpdateLocationBroadcast>)OnUpdateLocationBroadcast },

            { typeof(UpdatePlayerCurrency),     (Action<UpdatePlayerCurrency>)OnUpdatePlayerCurrencySingle },

            { typeof(UpdateRewardBroadcast),    (Action<UpdateRewardBroadcast>)OnUpdateRewardBroadcast },
        };
    }

    public void OnResponseIncreaseStat(IncreaseStatResponse packet)
    {
       var player = _gameMode.GetPlayerEntity(packet.id);

        if (player == null)
            return;

        player.stat.IncreaseStat((EStatType)packet.type, packet.increase);
        // OnRecvIncreasStatHandler?.Invoke(packet);

        NotifyClient(packet);
    }

    public void OnResponsePickReward(PickRewardResponse packet)
    {
        // OnRecvPickRewardHandler?.Invoke(packet);
        NotifyClient(packet);
    }

    public void OnUpdateAttackBroadcast(CS_Attack packet)
    {
        Entity toEntity = null;

        switch (Entity.GetEntityType(packet.targetId))
        {
            case Entity.EEntityType.PLAYER:
                toEntity = _gameMode.GetPlayerEntity(packet.targetId);
                break;

            case Entity.EEntityType.MOSNTER:
                toEntity = _gameMode.GetMonsterEntity(packet.targetId);
                break;
        }

        // 공격
        toEntity.stat.AddCurrHp(-packet.attackValue);

        if (_gameMode.IsSelf(packet.id) == false)
        {
            // OnRecvAttackHandler?.Invoke(inPacket);
            NotifyClient(packet);
        }
    }

    public void OnUpdateDeadBroadcast(S_Dead packet)
    {
        NotifyClient(packet);
    }

    public void OnUpdateInvenItemSingle(UpdateInvenItem packet)
    {
        var invenItem = new EntityItem(packet.invenItem);
        _gameMode.EntitySelf.itemSlot[packet.invenItem.slot] = invenItem;

        NotifyClient(packet);
    }

    public void OnUpdateLocationBroadcast(UpdateLocationBroadcast packet)
    {
        Entity entity = null;
        switch (Entity.GetEntityType(packet.id))
        {
            case Entity.EEntityType.PLAYER:
                entity = _gameMode.GetPlayerEntity(packet.id);
                break;

            case Entity.EEntityType.MOSNTER:
                entity = _gameMode.GetMonsterEntity(packet.id);
                break;
        }

        entity.pos = packet.currentPos;
    }

    public void OnUpdatePlayerCurrencySingle(UpdatePlayerCurrency packet)
    {
        foreach (var currency in packet.currencyList)
        {
            switch ((ECurrency)currency.currencyType)
            {
                case ECurrency.GOLD:
                    _gameMode.EntitySelf.SetGold(currency.count);
                    break;
            }
        }

        NotifyClient(packet);
    }

    public void OnUpdateRewardBroadcast(UpdateRewardBroadcast packet)
    {
        // 아이템 드랍 및 월드맵 오브젝트 제거
        NotifyClient(packet);
    }
}


public abstract class PacketEventBase
{
    protected GameMode _gameMode;

    protected Dictionary<Type, Delegate> _onEventHanderServerDict;
    protected Dictionary<Type, Delegate> _onEventHandlerClientDict = new Dictionary<Type, Delegate>();

    public PacketEventBase(GameMode gameMode)
    {
        _gameMode = gameMode;
    }

    public void NotifyServer<T>(T packet) where T : IPacket
    {
        Type packetType = packet.GetType();

        if (_onEventHanderServerDict.TryGetValue(packetType, out var eventHandler))
        {
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

    // 이벤트 핸들러를 제거하는 메서드
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