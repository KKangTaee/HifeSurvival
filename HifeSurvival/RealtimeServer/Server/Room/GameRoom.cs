using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    public class GameRoom : JobQueue
    {
        public int RoomId { get; private set; }

        public GameMode Mode { get; private set; }

        private Sender _sender;

        public GameRoom(int roomId)
        {
            RoomId = roomId;
            Mode = new GameMode(this);
            _sender = new Sender();
        }

        public void Enter(ServerSession session)
        {
            session.Room = this;
            _sender.OnEnter(session);
        }

        public void Leave(ServerSession session)
        {
            var seshId = session.SessionId;
            _sender.OnLeave(seshId);
            Mode.OnSessionRemove(seshId);
            //TODO : 리펙토링 중 : GameRoom 등록된 곳에서 삭제 처리 필요. 
        }

        public void Send(int id, IPacket p)
        {
            _sender.Send(id, p);
        }

        public void Broadcast(IPacket p)
        {
            _sender.Broadcast(p);
        }
    }
}
