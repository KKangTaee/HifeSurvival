using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private const int PLAYER_MAX_COUNT = 4;
        private const int CONUTDOWN_SEC = 10;

        private Dictionary<int, PlayerEntity> _playersDict = new Dictionary<int, PlayerEntity>();
        private Dictionary<int, MonsterGroup> _monsterGroupDict = new Dictionary<int, MonsterGroup>();

        private IBroadcaster _broadcaster = null;
        private WorldMap _worldMap = new WorldMap();
        private int _mId = 10000;

        public EStatus Status { get; private set; } = EStatus.NONE;



        public GameMode(GameRoom inRoom)
        {
            _broadcaster = new RoomBroadcaster(inRoom);

            _worldMap.Init();
        }


        public List<S_StartGame.Monster> SetupMonster()
        {
            var monsterList = new List<S_StartGame.Monster>();

            var groupList = StaticData.Instance.MonstersGroupDict.Values;

            foreach (var group in groupList)
            {
                var monstersId = group.monsterGroups.Split(':');
                var spawnData = _worldMap.SpawnList.FirstOrDefault(x => x.spawnType == (int)WorldMap.ESpawnType.MONSTER &&
                                                                     x.groupId == group.groupId);

                if (spawnData == null)
                {
                    HSLogger.GetInstance().Error("spawnData is null or empty!");
                    continue;
                }

                var pivotIter = spawnData.pivotList.GetEnumerator();


                foreach (var id in monstersId)
                {
                    if (StaticData.Instance.MonstersDict.TryGetValue(id, out var data) == true &&
                        pivotIter.MoveNext() == true)
                    {
                        Vec3 pos = pivotIter.Current;

                        MonsterEntity entity = new MonsterEntity()
                        {
                            targetId = _mId++,
                            groupId = group.groupId,
                            monsterId = int.Parse(id),
                            pos = pos,
                            spawnPos = pos,
                            grade = data.grade,
                            broadcaster = _broadcaster,
                            stat = new EntityStat(data),
                            rewardDatas = data.rewardIds,
                        };

                        MonsterGroup monsterGroup = null;

                        if (_monsterGroupDict.TryGetValue(entity.groupId, out var mg) == true)
                        {
                            monsterGroup = mg;
                        }
                        else
                        {
                            monsterGroup = new MonsterGroup(entity.groupId, group.respawnTime);
                            _monsterGroupDict.Add(entity.groupId, monsterGroup);
                        }

                        monsterGroup.Add(entity);

                        var mData = new S_StartGame.Monster()
                        {
                            targetId = entity.targetId,
                            monsterId = entity.monsterId,
                            groupId = entity.groupId,
                            grade = entity.grade,
                            spawnPos = entity.spawnPos,
                        };

                        monsterList.Add(mData);

                    }
                }
            }

            return monsterList;
        }

        public List<S_StartGame.Player> SetupPlayer()
        {
            var playerList = new List<S_StartGame.Player>();

            var playerSpawn = _worldMap.SpawnList.FirstOrDefault(x => x.spawnType == (int)WorldMap.ESpawnType.PLAYER);

            if (playerSpawn == null)
            {
                HSLogger.GetInstance().Error("player spawn null or empty!");
                return null;
            }

            var pivotIter = playerSpawn.pivotList.Shuffle().GetEnumerator();

            foreach (var info in _playersDict.Values)
            {
                if (pivotIter.MoveNext() == true)
                {
                    var pos = pivotIter.Current;

                    var data = new S_StartGame.Player()
                    {
                        targetId = info.targetId,
                        heroId = info.heroId,
                        spawnPos = pos
                    };

                    playerList.Add(data);
                }
            }

            return playerList;
        }

        public PlayerEntity GetPlayerEntity(int inId)
        {
            if (_playersDict.TryGetValue(inId, out var player) && player != null)
            {
                return player;
            }

            HSLogger.GetInstance().Error("PlayerEntity is null or Empty");
            return null;
        }


        public MonsterEntity GetMonsterEntity(int inId)
        {
            var group = _monsterGroupDict.Values.FirstOrDefault(x => x.HaveEntityInGroup(inId));

            if (group == null)
            {
                HSLogger.GetInstance().Error("MonsterEntity is null or Empty");
                return null;
            }

            return group.GetMonsterEntity(inId);
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
                userId = playerInfo.userId,
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
                stat = player.stat.ConvertStat(),
            };

            _broadcaster.Broadcast(respawn);
        }


        //---------------
        // Receive
        //---------------

        public void OnRecvJoin(C_JoinToGame inPacket, int inSessionId)
        {
            var data = StaticData.Instance.HerosDict.Values.FirstOrDefault();

            if (data == null)
                return;

            var playerInfo = new PlayerEntity()
            {
                userId = inPacket.userId,
                targetId = inSessionId,
                heroId = data.key,
                userName = inPacket.userName,
                broadcaster = _broadcaster,
                stat = new EntityStat(data)
            };

            _playersDict.Add(inSessionId, playerInfo);

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

            if (inPacket.toIsPlayer == true)
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
                    toIsPlayer = toEntity.IsPlayer,
                    toId = inPacket.toId,
                    fromIsPlayer = true,
                    fromId = inPacket.fromId,
                    respawnTime = 15,
                };

                toEntity.OnDead();
                fromPlayer.OnIdle();

                _broadcaster.Broadcast(dead);

                // 죽은 대상이 몬스터라면
                if (inPacket.toIsPlayer == false)
                {
                    var monsterEntity = toEntity as MonsterEntity;
                    var worldItem = _worldMap.DropItem(monsterEntity.rewardDatas);

                    // worldItem이 null이라는 것은 확률결과 드랍을 못한것
                    if (worldItem == null)
                        return;

                    S_DropItem dropItem = new S_DropItem()
                    {
                        worldId  = worldItem.worldId,
                        itemData = worldItem.itemData,
                        pos      = monsterEntity.pos,
                    };

                    _broadcaster.Broadcast(dropItem);
                }
            }
            else
            {
                var attackParam = new AttackParam()
                {
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

            player.stat.AddMaxHp(inPacket.updateStat.str);
            player.stat.AddDef(inPacket.updateStat.def);
            player.stat.AddMaxHp(inPacket.updateStat.hp);
            player.stat.AddCurrHp(inPacket.updateStat.hp);

            _broadcaster.Broadcast(inPacket);
        }
    }
}