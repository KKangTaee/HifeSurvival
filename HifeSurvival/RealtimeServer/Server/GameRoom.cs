using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class GameRoom : IJobQueue
    {
        JobQueue _jobQueue = new JobQueue();

        List<ClientSession> _sessions = new List<ClientSession>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();


        private GameMode _gameMode;

        public int RoomId { get; private set; }
        public bool IsRunning { get; private set; }

        public GameMode Mode { get => _gameMode; }

        public GameRoom(int inRoomId)
        {
            RoomId = inRoomId;
            _gameMode = new GameMode(this);
            IsRunning = true;

            FlushRoom();
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
            _jobQueue.Push(() =>
            {
                System.Console.WriteLine($"[{inPacket.GetType()}] 브로드캐스팅");
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
            _gameMode.OnLeave(session.SessionId);

            if (_sessions.Count == 0)
                IsRunning = false;
        }

        public void FlushRoom()
        {
            if (this != null && IsRunning == true)
            {
                Push(() => Flush());
                JobTimer.Instance.Push(FlushRoom, 250);
            }
        }
    }
}
