using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerCore;

namespace Server
{
    public class Sender : JobQueue
    {
        private Dictionary<int, ServerSession> _seshDict = new Dictionary<int, ServerSession>();
        private List<ArraySegment<byte>> _broadcastMessage = new List<ArraySegment<byte>>();
        private Dictionary<int, List<ArraySegment<byte>>> _sendMessage = new Dictionary<int, List<ArraySegment<byte>>>();

        private bool _existSesh;

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
                    if(sesh.Value == null)
                    {
                        Logger.GetInstance().Warn("Not Found By  Session id {sm.Key}");
                        continue;
                    }

                    sesh.Value.Send(sm.Value);
                }

                _sendMessage.Clear();
            }

            if(_broadcastMessage.Count > 0)
            {
                foreach (var s in _seshDict)
                {
                    s.Value.Send(_broadcastMessage);
                }

                _broadcastMessage.Clear();
            }

            JobTimer.Instance.Push(FlushSendQueue, DEFINE.SEND_TICK_MS);
        }

        public void Broadcast(IPacket packet)
        {
            Push(() =>
            {
                Logger.GetInstance().Log("INF", $"PacketType : {packet.GetType()}", $"{nameof(Broadcast)}");
                Logger.GetInstance().Trace(packet);
                ArraySegment<byte> segment = packet.Write();
                _broadcastMessage.Add(segment);
            });
        }

        public void Send(int id , IPacket packet)
        {
            Push(() =>
            {
                Logger.GetInstance().Log("INF", $"PacketType : {packet.GetType()}", $"{nameof(Send)}");
                Logger.GetInstance().Trace(packet);
                ArraySegment<byte> segment = packet.Write();
                if(_sendMessage.TryGetValue(id, out var msgList))
                {
                    msgList.Add(segment);
                }
                else
                {
                    var initMsgList = new List<ArraySegment<byte>>();
                    initMsgList.Add(segment);
                    _sendMessage.Add(id, initMsgList);
                }
            });
        }

        public void OnEnter(ServerSession session)
        {
            _seshDict.Add(session.SessionId, session);
            Logger.GetInstance().Debug($"Session id {session.SessionId}");

            if (!_existSesh && _seshDict.Count > 0 )
            {
                _existSesh = true;
                Logger.GetInstance().Debug($"FlushSendQueue Start");
                JobTimer.Instance.Push(FlushSendQueue, DEFINE.SEND_TICK_MS);
            }
        }

        public void OnLeave(int seshId)
        {
            _seshDict.Remove(seshId);

            if(_seshDict.Count == 0 && _existSesh)
            {
                _existSesh = false;
                Logger.GetInstance().Debug($"FlushSendQueue End");
            }
        }
    }
}
