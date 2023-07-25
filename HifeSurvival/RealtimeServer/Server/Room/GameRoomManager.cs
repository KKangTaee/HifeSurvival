using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ServerCore;

namespace Server
{
    public class GameRoomManager
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

        private WorkManager _worker = new WorkManager("GameRoomManager");
        private ConcurrentDictionary<int, GameRoom> _gameRoomDict = new ConcurrentDictionary<int, GameRoom>();
        private int _nextRoomNum;

        private GameRoomManager()
        {
            _worker.Start();
        }

        public int GetRoomCount()
        {
            return _gameRoomDict.Count;
        }

        public void EnterRoom(ServerSession session)
        {
            _worker.Push(() =>
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
                    }

                    Logger.Instance.Warn($"Room Created {_nextRoomNum}");
                }
            });
        }

        public void LeaveRoom(ServerSession session)
        {
            _worker.Push(() =>
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
                room.Worker.Stop();
                Logger.Instance.Warn($"Room Deleted {roomId}");
            }
        }
    }
}
