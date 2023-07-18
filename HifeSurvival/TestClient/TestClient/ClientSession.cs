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
        public PlayerEntity Player { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            IsConntected = true;
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            IsConntected = false;
            var packet = new S_LeaveToGame()
            {
                id = Player?.Id ?? 0,
                userId = DEFINE.TEST_USER_ID,
            };

            Send(packet.Write());
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public void RoomReady()
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

        public void PlayStart()
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

        public void Cheat(string command)
        {
            var commandArr = command.Split();

            var cheatReq = new CheatRequest();

            if (commandArr.Length == 0)
            {
                return;
            }

            cheatReq.type = commandArr[0];

            if(commandArr.Length == 2)
            {
                cheatReq.arg1 = int.Parse(commandArr[1]);
            }

            if (commandArr.Length == 3)
            {
                cheatReq.arg2 = int.Parse(commandArr[2]);
            }

            if (commandArr.Length == 4)
            {
                cheatReq.arg3 = int.Parse(commandArr[3]);
            }

            if (commandArr.Length == 5)
            {
                cheatReq.arg4 = int.Parse(commandArr[4]);
            }

            if (commandArr.Length == 6)
            {
                cheatReq.arg5 = int.Parse(commandArr[5]);
            }

            Send(cheatReq.Write());
        }
    }
}


