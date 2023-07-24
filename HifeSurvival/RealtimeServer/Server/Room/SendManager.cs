using System;
using System.Collections.Concurrent;
using System.Linq;
using ServerCore;

namespace Server
{
    public class SendManager
    {
        private ConcurrentDictionary<int, ServerSession> _seshDict = new ConcurrentDictionary<int, ServerSession>();
        private ConcurrentQueue<ArraySegment<byte>> _broadcastMessage = new ConcurrentQueue<ArraySegment<byte>>();
        private ConcurrentDictionary<int, ConcurrentQueue<ArraySegment<byte>>> _sendMessage = new ConcurrentDictionary<int, ConcurrentQueue<ArraySegment<byte>>>();

        private bool _existSesh;
        private WorkManager _worker;

        public SendManager(WorkManager worker)
        {
            _worker = worker;
        }

        private void FlushSendQueue()
        {
            if (!_existSesh)
            {
                return;
            }

            if (_sendMessage.Count > 0)
            {
                foreach (var sm in _sendMessage)
                {
                    var sesh = _seshDict.AsQueryable().Where(ds => ds.Key == sm.Key).FirstOrDefault();
                    if (sesh.Value == null)
                    {
                        Logger.Instance.Warn("Not Found By  Session id {sm.Key}");
                        continue;
                    }

                    sesh.Value.Send(sm.Value);
                }

                _sendMessage.Clear();
            }

            if (_broadcastMessage.Count > 0)
            {
                foreach (var s in _seshDict)
                {
                    s.Value.Send(_broadcastMessage);
                }

                _broadcastMessage.Clear();
            }

            _worker.Push(FlushSendQueue, DEFINE.SERVER_TICK);
        }

        public void Broadcast(IPacket packet)
        {
            Logger.Instance.Info($"PacketType : {packet.GetType()}");
            Logger.Instance.Trace(packet);
            ArraySegment<byte> segment = packet.Write();
            _broadcastMessage.Enqueue(segment);
        }

        public void Send(int id, IPacket packet)
        {
            Logger.Instance.Info($"PacketType : {packet.GetType()}");
            Logger.Instance.Trace(packet);
            ArraySegment<byte> segment = packet.Write();
            if (_sendMessage.TryGetValue(id, out var msgList))
            {
                msgList.Enqueue(segment);
            }
            else
            {
                var initMsgList = new ConcurrentQueue<ArraySegment<byte>>();
                initMsgList.Enqueue(segment);
                _sendMessage.TryAdd(id, initMsgList);
            }
        }

        public void OnEnter(ServerSession session)
        {
            _seshDict.TryAdd(session.SessionId, session);
            Logger.Instance.Debug($"Session id {session.SessionId}");

            if (!_existSesh && _seshDict.Count > 0)
            {
                _existSesh = true;
                Logger.Instance.Debug($"FlushSendQueue Start");
                _worker.Push(FlushSendQueue, DEFINE.SERVER_TICK);
            }
        }

        public void OnLeave(int seshId)
        {
            _seshDict.TryRemove(seshId, out _);

            if (_seshDict.Count == 0 && _existSesh)
            {
                _existSesh = false;
                Logger.Instance.Debug($"FlushSendQueue End");
            }
        }
    }
}
