using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

namespace DummyClient
{
    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }


        public class QuickMatch
        {
            public enum EStatus
            {
                READY_TO_MACTH,
                SELECT_TO_HERO,
                READY_TO_GAME,
                COUNTDOWN_GAME,
                JOIN_TO_GAME,
                NONE,
            }

            public EStatus Status { get; private set; } = EStatus.NONE;

            public int PlayerId { get; private set; }

            public int ChannelId { get; private set; }

            public void SetStatus(EStatus inStatus) =>
                Status = inStatus;

            public void SetPlayerId(int inPlayerId) =>
                PlayerId = inPlayerId;

            public void SetChannelId(int inChannelId) =>
                ChannelId = inChannelId;

        }

        public QuickMatch quickMatch;
    }
}
