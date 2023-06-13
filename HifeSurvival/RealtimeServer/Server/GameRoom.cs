using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    public interface IBroadcaster
    {
        void Broadcast(IPacket packet);
    }

    public class RoomBroadcaster : IBroadcaster
    {
        private GameRoom _room;

        public RoomBroadcaster(GameRoom room)
        {
            _room = room;
        }

        public void Broadcast(IPacket packet)
        {
            _room.Broadcast(packet);
        }
    }


    public class GameRoom : IJobQueue
    {
        JobQueue _jobQueue = new JobQueue();

        List<ClientSession>      _sessions = new List<ClientSession>();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        private GameMode _gameMode;
        private bool     _isRunningFlush;

        public int RoomId { get; private set; }

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
            Push(() =>
            {
                Logger.GetInstance().Log("INF", $"PacketType : {inPacket.GetType()}", $"{nameof(Broadcast)}");
                ArraySegment<byte> segment = inPacket.Write();
                _pendingList.Add(segment);
            });
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;

            if(_sessions.Count > 0)
            {
                _isRunningFlush = true;
                FlushRoom();
            }
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
            _gameMode.OnSendLeave(session.SessionId);

            session.Room = null;

            if (_sessions.Count == 0)
                _isRunningFlush = false;
        }

        public void FlushRoom()
        {
            if (this != null && _isRunningFlush == true)
            {
                Push(() => Flush());
                JobTimer.Instance.Push(FlushRoom, 125);
            }
        }

        public bool CanJoinRoom()
        {
            return _gameMode.CanJoinRoom();
        }
    }
}
