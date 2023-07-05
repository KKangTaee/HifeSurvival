using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

public class ClientPacketHandler : PacketHandler
{
    
    [Obsolete]
    public override void S_CountdownHandler(Session session, IPacket packet)
    {
        S_Countdown countdown = packet as S_Countdown;
    }

    public override void S_JoinToGameHandler(Session session, IPacket packet)
    {
        S_JoinToGame joinToGame = packet as S_JoinToGame;
        GameMode.Instance.OnRecvJoin(joinToGame);
    }

    public override void S_LeaveToGameHandler(Session session, IPacket packet)
    {
        S_LeaveToGame leaveToGame = packet as S_LeaveToGame;
        GameMode.Instance.OnRecvLeave(leaveToGame);
    }

    public override void S_StartGameHandler(Session session, IPacket packet)
    {
        S_StartGame startGame = packet as S_StartGame;
        GameMode.Instance.GetEventHandler<GameReadyPacketEventHandler>().NotifyServer(startGame);
    }

    public override void CS_SelectHeroHandler(Session session, IPacket packet)
    {
        CS_SelectHero selectHero = packet as CS_SelectHero;
        GameMode.Instance.GetEventHandler<GameReadyPacketEventHandler>().NotifyServer(selectHero);
    }

    public override void CS_ReadyToGameHandler(Session session, IPacket packet)
    {
        CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
         GameMode.Instance.GetEventHandler<GameReadyPacketEventHandler>().NotifyServer(readyToGame);
    }

    public override void CS_AttackHandler(Session session, IPacket packet)
    {
        CS_Attack attack = packet as CS_Attack;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(attack);
    }

    public override void S_DeadHandler(Session session, IPacket packet)
    {
        S_Dead dead = packet as S_Dead;
         GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(dead);
    }

    public override void S_RespawnHandler(Session session, IPacket packet)
    {
        S_Respawn respawn = packet as S_Respawn;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(respawn);
    }

    public override void C_JoinToGameHandler(Session session, IPacket packet)
    {

    }

    public override void S_SpawnMonsterHandler(Session session, IPacket packet)
    {

    }

    public override void MoveRequestHandler(Session session, IPacket packet)
    {
    }

    public override void UpdateLocationBroadcastHandler(Session session, IPacket packet)
    {
        UpdateLocationBroadcast locationBroadcast = packet as UpdateLocationBroadcast;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(locationBroadcast);
    }

    public override void MoveResponseHandler(Session session, IPacket packet)
    {

    }

    public override void IncreaseStatRequestHandler(Session session, IPacket packet)
    {

    }

    public override void IncreaseStatResponseHandler(Session session, IPacket packet)
    {
        IncreaseStatResponse increaseStat = packet as IncreaseStatResponse;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(increaseStat);
    }

    public override void UpdateStatBroadcastHandler(Session session, IPacket packet)
    {
        UpdateStatBroadcast updateStat = packet as UpdateStatBroadcast;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(updateStat);
    }

    public override void PickRewardRequestHandler(Session session, IPacket packet)
    {

    }

    public override void PickRewardResponseHandler(Session session, IPacket packet)
    {
        PickRewardResponse pickReward = packet as PickRewardResponse;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(pickReward);
    }

    public override void UpdateRewardBroadcastHandler(Session session, IPacket packet)
    {
        UpdateRewardBroadcast updateReward = packet as UpdateRewardBroadcast;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(updateReward);
    }

    public override void PlayStartRequestHandler(Session session, IPacket packet)
    {

    }

    public override void PlayStartResponseHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdateGameModeStatusBroadcastHandler(Session session, IPacket packet)
    {
        UpdateGameModeStatusBroadcast gameModeStatus = packet as UpdateGameModeStatusBroadcast;
        GameMode.Instance.GetEventHandler<GameReadyPacketEventHandler>().NotifyServer(gameModeStatus);
    }

    public override void UpdateInvenItemHandler(Session session, IPacket packet)
    {
        UpdateInvenItem invenItem = packet as UpdateInvenItem;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(invenItem);
    }
    
    public override void UpdatePlayerCurrencyHandler(Session session, IPacket packet)
    {
        UpdatePlayerCurrency playerCurrency = packet as UpdatePlayerCurrency;
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().NotifyServer(playerCurrency);
    }
}
