using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class GameMode
    {
        Dictionary<int, PlayerEntity>  _playersDic  = new Dictionary<int, PlayerEntity>();
        Dictionary<int, MonsterEntity> _monstersDic = new Dictionary<int, MonsterEntity>();

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
                var info = new MonsterEntity()
                {
                    monsterId = i,
                    groupId = i / 3,
                    monsterType = i % 3,
                    subId = i % 3,
                };

                _monstersDic.Add(i, info);
            }

            var monsterList = new List<S_StartGame.Monster>();

            foreach (var info in _monstersDic.Values)
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

            foreach (var info in _playersDic.Values)
            {
                var data = new S_StartGame.Player()
                {
                    playerId = info.playerId,
                    heroId = info.heroType,
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
            return _playersDic.Count == PLAYER_MAX_COUNT &&
                   _playersDic.Values.All(x => x.isReady);
        }


        public void Join(C_JoinToGame inPacket, int inSessionId)
        {
            var playerInfo = new PlayerEntity()
            {
                userId = inPacket.userId,
                playerId = inSessionId,
                heroType = 1,
                userName = inPacket.userName,
                // info = null,
            };

            _playersDic.Add(inSessionId, playerInfo);


            // 브로드캐스팅
            S_JoinToGame packet = new S_JoinToGame();
            packet.joinPlayerList = new List<S_JoinToGame.JoinPlayer>();

            foreach (var player in _playersDic.Values)
                packet.joinPlayerList.Add(player.CreateJoinPlayerPacket());

            _room.Broadcast(packet);
        }


        public void Leave(int inSessionId)
        {
            var playerInfo = _playersDic.Values.FirstOrDefault(x => x.playerId == inSessionId);

            if (playerInfo == null)
                return;

            _playersDic.Remove(playerInfo.playerId);

            // 브로드캐스팅
            S_LeaveToGame packet = new S_LeaveToGame()
            {
                userId = playerInfo.userId
            };

            _room.Broadcast(packet);
        }


        // GameMode.Attack(C_Attack inPacket) 함수
        public void Attack(C_Attack inPacket)
        {
            // 플레이어를 공격했을 때
            if(inPacket.toIdIsPlayer == true)
            {

            }
            // 몬스터를 공격했을 때
            else
            {
                if(_monstersDic.TryGetValue(inPacket.toId, out var monster) == true &&
                   _playersDic.TryGetValue(inPacket.fromId, out var player) == true)
                {                 
                    var damagedParam = new DamagedParam<PlayerEntity>()
                    {
                        damageValue = inPacket.damageValue,
                        target = player,
                    };

                    var attackParam = new AttackParam<MonsterEntity>()
                    {
                        damageValue = inPacket.damageValue,
                        target = monster,
                    };

                    player.OnAttack(attackParam);
                    monster.OnDamaged(damagedParam);
                }
            }
        }
    }
}

