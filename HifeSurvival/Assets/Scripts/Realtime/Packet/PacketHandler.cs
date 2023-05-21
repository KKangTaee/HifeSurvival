﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void S_ChatHandler(PacketSession session, IPacket packet)
	{
		S_Chat chatPacket = packet as S_Chat;
		ServerSession serverSession = session as ServerSession;

		//if (chatPacket.playerId == 1)
			//Console.WriteLine(chatPacket.chat);
	}

    public static void ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        ReadyToGame readyToGame = packet as ReadyToGame;
        GameMode.Instance.OnRecvReadyToGame(readyToGame);
    }

    public static void SelectHeroHandler(PacketSession session, IPacket packet)
    {
        SelectHero selectHero = packet as SelectHero;
        GameMode.Instance.OnRecvSelectHero(selectHero);
    }

    internal static void S_CountdownHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_CountdownToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public static void S_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        S_JoinToGame joinToGame = packet as S_JoinToGame;
        GameMode.Instance.OnRecvJoin(joinToGame);
    }

    internal static void S_LeaveToGameHandler(PacketSession session, IPacket packet)
    {
        S_LeaveToGame leaveToGame = packet as S_LeaveToGame;
        GameMode.Instance.OnRecvLeave(leaveToGame);
    }

    internal static void S_ReslutSelectHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_ResultToMatchHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_StartGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_AddJoinHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
}
