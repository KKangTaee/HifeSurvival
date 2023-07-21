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

                    if(session.Room.SessionCount <= 0)
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
                //TODO : Room 이 만든 타이머 혹은 얽힌 것들 모두 지워야함.. 
                Logger.Instance.Warn($"Room Deleted {roomId}");
            }
        }
    }
}
