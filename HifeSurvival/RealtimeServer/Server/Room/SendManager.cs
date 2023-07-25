using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public class SendManager
    {
        private ConcurrentDictionary<int, ServerSession> _seshDict = new ConcurrentDictionary<int, ServerSession>();
        private ConcurrentQueue<ArraySegment<byte>> _broadcastMessage = new ConcurrentQueue<ArraySegment<byte>>();
        private ConcurrentDictionary<int, ConcurrentQueue<ArraySegment<byte>>> _sendMessage = new ConcurrentDictionary<int, ConcurrentQueue<ArraySegment<byte>>>();

        private bool _existSesh;
        private bool _isRun;


        private async void Run()
        {
            if(_isRun)
            {
                return;
            }


            _isRun = true;

            while (_isRun)
            {
                _seshDict.AsParallel().ForAll(sesh =>
                {
                    sesh.Value.Send(_broadcastMessage);
                    if (_sendMessage.TryGetValue(sesh.Key, out var msg))
                    {
                        sesh.Value.Send(msg);
                    }
                });

                if (_sendMessage.Count != 0)
                {
                    _sendMessage.Clear();
                }

                if (_broadcastMessage.Count != 0)
                {
                    _broadcastMessage.Clear();
                }

                await Task.Delay(DEFINE.SERVER_TICK);
            }
        }

        public void Stop()
        {
            _isRun = false;
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
                Logger.Instance.Debug($"SendManager Run!");
                Run();
            }
        }

        public void OnLeave(int seshId)
        {
            _seshDict.TryRemove(seshId, out _);
        }
    }
}
