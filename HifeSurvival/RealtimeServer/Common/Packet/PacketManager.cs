using System;
using System.Collections.Generic;
using ServerCore;


public abstract class PacketHandler
{
	public abstract void C_JoinToGameHandler(Session session, IPacket packet);
	public abstract void S_JoinToGameHandler(Session session, IPacket packet);
	public abstract void S_LeaveToGameHandler(Session session, IPacket packet);
	public abstract void CS_SelectHeroHandler(Session session, IPacket packet);
	public abstract void CS_ReadyToGameHandler(Session session, IPacket packet);
	public abstract void S_CountdownHandler(Session session, IPacket packet);
	public abstract void S_StartGameHandler(Session session, IPacket packet);
	public abstract void S_SpawnMonsterHandler(Session session, IPacket packet);
	public abstract void CS_AttackHandler(Session session, IPacket packet);
	public abstract void MoveRequestHandler(Session session, IPacket packet);
	public abstract void MoveResponseHandler(Session session, IPacket packet);
	public abstract void S_DeadHandler(Session session, IPacket packet);
	public abstract void S_RespawnHandler(Session session, IPacket packet);
	public abstract void IncreaseStatRequestHandler(Session session, IPacket packet);
	public abstract void IncreaseStatResponseHandler(Session session, IPacket packet);
	public abstract void PickRewardRequestHandler(Session session, IPacket packet);
	public abstract void PickRewardResponseHandler(Session session, IPacket packet);
	public abstract void UpdateRewardBroadcastHandler(Session session, IPacket packet);
	public abstract void UpdateLocationBroadcastHandler(Session session, IPacket packet);
	public abstract void UpdateStatBroadcastHandler(Session session, IPacket packet);
	public abstract void UpdatePlayerCurrencyHandler(Session session, IPacket packet);
	public abstract void PlayStartRequestHandler(Session session, IPacket packet);
	public abstract void PlayStartResponseHandler(Session session, IPacket packet);
	public abstract void UpdateGameModeStatusBroadcastHandler(Session session, IPacket packet);

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


	Dictionary<ushort, Func<Session, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<Session, ArraySegment<byte>, IPacket>>();
	Dictionary<ushort, Action<Session, IPacket>> _handler = new Dictionary<ushort, Action<Session, IPacket>>();
	
	public void BindHandler(PacketHandler handler)
	{
		_handler.Clear();
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
		_handler.Add((ushort)PacketID.MoveResponse, handler.MoveResponseHandler);
		_handler.Add((ushort)PacketID.S_Dead, handler.S_DeadHandler);
		_handler.Add((ushort)PacketID.S_Respawn, handler.S_RespawnHandler);
		_handler.Add((ushort)PacketID.IncreaseStatRequest, handler.IncreaseStatRequestHandler);
		_handler.Add((ushort)PacketID.IncreaseStatResponse, handler.IncreaseStatResponseHandler);
		_handler.Add((ushort)PacketID.PickRewardRequest, handler.PickRewardRequestHandler);
		_handler.Add((ushort)PacketID.PickRewardResponse, handler.PickRewardResponseHandler);
		_handler.Add((ushort)PacketID.UpdateRewardBroadcast, handler.UpdateRewardBroadcastHandler);
		_handler.Add((ushort)PacketID.UpdateLocationBroadcast, handler.UpdateLocationBroadcastHandler);
		_handler.Add((ushort)PacketID.UpdateStatBroadcast, handler.UpdateStatBroadcastHandler);
		_handler.Add((ushort)PacketID.UpdatePlayerCurrency, handler.UpdatePlayerCurrencyHandler);
		_handler.Add((ushort)PacketID.PlayStartRequest, handler.PlayStartRequestHandler);
		_handler.Add((ushort)PacketID.PlayStartResponse, handler.PlayStartResponseHandler);
		_handler.Add((ushort)PacketID.UpdateGameModeStatusBroadcast, handler.UpdateGameModeStatusBroadcastHandler);

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
		_makeFunc.Add((ushort)PacketID.MoveResponse, MakePacket<MoveResponse>);
		_makeFunc.Add((ushort)PacketID.S_Dead, MakePacket<S_Dead>);
		_makeFunc.Add((ushort)PacketID.S_Respawn, MakePacket<S_Respawn>);
		_makeFunc.Add((ushort)PacketID.IncreaseStatRequest, MakePacket<IncreaseStatRequest>);
		_makeFunc.Add((ushort)PacketID.IncreaseStatResponse, MakePacket<IncreaseStatResponse>);
		_makeFunc.Add((ushort)PacketID.PickRewardRequest, MakePacket<PickRewardRequest>);
		_makeFunc.Add((ushort)PacketID.PickRewardResponse, MakePacket<PickRewardResponse>);
		_makeFunc.Add((ushort)PacketID.UpdateRewardBroadcast, MakePacket<UpdateRewardBroadcast>);
		_makeFunc.Add((ushort)PacketID.UpdateLocationBroadcast, MakePacket<UpdateLocationBroadcast>);
		_makeFunc.Add((ushort)PacketID.UpdateStatBroadcast, MakePacket<UpdateStatBroadcast>);
		_makeFunc.Add((ushort)PacketID.UpdatePlayerCurrency, MakePacket<UpdatePlayerCurrency>);
		_makeFunc.Add((ushort)PacketID.PlayStartRequest, MakePacket<PlayStartRequest>);
		_makeFunc.Add((ushort)PacketID.PlayStartResponse, MakePacket<PlayStartResponse>);
		_makeFunc.Add((ushort)PacketID.UpdateGameModeStatusBroadcast, MakePacket<UpdateGameModeStatusBroadcast>);

	}

	public void OnRecvPacket(Session session, ArraySegment<byte> buffer, Action<Session, IPacket> onRecvCallback = null)
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

	T MakePacket<T>(Session session, ArraySegment<byte> buffer) where T : IPacket, new()
	{
		T pkt = new T();
		pkt.Read(buffer);
		return pkt;	
	}

	public void HandlePacket(Session inSession, IPacket inPacket)
	{
		Action<Session, IPacket> action = null;
		if (_handler.TryGetValue(inPacket.Protocol, out action))
			action.Invoke(inSession, inPacket);
	}
}
