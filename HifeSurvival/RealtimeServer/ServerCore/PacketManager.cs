using System;
using System.Collections.Generic;
using ServerCore;


public abstract class PacketHandler
{
	public abstract void C_JoinToGameHandler(PacketSession session, IPacket packet);
	public abstract void S_JoinToGameHandler(PacketSession session, IPacket packet);
	public abstract void S_LeaveToGameHandler(PacketSession session, IPacket packet);
	public abstract void CS_SelectHeroHandler(PacketSession session, IPacket packet);
	public abstract void CS_ReadyToGameHandler(PacketSession session, IPacket packet);
	public abstract void S_CountdownHandler(PacketSession session, IPacket packet);
	public abstract void S_StartGameHandler(PacketSession session, IPacket packet);
	public abstract void S_SpawnMonsterHandler(PacketSession session, IPacket packet);
	public abstract void CS_AttackHandler(PacketSession session, IPacket packet);
	public abstract void MoveRequestHandler(PacketSession session, IPacket packet);
	public abstract void S_DeadHandler(PacketSession session, IPacket packet);
	public abstract void S_RespawnHandler(PacketSession session, IPacket packet);
	public abstract void CS_UpdateStatHandler(PacketSession session, IPacket packet);
	public abstract void S_DropRewardHandler(PacketSession session, IPacket packet);
	public abstract void C_PickRewardHandler(PacketSession session, IPacket packet);
	public abstract void S_GetItemHandler(PacketSession session, IPacket packet);
	public abstract void S_GetGoldHandler(PacketSession session, IPacket packet);
	public abstract void UpdateLocationBroadcastHandler(PacketSession session, IPacket packet);

}

public class PacketManager
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
	
	public void BindHandler(PacketHandler handler)
	{
		_handler.Add((ushort)PacketID.C_JoinToGame, handler.C_JoinToGameHandler);
		_handler.Add((ushort)PacketID.S_JoinToGame, handler.S_JoinToGameHandler);
		_handler.Add((ushort)PacketID.S_LeaveToGame, handler.S_LeaveToGameHandler);
		_handler.Add((ushort)PacketID.CS_SelectHero, handler.CS_SelectHeroHandler);
		_handler.Add((ushort)PacketID.CS_ReadyToGame, handler.CS_ReadyToGameHandler);
		_handler.Add((ushort)PacketID.S_Countdown, handler.S_CountdownHandler);
		_handler.Add((ushort)PacketID.S_StartGame, handler.S_StartGameHandler);
		_handler.Add((ushort)PacketID.S_SpawnMonster, handler.S_SpawnMonsterHandler);
		_handler.Add((ushort)PacketID.CS_Attack, handler.CS_AttackHandler);
		_handler.Add((ushort)PacketID.MoveRequest, handler.MoveRequestHandler);
		_handler.Add((ushort)PacketID.S_Dead, handler.S_DeadHandler);
		_handler.Add((ushort)PacketID.S_Respawn, handler.S_RespawnHandler);
		_handler.Add((ushort)PacketID.CS_UpdateStat, handler.CS_UpdateStatHandler);
		_handler.Add((ushort)PacketID.S_DropReward, handler.S_DropRewardHandler);
		_handler.Add((ushort)PacketID.C_PickReward, handler.C_PickRewardHandler);
		_handler.Add((ushort)PacketID.S_GetItem, handler.S_GetItemHandler);
		_handler.Add((ushort)PacketID.S_GetGold, handler.S_GetGoldHandler);
		_handler.Add((ushort)PacketID.UpdateLocationBroadcast, handler.UpdateLocationBroadcastHandler);

	}

	public void Register()
	{
		_makeFunc.Add((ushort)PacketID.C_JoinToGame, MakePacket<C_JoinToGame>);
		_makeFunc.Add((ushort)PacketID.S_JoinToGame, MakePacket<S_JoinToGame>);
		_makeFunc.Add((ushort)PacketID.S_LeaveToGame, MakePacket<S_LeaveToGame>);
		_makeFunc.Add((ushort)PacketID.CS_SelectHero, MakePacket<CS_SelectHero>);
		_makeFunc.Add((ushort)PacketID.CS_ReadyToGame, MakePacket<CS_ReadyToGame>);
		_makeFunc.Add((ushort)PacketID.S_Countdown, MakePacket<S_Countdown>);
		_makeFunc.Add((ushort)PacketID.S_StartGame, MakePacket<S_StartGame>);
		_makeFunc.Add((ushort)PacketID.S_SpawnMonster, MakePacket<S_SpawnMonster>);
		_makeFunc.Add((ushort)PacketID.CS_Attack, MakePacket<CS_Attack>);
		_makeFunc.Add((ushort)PacketID.MoveRequest, MakePacket<MoveRequest>);
		_makeFunc.Add((ushort)PacketID.S_Dead, MakePacket<S_Dead>);
		_makeFunc.Add((ushort)PacketID.S_Respawn, MakePacket<S_Respawn>);
		_makeFunc.Add((ushort)PacketID.CS_UpdateStat, MakePacket<CS_UpdateStat>);
		_makeFunc.Add((ushort)PacketID.S_DropReward, MakePacket<S_DropReward>);
		_makeFunc.Add((ushort)PacketID.C_PickReward, MakePacket<C_PickReward>);
		_makeFunc.Add((ushort)PacketID.S_GetItem, MakePacket<S_GetItem>);
		_makeFunc.Add((ushort)PacketID.S_GetGold, MakePacket<S_GetGold>);
		_makeFunc.Add((ushort)PacketID.UpdateLocationBroadcast, MakePacket<UpdateLocationBroadcast>);

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
