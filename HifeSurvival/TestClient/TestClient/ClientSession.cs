using System;
using System.Net;
using ServerCore;

namespace TestClient
{
    public class PlayerEntity
    {
        public int Id { get; set; }
        public int GameModeStatus { get; set; }
        public int ClientStatus { get; set; }   // 0 : none, 1: 준비, 2 : 게임 시작 준비 완료
    }

    public class ClientSession : Session
    {
        public bool IsConntected { get; private set; }
        public  PlayerEntity Player;


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


