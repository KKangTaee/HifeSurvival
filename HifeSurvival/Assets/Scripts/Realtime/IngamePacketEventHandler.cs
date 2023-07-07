using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class IngamePacketEventHandler : PacketEventHandlerBase,
    IUpdateDeadBroadcast, IUpdateAttackBroadcast, IUpdateRewardBroadcast, IResponseIncreaseStat, IResponsePickReward,
    IUpdateLocationBroadcast, IUpdateInvenItemSingle, IUpdatePlayerCurrencySingle, IUpdateRespawn,IUpdateStatBroadcast
{
    public IngamePacketEventHandler(GameMode gameMode) : base(gameMode)
    {
        _onEventHanderGameModeDict = new Dictionary<Type, Delegate>()
        {
            { typeof(IncreaseStatResponse),     (Action<IncreaseStatResponse>)OnResponseIncreaseStat },
            
            { typeof(PickRewardResponse),       (Action<PickRewardResponse>)OnResponsePickReward },

            { typeof(S_Dead),                   (Action<S_Dead>)OnUpdateDeadBroadcast },

            { typeof(CS_Attack),                (Action<CS_Attack>)OnUpdateAttackBroadcast },

            { typeof(UpdateInvenItem),          (Action<UpdateInvenItem>)OnUpdateInvenItemSingle },

            { typeof(UpdateLocationBroadcast),  (Action<UpdateLocationBroadcast>)OnUpdateLocationBroadcast },

            { typeof(UpdatePlayerCurrency),     (Action<UpdatePlayerCurrency>)OnUpdatePlayerCurrencySingle },

            { typeof(UpdateRewardBroadcast),    (Action<UpdateRewardBroadcast>)OnUpdateRewardBroadcast },

            { typeof(UpdateStatBroadcast),      (Action<UpdateStatBroadcast>)OnUpdateStatBroadcast },
        };
    }

    public void OnResponseIncreaseStat(IncreaseStatResponse packet)
    {
       var player = _gameMode.GetPlayerEntity(packet.id);

        if (player == null)
            return;

        // player.stat.IncreaseStat((EStatType)packet.type, packet.increase);
        NotifyClient(packet);
    }

    public void OnResponsePickReward(PickRewardResponse packet)
    {
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
            NotifyClient(packet);
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

    public void OnUpdateRespawnBroadcast(S_Respawn packet)
    {
        switch(Entity.GetEntityType(packet.id))
        {
            case Entity.EEntityType.PLAYER:
              
            var player = _gameMode.GetPlayerEntity(packet.id);

            if (player == null)
                return;

            player.pos = packet.pos;
                break;
            
            case Entity.EEntityType.MOSNTER:
                break;
        }

        NotifyClient(packet);
    }

    public void OnUpdateRewardBroadcast(UpdateRewardBroadcast packet)
    {
        // 아이템 드랍 및 월드맵 오브젝트 제거
        NotifyClient(packet);
    }

    public void OnUpdateStatBroadcast(UpdateStatBroadcast packet)
    {
        Entity entity = null;
        switch(Entity.GetEntityType(packet.id))
        {
            case Entity.EEntityType.PLAYER:
                entity = _gameMode.GetPlayerEntity(packet.id);
                break;
            
            case Entity.EEntityType.MOSNTER:
                entity = _gameMode.GetMonsterEntity(packet.id);
                break;
        }

        //TODO : @Yodle_94 :  Spawn 대응 되기 전까지는 필히 발생되므로, 예외처리 
        if (entity == null)
        {
            return;
        }
        //TODO end

        if (entity.stat == null)
        {
           entity.stat = new EntityStat(packet.originStat);
        }
        else
        {
            entity.stat.UpdateOriginStat(packet.originStat);
            entity.stat.UpdateAddStat(packet.addStat);
        }

        NotifyClient(packet);
    }
}

