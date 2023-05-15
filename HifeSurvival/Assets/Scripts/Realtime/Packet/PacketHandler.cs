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

    internal static void S_CountdownToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_ReslutSelectHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_ResultToMatchHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
}
