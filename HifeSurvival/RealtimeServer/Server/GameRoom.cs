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

        public int ChannelId { get; private set; }
        public int JoinedCount { get => _sessions.Count; }
        public bool IsStartedGame { get; private set; }

        public GameRoom(int inChannelId)
        {
            ChannelId = inChannelId;
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

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
			System.Console.WriteLine($"룸번호 :{ChannelId}, 현재인원 {_sessions.Count}");
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}
