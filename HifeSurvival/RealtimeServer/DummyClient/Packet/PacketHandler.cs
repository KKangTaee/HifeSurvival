using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

class PacketHandler
{
	public static void S_ChatHandler(PacketSession session, IPacket packet)
	{
		S_Chat chatPacket = packet as S_Chat;
		ServerSession serverSession = session as ServerSession;
	}

    public static void S_CountdownToGameHandler(PacketSession session, IPacket packet)
    {
		if(packet is S_CountdownToGame p)
		{
			System.Console.WriteLine($"게임시작전 : {p.countdownSec}전");
			
			var serverSession = session as ServerSession;
			serverSession.quickMatch.SetStatus(ServerSession.QuickMatch.EStatus.COUNTDOWN_GAME);
		}
    }

    public static void S_JoinToGameHandler(PacketSession session, IPacket packet)
    {
		if(packet is S_JoinToGame p)
		{
			var serverSession = session as ServerSession;
			serverSession.quickMatch.SetStatus(ServerSession.QuickMatch.EStatus.JOIN_TO_GAME);
		}
	}
        
    public static void S_ReslutSelectHandler(PacketSession session, IPacket packet)
    {
		if(packet is S_ReslutSelect p)
		{
			System.Console.WriteLine($"선택된 캐릭터 : {string.Join(',', p.reslutSelects.Select(x=>x.heroType))}");
		}
    }

    public static void S_ResultToMatchHandler(PacketSession session, IPacket packet)
    {
		if(packet is S_ResultToMatch p)
		{
			var serverSession = session as ServerSession;
			var quickMatch = serverSession.quickMatch;

			quickMatch.SetStatus(ServerSession.QuickMatch.EStatus.SELECT_TO_HERO);
			quickMatch.SetPlayerId(p.playerId);
			quickMatch.SetChannelId(p.channelId);
		}
    }
}
