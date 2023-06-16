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
		_makeFunc.Add((ushort)PacketID.S_JoinToGame, MakePacket<S_JoinToGame>);
		_handler.Add((ushort)PacketID.S_JoinToGame, PacketHandler.S_JoinToGameHandler);
		_makeFunc.Add((ushort)PacketID.S_LeaveToGame, MakePacket<S_LeaveToGame>);
		_handler.Add((ushort)PacketID.S_LeaveToGame, PacketHandler.S_LeaveToGameHandler);
		_makeFunc.Add((ushort)PacketID.CS_SelectHero, MakePacket<CS_SelectHero>);
		_handler.Add((ushort)PacketID.CS_SelectHero, PacketHandler.CS_SelectHeroHandler);
		_makeFunc.Add((ushort)PacketID.CS_ReadyToGame, MakePacket<CS_ReadyToGame>);
		_handler.Add((ushort)PacketID.CS_ReadyToGame, PacketHandler.CS_ReadyToGameHandler);
		_makeFunc.Add((ushort)PacketID.S_Countdown, MakePacket<S_Countdown>);
		_handler.Add((ushort)PacketID.S_Countdown, PacketHandler.S_CountdownHandler);
		_makeFunc.Add((ushort)PacketID.S_StartGame, MakePacket<S_StartGame>);
		_handler.Add((ushort)PacketID.S_StartGame, PacketHandler.S_StartGameHandler);
		// _makeFunc.Add((ushort)PacketID.S_SpawnMonster, MakePacket<S_SpawnMonster>);
		// _handler.Add((ushort)PacketID.S_SpawnMonster, PacketHandler.S_SpawnMonsterHandler);
		_makeFunc.Add((ushort)PacketID.CS_Attack, MakePacket<CS_Attack>);
		_handler.Add((ushort)PacketID.CS_Attack, PacketHandler.CS_AttackHandler);
		_makeFunc.Add((ushort)PacketID.CS_Move, MakePacket<CS_Move>);
		_handler.Add((ushort)PacketID.CS_Move, PacketHandler.CS_MoveHandler);
		_makeFunc.Add((ushort)PacketID.CS_StopMove, MakePacket<CS_StopMove>);
		_handler.Add((ushort)PacketID.CS_StopMove, PacketHandler.CS_StopMoveHandler);
		_makeFunc.Add((ushort)PacketID.S_Dead, MakePacket<S_Dead>);
		_handler.Add((ushort)PacketID.S_Dead, PacketHandler.S_DeadHandler);
		_makeFunc.Add((ushort)PacketID.S_Respawn, MakePacket<S_Respawn>);
		_handler.Add((ushort)PacketID.S_Respawn, PacketHandler.S_RespawnHandler);
		_makeFunc.Add((ushort)PacketID.CS_UpdateStat, MakePacket<CS_UpdateStat>);
		_handler.Add((ushort)PacketID.CS_UpdateStat, PacketHandler.CS_UpdateStatHandler);
		_makeFunc.Add((ushort)PacketID.S_DropReward, MakePacket<S_DropReward>);
		_handler.Add((ushort)PacketID.S_DropReward, PacketHandler.S_DropRewardHandler);
		_makeFunc.Add((ushort)PacketID.S_GetItem, MakePacket<S_GetItem>);
		_handler.Add((ushort)PacketID.S_GetItem, PacketHandler.S_GetItemHandler);
		_makeFunc.Add((ushort)PacketID.S_GetGold, MakePacket<S_GetGold>);
		_handler.Add((ushort)PacketID.S_GetGold, PacketHandler.S_GetGoldHandler);

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