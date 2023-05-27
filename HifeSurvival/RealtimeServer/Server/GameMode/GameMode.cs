using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class GameMode
    {
        public enum EStatus
        {
            READY,

            COUNTDOWN,

            GAME_START,

            RUNIING_GAME,

            FINISH_GAME,

            NONE,
        }


        Dictionary<int, PlayerEntity> _playersDict = new Dictionary<int, PlayerEntity>();
        Dictionary<int, MonsterEntity> _monstersDict = new Dictionary<int, MonsterEntity>();

        private const int PLAYER_MAX_COUNT = 4;
        private const int CONUTDOWN_SEC = 10;

        private int _roomId;

        IBroadcaster _broadcaster = null;
        public EStatus Status { get; private set; } = EStatus.NONE;

        public GameMode(GameRoom inRoom)
        {
            _broadcaster = new RoomBroadcaster(inRoom);
            _roomId = inRoom.RoomId;
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

                _monstersDict.Add(i, info);
            }

            var monsterList = new List<S_StartGame.Monster>();

            foreach (var info in _monstersDict.Values)
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

            foreach (var info in _playersDict.Values)
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


        public bool CanStartGame()
        {
            return _playersDict.Values.All(x => x.isReady);
        }

        public bool CanJoinRoom()
        {
            return Status == EStatus.READY && _playersDict.Count < PLAYER_MAX_COUNT;
        }

        public void OnRecvJoin(C_JoinToGame inPacket, int inSessionId)
        {
            var playerInfo = new PlayerEntity()
            {
                userId = inPacket.userId,
                playerId = inSessionId,
                heroType = 1,
                userName = inPacket.userName,
                broadcaster = _broadcaster,
                stat = new Stat(100,100,100,100,4)
            };

            _playersDict.Add(inSessionId, playerInfo);

            System.Console.WriteLine($"[{nameof(OnRecvJoin)}] 접속! userId {playerInfo.userId}, roomId : {_roomId}");

            // 브로드캐스팅
            S_JoinToGame packet = new S_JoinToGame();
            packet.joinPlayerList = new List<S_JoinToGame.JoinPlayer>();

            foreach (var player in _playersDict.Values)
                packet.joinPlayerList.Add(player.CreateJoinPlayerPacket());

            _broadcaster.Broadcast(packet);

            Status = EStatus.READY;
        }


        public void OnLeave(int inSessionId)
        {
            var playerInfo = _playersDict.Values.FirstOrDefault(x => x.playerId == inSessionId);

            if (playerInfo == null)
                return;

            _playersDict.Remove(playerInfo.playerId);

            S_LeaveToGame packet = new S_LeaveToGame()
            {
                userId = playerInfo.userId,
                playerId = playerInfo.playerId,
            };

            _broadcaster.Broadcast(packet);
        }

        public void OnStartGame()
        {
            S_StartGame gameStart = new S_StartGame();

            gameStart.playerList = SetupPlayer();
            gameStart.monsterList = SetupMonster();

            _broadcaster.Broadcast(gameStart);

            Status = EStatus.GAME_START;
        }

        public void OnRecvReady(CS_ReadyToGame inPacket)
        {
            if (_playersDict.TryGetValue(inPacket.playerId, out var player) == true)
            {
                player.isReady = true;
                _broadcaster.Broadcast(inPacket);

                // 모두 레디라면..? 게임시작
                if (CanStartGame() == true)
                {
                    OnCountdown();
                }
            }
        }

        public void OnCountdown()
        {
            S_Countdown countdown = new S_Countdown()
            {
                countdownSec = CONUTDOWN_SEC
            };

            _broadcaster.Broadcast(countdown);

            Status = EStatus.COUNTDOWN;

            // N초 후 자동으로 호출
            JobTimer.Instance.Push(OnStartGame, CONUTDOWN_SEC * 1000);
        }

        public void OnRecvSelect(CS_SelectHero inPacket)
        {
            _broadcaster.Broadcast(inPacket);
        }

        public void OnRecvAttack(CS_Attack inPacket)
        {
            // 플레이어를 공격했을 때
            if (inPacket.toIdIsPlayer == true)
            {
                if (_playersDict.TryGetValue(inPacket.toId, out var toTarget) == true &&
                    _playersDict.TryGetValue(inPacket.fromId, out var fromTarget) == true)
                {
                    var attackParam = new AttackParam()
                    {
                        attackValue = inPacket.damageValue,
                        target = toTarget,
                    };

                    fromTarget.OnAttack(attackParam);
                    _broadcaster.Broadcast(inPacket);
                }
            }
            // 몬스터를 공격했을 때
            else
            {
                if (_monstersDict.TryGetValue(inPacket.toId, out var toTarget) == true &&
                   _playersDict.TryGetValue(inPacket.fromId, out var fromTarget) == true)
                {
                    var attackParam = new AttackParam()
                    {
                        attackValue = inPacket.damageValue,
                        target = toTarget,
                    };

                    fromTarget.OnAttack(attackParam);
                    _broadcaster.Broadcast(inPacket);
                }
            }
        }


        public void OnRecvMove(CS_Move inPacket)
        {
            if (_playersDict.TryGetValue(inPacket.targetId, out var player) == true)
            {
                player.pos = inPacket.pos;
                player.dir = inPacket.dir;
                player.stat.speed = inPacket.speed;

                var moveParam = new MoveParam()
                {
                    pos   = inPacket.pos,
                    dir   = inPacket.dir,
                    speed = inPacket.speed,
                };

                // TODO@taeho.kang 이동상태 처리
                player.OnMove(moveParam);

                _broadcaster.Broadcast(inPacket);
            }
        }


        public void OnRecvStopMove(CS_StopMove inPacket)
        {
            if (_playersDict.TryGetValue(inPacket.targetId, out var player) == true)
            {
                player.pos = inPacket.pos;
                player.dir = inPacket.dir;

                var idleParam = new IdleParam()
                {
                    pos = inPacket.pos,
                    dir = inPacket.dir,
                };

                player.OnIdle(idleParam);

                _broadcaster.Broadcast(inPacket);
            }
        }
    }
}

