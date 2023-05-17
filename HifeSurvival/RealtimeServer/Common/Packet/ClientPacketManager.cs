using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	
	Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
	Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
		
	public void Register()
	{
		_makeFunc.Add((ushort)PacketID.C_Chat, MakePacket<S_Chat>);
		_handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);
		_makeFunc.Add((ushort)PacketID.C_Chat, MakePacket<S_ResultToMatch>);
		_handler.Add((ushort)PacketID.S_ResultToMatch, PacketHandler.S_ResultToMatchHandler);
		_makeFunc.Add((ushort)PacketID.C_Chat, MakePacket<S_ReslutSelect>);
		_handler.Add((ushort)PacketID.S_ReslutSelect, PacketHandler.S_ReslutSelectHandler);
		_makeFunc.Add((ushort)PacketID.C_Chat, MakePacket<S_CountdownToGame>);
		_handler.Add((ushort)PacketID.S_CountdownToGame, PacketHandler.S_CountdownToGameHandler);
		_makeFunc.Add((ushort)PacketID.C_Chat, MakePacket<S_JoinToGame>);
		_handler.Add((ushort)PacketID.S_JoinToGame, PacketHandler.S_JoinToGameHandler);

	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		if(_makeFunc.TryGetValue(id, out var func) == true)
		{
			IPacket packet = func.Invoke(session, buffer);

			if(onRecvCallback != null)
			   onRecvCallback.Invoke(session, packet);
			else
				HandlePacket(session,packet);
		}
	}

	T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
	{
		T pkt = new T();
		pkt.Read(buffer);
		return pkt;	
	}

	public void HandlePacket(PacketSession inSession, IPacket inPacket)
	{
		Action<PacketSession, IPacket> action = null;
		if (_handler.TryGetValue(inPacket.Protocol, out action))
			action.Invoke(inSession, inPacket);
	}
}