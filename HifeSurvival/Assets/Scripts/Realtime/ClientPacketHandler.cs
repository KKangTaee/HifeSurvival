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
        // GameMode.Instance.OnRecvCountdown(countdown);
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
        GameMode.Instance.OnRecvStartGame(startGame);
    }

    public override void CS_SelectHeroHandler(Session session, IPacket packet)
    {
        CS_SelectHero selectHero = packet as CS_SelectHero;
        GameMode.Instance.OnRecvSelectHero(selectHero);
    }

    public override void CS_ReadyToGameHandler(Session session, IPacket packet)
    {
        CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
        GameMode.Instance.OnRecvReadyToGame(readyToGame);
    }

    public override void CS_AttackHandler(Session session, IPacket packet)
    {
        CS_Attack attack = packet as CS_Attack;
        // GameMode.Instance.OnRecvAttack(attack);
        GameMode.Instance.GetEventHandler<IngamePacketEvent>().NotifyServer(attack);
    }

    public override void S_DeadHandler(Session session, IPacket packet)
    {
        S_Dead dead = packet as S_Dead;
        // GameMode.Instance.OnRecvDead(dead);
         GameMode.Instance.GetEventHandler<IngamePacketEvent>().NotifyServer(dead);
    }

    public override void S_RespawnHandler(Session session, IPacket packet)
    {
        S_Respawn respawn = packet as S_Respawn;
        GameMode.Instance.OnRecvRespawn(respawn);
    }

    public override void C_JoinToGameHandler(Session session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_SpawnMonsterHandler(Session session, IPacket packet)
    {

    }

    public override void MoveRequestHandler(Session session, IPacket packet)
    {
        //TODO 좌표 기준 이동 리뉴얼
    }

    public override void UpdateLocationBroadcastHandler(Session session, IPacket packet)
    {
        //TODO 좌표 기준 이동 리뉴얼
        UpdateLocationBroadcast locationBroadcast = packet as UpdateLocationBroadcast;
        GameMode.Instance.GetEventHandler<IngamePacketEvent>().NotifyServer(locationBroadcast);
        // GameMode.Instance.OnUpdateLocationBroadcast(locationBroadcast);
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
        GameMode.Instance.OnRecvIncreasStat(increaseStat);
    }

    public override void UpdateStatBroadcastHandler(Session session, IPacket packet)
    {
        UpdateStatBroadcast updateStat = packet as UpdateStatBroadcast;
        GameMode.Instance.OnUpdateStatBroadcast(updateStat);
    }

    public override void PickRewardRequestHandler(Session session, IPacket packet)
    {
    }

    public override void PickRewardResponseHandler(Session session, IPacket packet)
    {
        PickRewardResponse pickReward = packet as PickRewardResponse;
        GameMode.Instance.OnRecvPickReward(pickReward);
    }

    public override void UpdateRewardBroadcastHandler(Session session, IPacket packet)
    {
        UpdateRewardBroadcast updateReward = packet as UpdateRewardBroadcast;
        // GameMode.Instance.OnUpdateRewardBroadcast(updateReward);
        GameMode.Instance.GetEventHandler<IngamePacketEvent>().NotifyServer(updateReward);
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
        GameMode.Instance.OnUpdateGameModeStatusBroadcast(gameModeStatus);
    }

    public override void UpdateInvenItemHandler(Session session, IPacket packet)
    {
        UpdateInvenItem invenItem = packet as UpdateInvenItem;
        // GameMode.Instance.OnUpdateInvenItemSingle(invenItem);
        GameMode.Instance.GetEventHandler<IngamePacketEvent>().NotifyServer(invenItem);
    }

    public override void UpdatePlayerCurrencyHandler(Session session, IPacket packet)
    {
        UpdatePlayerCurrency playerCurrency = packet as UpdatePlayerCurrency;
        // GameMode.Instance.OnUpdatePlayerCurrencySingle(playerCurrency);
        GameMode.Instance.GetEventHandler<IngamePacketEvent>().NotifyServer(playerCurrency);
    }
}
