using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public class GameRoomManager : IJobQueue
    {
        static GameRoomManager _instance = new GameRoomManager();
        public static GameRoomManager Instance { get => _instance; }

        private Dictionary<int, GameRoom> _gameRoomDict = new Dictionary<int, GameRoom>();

        private JobQueue _jobQueue = new JobQueue();

        private int _nextRoomNum = 1;

        public void EnterRoom(ClientSession session)
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
                    var newRoom = new GameRoom(_nextRoomNum++);
                    newRoom.Enter(session);
                    _gameRoomDict.Add(newRoom.RoomId, newRoom);

                }
            });
        }

        public void LeaveRoom(ClientSession session)
        {
            Push(() =>
            {
                SessionManager.Instance.Remove(session);

                if (session.Room != null)
                {
                    GameRoom room = session.Room;
                    room.Push(() => room.Leave(session));
                }
            });
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }
    }
}
