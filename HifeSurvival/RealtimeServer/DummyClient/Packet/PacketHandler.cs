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

    internal static void S_StartGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_CountdownHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void SelectHeroHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_LeaveToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_JoinOtherHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    internal static void S_LeaveOtherHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
}
