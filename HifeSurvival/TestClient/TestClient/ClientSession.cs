using System;
using System.Net;
using System.Collections.Generic;
using ServerCore;

namespace TestClient
{
    public class PlayerEntity
    {
        public int Id { get; set; }
        public int HeroKey { get; set; }
        public int GameModeStatus { get; set; }
        public int ClientStatus { get; set; }   // 0 : none, 1: 준비, 2 : 게임 시작 준비 완료

        public float CountDownSec { get; set; }

        public List<PCurrency> CurrencyList { get; set; } = new List<PCurrency>();

        public PStat OriginStat;
        public PStat AdditionalStat;
        public Dictionary<int, PInvenItem> InvenItemDict { get; set; } = new Dictionary<int, PInvenItem>();
    }

    public class ClientSession : Session
    {
        public bool IsConntected { get; private set; }
        public PlayerEntity Player;


        public override void OnConnected(EndPoint endPoint)
        {
            IsConntected = true;
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            IsConntected = false;
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public void AutoReady()
        {
            JobTimer.Instance.Push(() =>
            {
                var req = new CS_SelectHero()
                {
                    id = Player.Id,
                    heroKey = 1,
                };

                Send(req.Write());
            }, 500);

            JobTimer.Instance.Push(() =>
            {
                var req = new CS_ReadyToGame()
                {
                    id = Player.Id,
                };

                Player.ClientStatus = 1;
                Send(req.Write());
            }, 2000);
        }

        public void AutoPlayStart()
        {
            JobTimer.Instance.Push(() =>
            {
                var req = new PlayStartRequest()
                {
                    id = Player.Id,
                };

                Player.ClientStatus = 2;
                Send(req.Write());
            }, 1000);
        }
    }
}


