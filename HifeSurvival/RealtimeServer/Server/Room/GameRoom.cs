﻿using System;
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

        private JobQueue _jobQueue = new JobQueue();
        private List<ClientSession> _sessionList = new List<ClientSession>();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        private GameMode _gameMode;
        private bool _isRunningFlush;

        public int RoomId { get; private set; }

        public GameMode Mode { get => _gameMode; }

        public GameRoom(int roomId)
        {
            RoomId = roomId;
            _gameMode = new GameMode(this);
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessionList)
                s.Send(_pendingList);

            _pendingList.Clear();
        }

        public void Broadcast(IPacket packet)
        {
            Push(() =>
            {
                Logger.GetInstance().Log("INF", $"PacketType : {packet.GetType()}", $"{nameof(Broadcast)}");
                ArraySegment<byte> segment = packet.Write();
                _pendingList.Add(segment);
            });
        }

        public void Enter(ClientSession session)
        {
            _sessionList.Add(session);
            session.Room = this;

            if (_sessionList.Count > 0)
            {
                _isRunningFlush = true;
                FlushRoom();
            }
        }

        public void Leave(ClientSession session)
        {
            _sessionList.Remove(session);
            _gameMode.OnSessionRemove(session.SessionId);

            session.Room = null;

            if (_sessionList.Count == 0)
                _isRunningFlush = false;
        }

        public void FlushRoom()
        {
            if (this != null && _isRunningFlush == true)
            {
                Push(() => Flush());
                JobTimer.Instance.Push(FlushRoom, DEFINE.SEND_TICK_MS);
            }
        }

        public bool CanJoinRoom()
        {
            return _gameMode.CanJoinRoom();
        }
    }
}
