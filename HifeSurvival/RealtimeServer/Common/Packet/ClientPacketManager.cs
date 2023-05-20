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
		_makeFunc.Add((ushort)PacketID.S_Chat, MakePacket<S_Chat>);
		_handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);
		_makeFunc.Add((ushort)PacketID.S_JoinToGame, MakePacket<S_JoinToGame>);
		_handler.Add((ushort)PacketID.S_JoinToGame, PacketHandler.S_JoinToGameHandler);
		_makeFunc.Add((ushort)PacketID.S_LeaveToGame, MakePacket<S_LeaveToGame>);
		_handler.Add((ushort)PacketID.S_LeaveToGame, PacketHandler.S_LeaveToGameHandler);
		_makeFunc.Add((ushort)PacketID.SelectHero, MakePacket<SelectHero>);
		_handler.Add((ushort)PacketID.SelectHero, PacketHandler.SelectHeroHandler);
		_makeFunc.Add((ushort)PacketID.ReadyToGame, MakePacket<ReadyToGame>);
		_handler.Add((ushort)PacketID.ReadyToGame, PacketHandler.ReadyToGameHandler);
		_makeFunc.Add((ushort)PacketID.S_Countdown, MakePacket<S_Countdown>);
		_handler.Add((ushort)PacketID.S_Countdown, PacketHandler.S_CountdownHandler);
		_makeFunc.Add((ushort)PacketID.S_StartGame, MakePacket<S_StartGame>);
		_handler.Add((ushort)PacketID.S_StartGame, PacketHandler.S_StartGameHandler);
		_makeFunc.Add((ushort)PacketID.S_JoinOther, MakePacket<S_JoinOther>);
		_handler.Add((ushort)PacketID.S_JoinOther, PacketHandler.S_JoinOtherHandler);
		_makeFunc.Add((ushort)PacketID.S_LeaveOther, MakePacket<S_LeaveOther>);
		_handler.Add((ushort)PacketID.S_LeaveOther, PacketHandler.S_LeaveOtherHandler);

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