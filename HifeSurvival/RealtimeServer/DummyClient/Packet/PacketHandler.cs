using DummyClient;
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

	
		Console.WriteLine($"player id : {chatPacket.playerId} chat : {chatPacket.chat}");
	}

    internal static void S_ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_ReadyToMatchHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
	
}
