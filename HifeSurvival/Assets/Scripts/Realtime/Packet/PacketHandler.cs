using ServerCore;
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

    internal static void CS_SelectHeroHandler(PacketSession session, IPacket packet)
    {
        CS_SelectHero selectHero = packet as CS_SelectHero;
        GameMode.Instance.OnRecvSelectHero(selectHero);
    }

    internal static void CS_ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
        GameMode.Instance.OnRecvReadyToGame(readyToGame);
    }

    public static void CS_AttackHandler(PacketSession session, IPacket packet)
    {
        
    }

    internal static void CS_MoveHandler(PacketSession session, IPacket packet)
    {
        
    }

    internal static void CS_StopMoveHandler(PacketSession session, IPacket packet)
    {
        
    }
}
