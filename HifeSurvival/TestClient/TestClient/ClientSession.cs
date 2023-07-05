using System;
using System.Net;
using ServerCore;

namespace TestClient
{
    public class ClientSession : Session
    {
        public bool IsConntected { get; private set; }

        public override void OnConnected(EndPoint endPoint)
        {
            IsConntected = true;
            Console.WriteLine($"OnConnected 접속성공!!: {endPoint}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            IsConntected = false;
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }
    }
}


