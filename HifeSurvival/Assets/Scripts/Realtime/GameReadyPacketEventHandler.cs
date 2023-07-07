using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameReadyPacketEventHandler : PacketEventHandlerBase,
    IUpdateSelectHero, IUpdateReadyToGame, IUpdateStartGame, IUpdateGameModeStatusBroadcast
{
    public GameReadyPacketEventHandler(GameMode gameMode) : base(gameMode)
    {
        _onEventHanderGameModeDict = new Dictionary<System.Type, System.Delegate>()
        {
            {typeof(CS_SelectHero),     (Action<CS_SelectHero>)OnUpdateSelectHeroBroadcast},

            {typeof(CS_ReadyToGame),    (Action<CS_ReadyToGame>)UpdateReadyToGameBroadcast},

            {typeof(S_StartGame),       (Action<S_StartGame>)UpdateStartGameBroadcast},

            {typeof(UpdateGameModeStatusBroadcast),   (Action<UpdateGameModeStatusBroadcast>)OnUpdateGameModeStatusBroadcast},
        };
    }


    // TODO@taeho.kang 임시처리

    public void OnUpdateGameModeStatusBroadcast(UpdateGameModeStatusBroadcast packet)
    {
        var status  = (EGameModeStatus)packet.status;
        _gameMode.SetStatus(status);

        NotifyClient(packet);
    }

    public void OnUpdateSelectHeroBroadcast(CS_SelectHero packet)
    {
        var player = _gameMode.GetPlayerEntity(packet.id);

        if (player == null)
            return;

        player.heroId = packet.heroKey;

        if (_gameMode.IsSelf(packet.id) == false)
            NotifyClient(packet);
    }

    public void UpdateReadyToGameBroadcast(CS_ReadyToGame packet)
    {
        var player = _gameMode.GetPlayerEntity(packet.id);

        if (player == null)
            return;

        player.isReady = true;

        if (_gameMode.IsSelf(packet.id) == false)
            NotifyClient(packet);
    }

    public void UpdateStartGameBroadcast(S_StartGame packet)
    {
        var playerList  = packet.playerList;
        var monsterList = packet.monsterList;

        foreach (PlayerSpawn p in playerList)
        {
            var playerEntity = _gameMode.GetPlayerEntity(p.id);
            playerEntity.heroId = p.herosKey;
            playerEntity.pos = p.pos;
        }

        foreach (MonsterSpawn m in monsterList)
        {
            _gameMode.CreateMonsterEntity(m);
        }

        // TODO@taeho.kang 이거는 OnUpdateGameModeStatusBroadcast 에서 처리가 되고, 
        // NotifyClient(packet);
    }
}
