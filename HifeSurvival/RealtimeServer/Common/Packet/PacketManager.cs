using System;
using System.Collections.Generic;

namespace ServerCore
{
	class PacketHandler
	{
		public void C_JoinToGameHandler(PacketSession session, IPacket packet) { }
		public void S_JoinToGameHandler(PacketSession session, IPacket packet) { }
		public void S_LeaveToGameHandler(PacketSession session, IPacket packet) { }
		public void CS_SelectHeroHandler(PacketSession session, IPacket packet) { }
		public void CS_ReadyToGameHandler(PacketSession session, IPacket packet) { }
		public void S_CountdownHandler(PacketSession session, IPacket packet) { }
		public void S_StartGameHandler(PacketSession session, IPacket packet) { }
		public void CS_AttackHandler(PacketSession session, IPacket packet) { }
		public void CS_MoveHandler(PacketSession session, IPacket packet) { }
		public void CS_StopMoveHandler(PacketSession session, IPacket packet) { }
		public void S_DeadHandler(PacketSession session, IPacket packet) { }
		public void S_RespawnHandler(PacketSession session, IPacket packet) { }
		public void CS_UpdateStatHandler(PacketSession session, IPacket packet) { }
		public void S_DropItemHandler(PacketSession session, IPacket packet) { }
		public void C_GetItemHandler(PacketSession session, IPacket packet) { }

	}

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
		
		public void BindHandler(PacketHandler handler)
		{
			_handler.Add((ushort)PacketID.C_JoinToGame, handler.C_JoinToGameHandler);
			_handler.Add((ushort)PacketID.S_JoinToGame, handler.S_JoinToGameHandler);
			_handler.Add((ushort)PacketID.S_LeaveToGame, handler.S_LeaveToGameHandler);
			_handler.Add((ushort)PacketID.CS_SelectHero, handler.CS_SelectHeroHandler);
			_handler.Add((ushort)PacketID.CS_ReadyToGame, handler.CS_ReadyToGameHandler);
			_handler.Add((ushort)PacketID.S_Countdown, handler.S_CountdownHandler);
			_handler.Add((ushort)PacketID.S_StartGame, handler.S_StartGameHandler);
			_handler.Add((ushort)PacketID.CS_Attack, handler.CS_AttackHandler);
			_handler.Add((ushort)PacketID.CS_Move, handler.CS_MoveHandler);
			_handler.Add((ushort)PacketID.CS_StopMove, handler.CS_StopMoveHandler);
			_handler.Add((ushort)PacketID.S_Dead, handler.S_DeadHandler);
			_handler.Add((ushort)PacketID.S_Respawn, handler.S_RespawnHandler);
			_handler.Add((ushort)PacketID.CS_UpdateStat, handler.CS_UpdateStatHandler);
			_handler.Add((ushort)PacketID.S_DropItem, handler.S_DropItemHandler);
			_handler.Add((ushort)PacketID.C_GetItem, handler.C_GetItemHandler);

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
			_makeFunc.Add((ushort)PacketID.CS_Attack, MakePacket<CS_Attack>);
			_makeFunc.Add((ushort)PacketID.CS_Move, MakePacket<CS_Move>);
			_makeFunc.Add((ushort)PacketID.CS_StopMove, MakePacket<CS_StopMove>);
			_makeFunc.Add((ushort)PacketID.S_Dead, MakePacket<S_Dead>);
			_makeFunc.Add((ushort)PacketID.S_Respawn, MakePacket<S_Respawn>);
			_makeFunc.Add((ushort)PacketID.CS_UpdateStat, MakePacket<CS_UpdateStat>);
			_makeFunc.Add((ushort)PacketID.S_DropItem, MakePacket<S_DropItem>);
			_makeFunc.Add((ushort)PacketID.C_GetItem, MakePacket<C_GetItem>);

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
}