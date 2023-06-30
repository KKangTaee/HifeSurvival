using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;
using System.Threading.Tasks;

namespace DummyClient
{
    class ClientSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected 접속성공!!: {endPoint}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }
    }
}


