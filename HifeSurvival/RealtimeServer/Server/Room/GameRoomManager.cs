using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ServerCore;

namespace Server
{
    public class GameRoomManager : JobQueue
    {
        public static GameRoomManager Instance { get => _instance; }

        private static GameRoomManager _instance = new GameRoomManager();
        private ConcurrentDictionary<int, GameRoom> _gameRoomDict = new ConcurrentDictionary<int, GameRoom>();
        private int _nextRoomNum;

        public void FlushAllRoom()
        {
            foreach(var gr in _gameRoomDict)
            {
                gr.Value.Worker.Flush();
            }
        }

        public int GetRoomCount()
        {
            return _gameRoomDict.Count;
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
                    newRoom.Enter(session);
                    _gameRoomDict.TryAdd(newRoom.RoomId, newRoom);
                    Logger.Instance.Warn($"Room Created {_nextRoomNum}");
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

                    if(session.Room.IsEmptyRoom())
                    {
                        TerminateRoom(room.RoomId);
                    }
                }
            });
        }

        public void TerminateRoom(int roomId)
        {
            if(_gameRoomDict.TryRemove(roomId, out var room))
            {
                room.Worker.ClearTimer();
                Logger.Instance.Warn($"Room Deleted {roomId}");
            }
        }
    }
}
