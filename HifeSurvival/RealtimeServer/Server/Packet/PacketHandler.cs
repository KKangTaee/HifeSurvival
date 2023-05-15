using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_ChatHandler(PacketSession session, IPacket packet)
	{
		C_Chat chatPacket = packet as C_Chat;
		ClientSession clientSession = session as ClientSession;

		if (clientSession.Room == null)
			return;

		GameRoom room = clientSession.Room;
		room.Push(
			() => room.Broadcast(clientSession, chatPacket.chat)
		);
	}

    internal static void C_CountdownToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void C_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void C_ReslutSelectHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void C_ResultToMatchHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
}
