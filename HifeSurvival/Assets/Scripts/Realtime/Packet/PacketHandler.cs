using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void S_CountdownHandler(PacketSession session, IPacket packet)
    {
        S_Countdown countdown = packet as S_Countdown;
        GameMode.Instance.OnRecvCountdown(countdown);
    }

    public static void S_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        S_JoinToGame joinToGame = packet as S_JoinToGame;
        GameMode.Instance.OnRecvJoin(joinToGame);
    }

    public static void S_LeaveToGameHandler(PacketSession session, IPacket packet)
    {
        S_LeaveToGame leaveToGame = packet as S_LeaveToGame;
        GameMode.Instance.OnRecvLeave(leaveToGame);
    }

    public static void S_StartGameHandler(PacketSession session, IPacket packet)
    {
        S_StartGame startGame = packet as S_StartGame;
        GameMode.Instance.OnRecvStartGame(startGame);
    }

    public static void CS_SelectHeroHandler(PacketSession session, IPacket packet)
    {
        CS_SelectHero selectHero = packet as CS_SelectHero;
        GameMode.Instance.OnRecvSelectHero(selectHero);
    }

    public static void CS_ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
        GameMode.Instance.OnRecvReadyToGame(readyToGame);
    }

    public static void CS_AttackHandler(PacketSession session, IPacket packet)
    {
        CS_Attack attack = packet as CS_Attack;
        GameMode.Instance.OnRecvAttack(attack);
    }

    public static void CS_MoveHandler(PacketSession session, IPacket packet)
    {
        CS_Move move = packet as CS_Move;
        GameMode.Instance.OnRecvMove(move);
    }

    public static void CS_StopMoveHandler(PacketSession session, IPacket packet)
    {
         CS_StopMove stopMove = packet as CS_StopMove;
         GameMode.Instance.OnRecvStopMove(stopMove);
    }

    public static void S_DeadHandler(PacketSession session, IPacket packet)
    {
        S_Dead dead = packet as S_Dead;
        GameMode.Instance.OnRecvDead(dead);
    }

    public static void S_RespawnHandler(PacketSession session, IPacket packet)
    {
        S_Respawn respawn = packet as S_Respawn;
        GameMode.Instance.OnRecvRespawn(respawn);
    }

    public static void CS_UpdateStatHandler(PacketSession session, IPacket packet)
    {
        CS_UpdateStat updateStat = packet as CS_UpdateStat;
        GameMode.Instance.OnRecvUpdateStat(updateStat);
    }


    public static void S_DropRewardHandler(PacketSession session, IPacket packet)
    {
        S_DropReward dropItem = packet as S_DropReward;
        GameMode.Instance.OnRecvDropItem(dropItem);
    }

    public static void S_GetItemHandler(PacketSession session, IPacket packet)
    {
        S_GetItem getItem = packet as S_GetItem;
        GameMode.Instance.OnRecvGetItem(getItem);
    }

    public static void S_GetGoldHandler(PacketSession session, IPacket packet)
    {
        S_GetGold getGold = packet as S_GetGold;
        GameMode.Instance.OnRecvGetGold(getGold);
    }
}
