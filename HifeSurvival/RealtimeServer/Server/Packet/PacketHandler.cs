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
        // room.Push( () => room.Broadcast(clientSession, chatPacket.chat));
	}

    public static void ReadyToGameHandler(PacketSession session, IPacket packet)
    {

    }

    public static void SelectHeroHandler(PacketSession session, IPacket packet)
    {

    }

    public static void C_JoinToGameHandler(PacketSession session, IPacket packet)
    {
		C_JoinToGame joinToGame = packet as C_JoinToGame;
		ClientSession client = session as ClientSession;
		
		client.Room?.Mode.Join(joinToGame, client.SessionId);	
    }
}
