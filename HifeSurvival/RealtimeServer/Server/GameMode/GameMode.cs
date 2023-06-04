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


        Dictionary<int, PlayerEntity>  _playersDict  = new Dictionary<int, PlayerEntity>();
        Dictionary<int, MonsterEntity> _monstersDict = new Dictionary<int, MonsterEntity>();

        private const int PLAYER_MAX_COUNT  = 4;
        private const int CONUTDOWN_SEC     = 10;

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
                    targetId = i,
                    groupId = i / 3,
                    monsterId = i % 3,
                    subId = i % 3,
                };

                _monstersDict.Add(i, info);
            }

            var monsterList = new List<S_StartGame.Monster>();

            foreach (var info in _monstersDict.Values)
            {
                var data = new S_StartGame.Monster()
                {
                    targetId  = info.targetId,
                    monsterId = info.monsterId,
                    groupId   = info.groupId,
                    subId     = info.subId,
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
                    targetId = info.targetId,
                    heroId   = info.heroId,
                };

                playerList.Add(data);
            }

            return playerList;
        }

        public PlayerEntity GetPlayerEntity(int inId)
        {
            if (_playersDict.TryGetValue(inId, out var player) && player != null)
            {
                return player;
            }

            Console.WriteLine("PlayerEntity is null or Empty");
            return null;
        }

        public MonsterEntity GetMonsterEntity(int inId)
        {
            if (_monstersDict.TryGetValue(inId, out var monster) && monster != null)
            {
                return monster;
            }

            Console.WriteLine("MonsterEntity is null or Empty");
            return null;
        }


        public bool CanStartGame()
        {
            return _playersDict.Values.All(x => x.isReady);
        }


        public bool CanJoinRoom()
        {
            return Status == EStatus.READY && _playersDict.Count < PLAYER_MAX_COUNT;
        }


        //---------------
        // Send
        //---------------

        public void OnSendLeave(int inSessionId)
        {
            var playerInfo = _playersDict.Values.FirstOrDefault(x => x.targetId == inSessionId);

            if (playerInfo == null)
                return;

            _playersDict.Remove(playerInfo.targetId);

            S_LeaveToGame packet = new S_LeaveToGame()
            {
                userId   = playerInfo.userId,
                targetId = playerInfo.targetId,
            };

            _broadcaster.Broadcast(packet);
        }


        public void OnSendStartGame()
        {
            S_StartGame gameStart = new S_StartGame();

            gameStart.playerList = SetupPlayer();
            gameStart.monsterList = SetupMonster();

            _broadcaster.Broadcast(gameStart);

            Status = EStatus.GAME_START;
        }


        public void OnSendCountDown()
        {
            S_Countdown countdown = new S_Countdown()
            {
                countdownSec = CONUTDOWN_SEC
            };

            _broadcaster.Broadcast(countdown);

            Status = EStatus.COUNTDOWN;

            // N초 후 자동으로 호출
            JobTimer.Instance.Push(OnSendStartGame, CONUTDOWN_SEC * 1000);
        }

        public void OnSendRespawn(int inPlayerId)
        {
            var player = GetPlayerEntity(inPlayerId);

            if (player == null)
                return;

            S_Respawn respawn = new S_Respawn()
            {
                targetId = inPlayerId,
                isPlayer = true,
                // stat = 
            };
        }


        //---------------
        // Receive
        //---------------

        public void OnRecvJoin(C_JoinToGame inPacket, int inSessionId)
        {
            var playerInfo = new PlayerEntity()
            {
                userId = inPacket.userId,
                targetId = inSessionId,
                heroId = 1,
                userName = inPacket.userName,
                broadcaster = _broadcaster,
                stat = new EntityStat(StaticData.Instance.HeroDic["1"])
            };

            _playersDict.Add(inSessionId, playerInfo);

            System.Console.WriteLine($"[{nameof(OnRecvJoin)}] 접속! userId {playerInfo.userId}, roomId : {_roomId}");


            S_JoinToGame packet = new S_JoinToGame();
            packet.joinPlayerList = new List<S_JoinToGame.JoinPlayer>();

            foreach (var player in _playersDict.Values)
                packet.joinPlayerList.Add(player.CreateJoinPlayerPacket());

            _broadcaster.Broadcast(packet);

            Status = EStatus.READY;
        }


        public void OnRecvReady(CS_ReadyToGame inPacket)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            player.isReady = true;
            _broadcaster.Broadcast(inPacket);

            // 모두 레디라면..? 게임시작
            if (CanStartGame() == true)
                OnSendCountDown();
        }


        public void OnRecvSelect(CS_SelectHero inPacket)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            player.heroId = inPacket.heroId;

            _broadcaster.Broadcast(inPacket);
        }


        public void OnRecvMove(CS_Move inPacket)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            player.pos = inPacket.pos;
            player.dir = inPacket.dir;

            player.OnMove();
        }


        public void OnRecvStopMove(CS_StopMove inPacket)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            player.pos = inPacket.pos;
            player.dir = inPacket.dir;

            player.OnIdle();

            _broadcaster.Broadcast(inPacket);
        }


        public void OnRecvAttack(CS_Attack inPacket)
        {
            var fromPlayer = GetPlayerEntity(inPacket.fromId);

            if (fromPlayer == null)
                return;

            fromPlayer.pos = inPacket.fromPos;
            fromPlayer.dir = inPacket.fromDir;

            Entity toEntity = null;

            if (inPacket.toIdIsPlayer == true)
                toEntity = GetPlayerEntity(inPacket.toId);          
            
            else
                toEntity = GetMonsterEntity(inPacket.toId);

            if (toEntity == null)
                return;

            toEntity.stat.AddCurrHp(-inPacket.attackValue);

            if (toEntity.stat.currHp <= 0)
            {
                S_Dead dead = new S_Dead()
                {
                    toId          = inPacket.toId,
                    fromId        = inPacket.fromId,
                    respawnTime   = 15,
                    toIdIsPlayer  = toEntity.IsPlayer
                };

                var deadParam = new DeadParam()
                {
                    respawnTime = 15,
                    respawnCallback = ()=>
                    {
                        OnSendRespawn(inPacket.toId);
                    },
                    
                };
                
                toEntity.OnDead(deadParam);
                fromPlayer.OnIdle();

                _broadcaster.Broadcast(dead);
            }
            else
            {
                var attackParam = new AttackParam()
                {
                    attackValue = inPacket.attackValue,
                    target = toEntity,
                };

                fromPlayer.OnAttack(attackParam);
                _broadcaster.Broadcast(inPacket);
            }
        }


        public void OnRecvUpdateStat(CS_UpdateStat inPacket)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            if (inPacket.usedGold > player.gold)
                return;

            player.gold -= inPacket.usedGold;

            player.stat.AddHp(inPacket.updateStat.str);
            player.stat.AddDef(inPacket.updateStat.def);
            player.stat.AddHp(inPacket.updateStat.hp);
            player.stat.AddCurrHp(inPacket.updateStat.hp);

            _broadcaster.Broadcast(inPacket);
        }
    }
}