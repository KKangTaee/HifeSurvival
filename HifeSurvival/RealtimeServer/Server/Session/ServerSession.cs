using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;

namespace Server
{
    public class ServerSession : Session
	{
		public int SessionId { get; set; }
		public GameRoom Room { get; set; }

		public override void OnConnected(EndPoint endPoint)
		{
            Logger.Instance.Info($"OnConnected : {endPoint}");
			GameRoomManager.Instance.EnterRoom(this);
		}

		public override void OnRecv(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameRoomManager.Instance.LeaveRoom(this);
            Logger.Instance.Info($"OnDisconnected : {endPoint}");
		}
	}
}
