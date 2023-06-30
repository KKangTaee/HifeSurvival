using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public class GameRoomManager : JobQueue
    {
        public static GameRoomManager Instance { get => _instance; }

        private static GameRoomManager _instance = new GameRoomManager();
        private Dictionary<int, GameRoom> _gameRoomDict = new Dictionary<int, GameRoom>();
        private int _nextRoomNum = 1;

        public void EnterRoom(ServerSession session)
        {
            Push(() =>
            {
                var canJoinRoom = _gameRoomDict.Values.FirstOrDefault(x => x.Mode.CanJoinRoom());
                if (canJoinRoom != null)
                {
                    canJoinRoom.Enter(session);
                }
                else
                {
                    var newRoom = new GameRoom(_nextRoomNum++);
                    newRoom.Enter(session);
                    _gameRoomDict.Add(newRoom.RoomId, newRoom);

                }
            });
        }

        public void LeaveRoom(ServerSession session)
        {
            Push(() =>
            {
                SessionManager.Instance.Remove(session);
                if (session.Room != null)
                {
                    var room = session.Room;
                    room.Leave(session);
                }
            });
        }
    }
}
