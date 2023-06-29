using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

public class ClientPacketHandler : PacketHandler
{
    
    [Obsolete]
    public override void S_CountdownHandler(PacketSession session, IPacket packet)
    {
        S_Countdown countdown = packet as S_Countdown;
        // GameMode.Instance.OnRecvCountdown(countdown);
    }

    public override void S_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        S_JoinToGame joinToGame = packet as S_JoinToGame;
        GameMode.Instance.OnRecvJoin(joinToGame);
    }

    public override void S_LeaveToGameHandler(PacketSession session, IPacket packet)
    {
        S_LeaveToGame leaveToGame = packet as S_LeaveToGame;
        GameMode.Instance.OnRecvLeave(leaveToGame);
    }

    public override void S_StartGameHandler(PacketSession session, IPacket packet)
    {
        S_StartGame startGame = packet as S_StartGame;
        GameMode.Instance.OnRecvStartGame(startGame);
    }

    public override void CS_SelectHeroHandler(PacketSession session, IPacket packet)
    {
        CS_SelectHero selectHero = packet as CS_SelectHero;
        GameMode.Instance.OnRecvSelectHero(selectHero);
    }

    public override void CS_ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
        GameMode.Instance.OnRecvReadyToGame(readyToGame);
    }

    public override void CS_AttackHandler(PacketSession session, IPacket packet)
    {
        CS_Attack attack = packet as CS_Attack;
        GameMode.Instance.OnRecvAttack(attack);
    }

    public override void S_DeadHandler(PacketSession session, IPacket packet)
    {
        S_Dead dead = packet as S_Dead;
        GameMode.Instance.OnRecvDead(dead);
    }

    public override void S_RespawnHandler(PacketSession session, IPacket packet)
    {
        S_Respawn respawn = packet as S_Respawn;
        GameMode.Instance.OnRecvRespawn(respawn);
    }

    public override void C_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_SpawnMonsterHandler(PacketSession session, IPacket packet)
    {

    }

    public override void MoveRequestHandler(PacketSession session, IPacket packet)
    {
        //TODO 좌표 기준 이동 리뉴얼
    }

    public override void UpdateLocationBroadcastHandler(PacketSession session, IPacket packet)
    {
        //TODO 좌표 기준 이동 리뉴얼
        UpdateLocationBroadcast locationBroadcast = packet as UpdateLocationBroadcast;
        GameMode.Instance.OnUpdateLocation(locationBroadcast);
    }

    public override void MoveResponseHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void IncreaseStatRequestHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void IncreaseStatResponseHandler(PacketSession session, IPacket packet)
    {
        IncreaseStatResponse increaseStat = packet as IncreaseStatResponse;
        GameMode.Instance.OnRecvIncreasStat(increaseStat);
    }

    public override void UpdateStatBroadcastHandler(PacketSession session, IPacket packet)
    {
        //TODO: Stat 업데이트 해야함. 
    }

    public override void PickRewardRequestHandler(PacketSession session, IPacket packet)
    {
        
    }

    public override void PickRewardResponseHandler(PacketSession session, IPacket packet)
    {
        PickRewardResponse pickReward = packet as PickRewardResponse;
        GameMode.Instance.OnRecvGetItem(pickReward);
    }

    public override void UpdateRewardBroadcastHandler(PacketSession session, IPacket packet)
    {
        UpdateRewardBroadcast updateReward = packet as UpdateRewardBroadcast;
    }

    public override void UpdatePlayerCurrencyHandler(PacketSession session, IPacket packet)
    {

    }

    public override void PlayStartRequestHandler(PacketSession session, IPacket packet)
    {

    }

    public override void PlayStartResponseHandler(PacketSession session, IPacket packet)
    {
        
    }

    public override void UpdateGameModeStatusBroadcastHandler(PacketSession session, IPacket packet)
    {
        UpdateGameModeStatusBroadcast gameModeStatus = packet as UpdateGameModeStatusBroadcast;
        GameMode.Instance.OnUpdateGameModeStatus(gameModeStatus);
    }
}
