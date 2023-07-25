using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ServerCore;

namespace Server
{
    public class GameRoomManager : WorkQueue
    {
        private static GameRoomManager _ins;

        public static GameRoomManager Instance
        {
            get
            {
                _ins ??= new GameRoomManager();
                return _ins;
            }
        }

        private ConcurrentDictionary<int, GameRoom> _gameRoomDict = new ConcurrentDictionary<int, GameRoom>();
        private int _nextRoomNum;

        private GameRoomManager()
        {
            Start("GameRoomManager");
        }

        public void EnterRoom(ServerSession session)
        {
            Push(() =>
            {
                var canJoinRoom = _gameRoomDict.Values.FirstOrDefault(x => x.CanJoinRoom());
                if (canJoinRoom != null)
                {
                    canJoinRoom.Enter(session);
                }
                else
                {
                    var newRoom = new GameRoom(++_nextRoomNum);
                    if (_gameRoomDict.TryAdd(newRoom.RoomId, newRoom))
                    {
                        newRoom.Enter(session);
                        Logger.Instance.Warn($"Room Created {_nextRoomNum}");
                    }
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

                    if (session.Room.IsEmptyRoom())
                    {
                        TerminateRoom(room.RoomId);
                    }
                }
            });
        }

        public void TerminateRoom(int roomId)
        {
            if (_gameRoomDict.TryRemove(roomId, out var room))
            {
                room.ReleaseRoom();
                Logger.Instance.Warn($"Room Deleted {roomId}");
            }
        }
    }
}
