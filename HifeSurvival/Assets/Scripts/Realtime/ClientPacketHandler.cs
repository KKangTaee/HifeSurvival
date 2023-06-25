﻿using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

public class ClientPacketHandler : PacketHandler
{
    public override void S_CountdownHandler(PacketSession session, IPacket packet)
    {
        S_Countdown countdown = packet as S_Countdown;
        GameMode.Instance.OnRecvCountdown(countdown);
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

    public override void CS_UpdateStatHandler(PacketSession session, IPacket packet)
    {
        CS_UpdateStat updateStat = packet as CS_UpdateStat;
        GameMode.Instance.OnRecvUpdateStat(updateStat);
    }

    public override void C_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_SpawnMonsterHandler(PacketSession session, IPacket packet)
    {
        // throw new NotImplementedException();
    }

    public override void S_DropRewardHandler(PacketSession session, IPacket packet)
    {
        // throw new NotImplementedException();
        S_DropReward dropReward = packet as S_DropReward;
        GameMode.Instance.OnRecvDropReward(dropReward);
    
    }

    public override void C_PickRewardHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_GetItemHandler(PacketSession session, IPacket packet)
    {
        // throw new NotImplementedException();
        S_GetItem item = packet as S_GetItem;
        GameMode.Instance.OnRecvGetItem(item);
    }

    public override void S_GetGoldHandler(PacketSession session, IPacket packet)
    {
        // hrow new NotImplementedException();
        S_GetGold gold = packet as S_GetGold;
        GameMode.Instance.OnRecvGetGold(gold);
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
}
