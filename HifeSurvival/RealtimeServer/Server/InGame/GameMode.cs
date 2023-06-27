using Server.GameData;
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

        private Dictionary<int, PlayerEntity> _playersDict = new Dictionary<int, PlayerEntity>();
        private Dictionary<int, MonsterGroup> _monsterGroupDict = new Dictionary<int, MonsterGroup>();

        private IBroadcaster _broadcaster = null;
        private WorldMap _worldMap;
        private int _mId = 10000;

        public EStatus Status { get; private set; } = EStatus.NONE;

        public GameMode(GameRoom inRoom)
        {
            _broadcaster = new RoomBroadcaster(inRoom);
            _worldMap = new WorldMap(_broadcaster);
        }


        public List<MonsterSpawn> SpawnMonster(string[] inGroupKeyArr)
        {
            var monsterList = new List<MonsterSpawn>();

            foreach (var groupKey in inGroupKeyArr)
            {
                if (GameDataLoader.Instance.MonstersGroupDict.TryGetValue(groupKey, out var group) == true)
                {
                    var monsterKey = group.monsterGroups.Split(':');
                    var spawnData = _worldMap.SpawnList.FirstOrDefault(x => x.spawnType == (int)WorldMap.ESpawnType.MONSTER &&
                                                                         x.groupId == group.groupId);

                    if (spawnData == null)
                    {
                        Logger.GetInstance().Error("spawnData is null or empty!");
                        continue;
                    }

                    var pivotIter = spawnData.pivotList.GetEnumerator();


                    foreach (var id in monsterKey)
                    {
                        if (GameDataLoader.Instance.MonstersDict.TryGetValue(id, out var data) == true &&
                            pivotIter.MoveNext() == true)
                        {
                            PVec3 pos = pivotIter.Current;

                            MonsterGroup monsterGroup = null;

                            if (_monsterGroupDict.TryGetValue(group.groupId, out var mg) == true)
                            {
                                monsterGroup = mg;
                            }
                            else
                            {
                                monsterGroup = new MonsterGroup(group.groupId, group.respawnTime);
                                _monsterGroupDict.Add(group.groupId, monsterGroup);
                            }

                            MonsterEntity entity = new MonsterEntity(monsterGroup, _worldMap.DropItem)
                            {
                                targetId = _mId++,
                                groupId = group.groupId,
                                monsterId = int.Parse(id),
                                currentPos = pos,
                                targetPos = pos,
                                spawnPos = pos,
                                grade = data.grade,
                                broadcaster = _broadcaster,
                                stat = new EntityStat(data),
                                rewardDatas = data.rewardIds,
                            };

                            monsterGroup.Add(entity);

                            var mData = new MonsterSpawn()
                            {
                                targetId = entity.targetId,
                                monstersKey = entity.monsterId,
                                groupId = entity.groupId,
                                grade = entity.grade,
                                pos = entity.spawnPos,
                            };

                            monsterList.Add(mData);
                        }
                    }
                }
            }

            return monsterList;
        }

        public List<PlayerSpawn> SpawnPlayer()
        {
            var playerList = new List<PlayerSpawn>();

            var playerSpawn = _worldMap.SpawnList.FirstOrDefault(x => x.spawnType == (int)WorldMap.ESpawnType.PLAYER);

            if (playerSpawn == null)
            {
                Logger.GetInstance().Error("player spawn null or empty!");
                return null;
            }

            var pivotIter = playerSpawn.pivotList.Shuffle().GetEnumerator();

            foreach (var info in _playersDict.Values)
            {
                if (pivotIter.MoveNext() == true)
                {
                    var pos = pivotIter.Current;

                    var data = new PlayerSpawn()
                    {
                        targetId = info.targetId,
                        herosKey = info.heroId,
                        pos = pos
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

            Logger.GetInstance().Error("PlayerEntity is null or Empty");
            return null;
        }


        public MonsterEntity GetMonsterEntity(int inId)
        {
            var group = _monsterGroupDict.Values.FirstOrDefault(x => x.HaveEntityInGroup(inId));

            if (group == null)
            {
                Logger.GetInstance().Error("MonsterEntity is null or Empty");
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
            return Status == EStatus.READY && _playersDict.Count < DEFINE.PLAYER_MAX_COUNT;
        }

        private void OnModeStatusChange()
        {
            switch(Status)
            {
                case EStatus.GAME_START:
                    {
                        _monsterGroupDict.AsParallel().ForAll(mg => {
                            foreach (var monster in mg.Value.GetMonsterGroupIter())
                                monster.Value.ExecuteAI();
                        });
                    }
                    break;
                default:
                    break;
            }
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


        public void SendStartGame()
        {
            // TODO@taeho.kang 후에 나중에
            if (GameDataLoader.Instance.ChapaterDataDict.TryGetValue("1", out var chapterData) == false)
            {
                Logger.GetInstance().Error("chapterdata is not found");
                return;
            }

            // 맵 로드
            _worldMap.LoadMap(chapterData.mapData);

            // 플레이어 몬스터 스폰
            var playerSpawnList = SpawnPlayer();

            S_StartGame gameStart = new S_StartGame()
            {
                playTimeSec = chapterData.playTimeSec,
                playerList = playerSpawnList,
                monsterList = SpawnMonster(chapterData.phase1.Split(':'))
            };

            _broadcaster.Broadcast(gameStart);

            // TODO@taeho.kang 나중에 더 좋은 방법이 있으면 수정
            // 몬스터 스폰
            // SpawnMonsterTimer(chapterData.phase1, 0);
            //SpawnMonsterTimer(chapterData.phase2, 60);
            //SpawnMonsterTimer(chapterData.phase3, 120);
            //SpawnMonsterTimer(chapterData.phase4, 300);

            // 게임종료 타이머도 추가해야함.

            Status = EStatus.GAME_START;
            OnModeStatusChange();
        }

        public void SpawnMonsterTimer(string inPhase, int inSec)
        {
            if (inPhase == null)
                return;

            JobTimer.Instance.Push(() =>
            {
                S_SpawnMonster spawnMoster = new S_SpawnMonster()
                {
                    monsterList = SpawnMonster(inPhase.Split(':'))
                };

                _broadcaster.Broadcast(spawnMoster);
            }, inSec * DEFINE.SEC_TO_MS);
        }

        public void OnSendCountDown()
        {
            S_Countdown countdown = new S_Countdown()
            {
                countdownSec = DEFINE.START_COUNTDOWN_SEC
            };

            _broadcaster.Broadcast(countdown);

            Status = EStatus.COUNTDOWN;

            JobTimer.Instance.Push(SendStartGame, DEFINE.START_COUNTDOWN_SEC * DEFINE.SEC_TO_MS);
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
            var data = GameDataLoader.Instance.HerosDict.Values.FirstOrDefault();

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

        public void OnRecvMoveRequest(MoveRequest inPacket)
        {
            var player = GetPlayerEntity(inPacket.targetId);
            if (player == null)
                return;

            player.currentPos = inPacket.currentPos;
            player.targetPos = inPacket.targetPos;

            if(player.currentPos.IsSame(player.targetPos))
            {
                player.MoveStop(new IdleParam()
                {
                    currentPos = inPacket.currentPos,
                    timestamp = inPacket.timestamp
                } );
            }
            else
            {
                player.Move(new MoveParam()
                {
                    currentPos = inPacket.currentPos,
                    targetPos = inPacket.targetPos,
                    speed = inPacket.speed,
                    timestamp = inPacket.timestamp
                });
            }
        }


        public void OnRecvAttack(CS_Attack inPacket)
        {
            var fromPlayer = GetPlayerEntity(inPacket.fromId);
            if (fromPlayer == null)
                return;

            Entity toEntity = null;
            if (inPacket.toIsPlayer)
                toEntity = GetPlayerEntity(inPacket.toId);
            else
                toEntity = GetMonsterEntity(inPacket.toId);

            if (toEntity == null)
                return;

            fromPlayer.Attack(new AttackParam()
            {
                target = toEntity,
            });
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


        public void OnRecvPickReward(C_PickReward inPacket)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            var rewardData = _worldMap.PickReward(inPacket.worldId);

            IPacket packet = null;

            switch ((RewardData.ERewardType)rewardData.rewardType)
            {
                case RewardData.ERewardType.GOLD:

                    packet = new S_GetGold()
                    {
                        targetId = inPacket.targetId,
                        worldId = inPacket.worldId,
                        gold = rewardData.count,
                    };

                    break;

                case RewardData.ERewardType.ITEM:

                    packet = new S_GetItem()
                    {
                        targetId = inPacket.targetId,
                        worldId = inPacket.worldId,
                        // itemSlotId = 1,
                        item = new Item()
                        {

                        }
                    };

                    break;
            }


            _broadcaster.Broadcast(packet);
        }
    }
}