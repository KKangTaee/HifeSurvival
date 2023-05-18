using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class GameMode
    {
        Dictionary<int, ServerPlayer> _playerInfoDic = new Dictionary<int, ServerPlayer>();
        Dictionary<int, ServerMonster> _monsterInfoDic = new Dictionary<int, ServerMonster>();

        private const int PLAYER_MAX_COUNT = 4;

        private GameRoom _room;

        public GameMode(GameRoom inRoom)
        {
            _room = inRoom;
        }

        public void StartGame()
        {
            // 패킷을 만든다.
            S_StartGame gameStart = new S_StartGame();

            gameStart.playerList = SetupPlayer();
            gameStart.monsterList = SetupMonster();
        }

        public List<S_StartGame.Monster> SetupMonster()
        {
            // 임시로 처리함.
            for (int i = 0; i < 9; i++)
            {
                var info = new ServerMonster()
                {
                    monsterId = i,
                    groupId = i / 3,
                    monsterType = i % 3,
                    subId = i % 3,
                };

                _monsterInfoDic.Add(i, info);
            }

            var monsterList = new List<S_StartGame.Monster>();

            foreach (var info in _monsterInfoDic.Values)
            {
                var data = new S_StartGame.Monster()
                {
                    monsterId = info.monsterId,
                    monsterType = info.monsterType,
                    groupId = info.groupId,
                    subId = info.subId,
                };

                monsterList.Add(data);
            }

            return monsterList;
        }

        public List<S_StartGame.Player> SetupPlayer()
        {
            var playerList = new List<S_StartGame.Player>();

            foreach (var info in _playerInfoDic.Values)
            {
                var data = new S_StartGame.Player()
                {
                    playerId = info.playerId,
                    heroType = info.heroType,
                };

                playerList.Add(data);
            }

            return playerList;
        }

        public void Init()
        {

        }

        public void Release()
        {

        }

        public void Update(double deltaTime)
        {

        }


        public bool CanStartGame()
        {
            return _playerInfoDic.Count == PLAYER_MAX_COUNT &&
                   _playerInfoDic.Values.All(x => x.isReady);
        }


        public void Join(C_JoinToGame inPacket, int inSessionId)
        {
            var playerInfo = new ServerPlayer()
            {
                userId = inPacket.userId,
                playerId = inSessionId,
                heroType = 1,
                userName = inPacket.userName,
                // info = null,
            };

            _playerInfoDic.Add(inSessionId, playerInfo);


            // 브로드캐스팅
            S_JoinToGame packet = new S_JoinToGame();
            packet.joinPlayerList = new List<S_JoinToGame.JoinPlayer>();

            foreach (var player in _playerInfoDic.Values)
                packet.joinPlayerList.Add(player.CreateJoinPlayerPacket());

            _room.Broadcast(packet);
        }


        public void Leave(int inSessionId)
        {
            var playerInfo = _playerInfoDic.Values.FirstOrDefault(x => x.playerId == inSessionId);

            if (playerInfo == null)
                return;

            _playerInfoDic.Remove(playerInfo.playerId);

            // 브로드캐스팅
            S_LeaveToGame packet = new S_LeaveToGame()
            {
                userId = playerInfo.userId
            };

            _room.Broadcast(packet);
        }
    }
}