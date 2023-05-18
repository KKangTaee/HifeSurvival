using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;
using System.Threading.Tasks;

namespace DummyClient
{
    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            
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

        public class PlayerDummy
        {
            public enum EGameStatus
            {
                TRY_TO_JOIN,
                SELECT_TO_HERO,
                READY_TO_GAME,
                START_GAME,
                GAME_PLAYING,
                FINISH_GAME,
            }

            public EGameStatus Status {get; set;}

            private TaskCompletionSource<S_JoinToRoom> joinTask;

            public async Task<bool>  TryToJoin(ServerSession session, string inUserId)
            {
                C_JoinToRoom joinToRoom = new C_JoinToRoom();
                joinToRoom.userId = inUserId;
                joinToRoom.userName = "탁공익";

                session.Send(joinToRoom.Write());

                joinTask = new TaskCompletionSource<S_JoinToRoom>();

                var completedTask = await Task.WhenAny(joinTask.Task, Task.Delay(10000));

                if(completedTask != joinTask.Task)
                    return false;

                var packet = joinTask.Task.Result;

                string log = null;

                log +=$"룸 번호 : {packet.roomId}, ";
                foreach(var player in packet.joinPlayerList)
                {
                    log += $"player : {player.playerId}, userid : {player.userName}";
                }
                System.Console.WriteLine(log);

                return true;
            }
        }

        public PlayerDummy Self { get; set; } = new PlayerDummy();
    }
}
