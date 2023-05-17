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

        private Dictionary<int, GameRoom> _gameRoomDic = new Dictionary<int, GameRoom>();

        private JobQueue _jobQueue = new JobQueue();

        const int MAX_PLAYER_COUNT_IN_ROOM = 4;
        
        private int nextRoomNum = 1;

        object _lock = new object();

        public void EnterRoom(ClientSession session)
        {
            Push(()=> 
            {
                var canJoinRoom = _gameRoomDic.Values.FirstOrDefault(x=>x.IsStartedGame == false && 
                                                                    x.JoinedCount <MAX_PLAYER_COUNT_IN_ROOM);
            
                if(canJoinRoom != null)
                {
                    canJoinRoom.Enter(session);
                }
                else
                {
                    var newRoom = new GameRoom(nextRoomNum++);
                    newRoom.Enter(session);
                    _gameRoomDic.Add(newRoom.ChannelId, newRoom);    
                }
            });
        }

        public void LeaveRoom(ClientSession session)
        {
            Push(()=>
            {
                SessionManager.Instance.Remove(session);
                
                if(session.Room != null)
                {
                    GameRoom room = session.Room;
				    room.Push(() => room.Leave(session));
				    session.Room = null;
                }
            });
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }
    }
}
