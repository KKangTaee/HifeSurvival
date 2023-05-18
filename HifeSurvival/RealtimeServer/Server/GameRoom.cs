using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public int RoomId { get; private set; }

        private GameMode _gameMode;
        public GameMode Mode { get => _gameMode; }

        public GameRoom(int inRoomId)
        {
            RoomId = inRoomId;
            _gameMode = new GameMode(this);
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            _pendingList.Clear();
        }

        public void Broadcast(IPacket inPacket)
        {
            System.Console.WriteLine($"[{inPacket.GetType()}] 브로드 캐스팅 완료");
            _jobQueue.Push(()=>
            {
                ArraySegment<byte> segment = inPacket.Write();
                _pendingList.Add(segment);
            });
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
            _gameMode.Leave(session.SessionId);
        }
    }
}
