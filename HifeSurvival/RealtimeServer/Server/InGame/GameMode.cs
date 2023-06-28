using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class GameMode
    {
        private Dictionary<int, PlayerEntity> _playersDict = new Dictionary<int, PlayerEntity>();
        private Dictionary<int, MonsterGroup> _monsterGroupDict = new Dictionary<int, MonsterGroup>();

        private IBroadcaster _broadcaster = null;
        private WorldMap _worldMap;
        private int _mId = DEFINE.MONSTER_ID;

        public GameModeStatus Status { get; private set; } = GameModeStatus.None;

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
                    var spawnData = _worldMap.SpawnList.FirstOrDefault(x => x.spawnType == (int)WorldMapSpawnType.Monster &&
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

                            MonsterEntity monsterEntity = new MonsterEntity(monsterGroup, _worldMap.DropItem)
                            {
                                id = _mId++,
                                groupId = group.groupId,
                                monsterId = int.Parse(id),
                                currentPos = pos,
                                targetPos = pos,
                                spawnPos = pos,
                                grade = data.grade,
                                broadcaster = _broadcaster,
                                defaultStat = new EntityStat(data),
                                rewardDatas = data.rewardIds,
                            };

                            monsterGroup.Add(monsterEntity);

                            var mData = new MonsterSpawn()
                            {
                                id = monsterEntity.id,
                                monstersKey = monsterEntity.monsterId,
                                groupId = monsterEntity.groupId,
                                grade = monsterEntity.grade,
                                pos = monsterEntity.spawnPos,
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
            var playerSpawn = _worldMap.SpawnList.FirstOrDefault(x => x.spawnType == (int)WorldMapSpawnType.Player);
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
                        id = info.id,
                        herosKey = info.heroKey,
                        pos = pos
                    };

                    playerList.Add(data);
                }
            }

            return playerList;
        }


        public Entity GetEntityById(int id)
        {
            // monster id 이하보다 작으면 플레이어라고 상정.
            if (id < DEFINE.MONSTER_ID)
            {
                if (_playersDict.TryGetValue(id, out var player) && player != null)
                {
                    return player;
                }
                Logger.GetInstance().Error("PlayerEntity is null or Empty");
            }
            else
            {
                var group = _monsterGroupDict.Values.FirstOrDefault(x => x.HaveEntityInGroup(id));
                if (group == null)
                {
                    Logger.GetInstance().Error("MonsterGroup is null or Empty");
                    return null;
                }

                var monster = group.GetMonsterEntity(id);
                if(monster == null)
                {
                    Logger.GetInstance().Error("MonsterEntity is null or Empty");
                    return null;
                }

                return monster;
            }

            return null;
        }

        public bool CanStartGame()
        {
            return _playersDict.Values.All(x => x.isReady);
        }

        public bool CanJoinRoom()
        {
            return Status == GameModeStatus.Ready && _playersDict.Count < DEFINE.PLAYER_MAX_COUNT;
        }

        private void OnModeStatusChange()
        {
            switch (Status)
            {
                case GameModeStatus.GameStart:
                    {
                        _monsterGroupDict.AsParallel().ForAll(mg => mg.Value.OnEnterGame());
                        _playersDict.AsParallel().ForAll(p => p.Value.UpdateStat());
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
            var playerInfo = _playersDict.Values.FirstOrDefault(x => x.id == inSessionId);

            if (playerInfo == null)
                return;

            _playersDict.Remove(playerInfo.id);

            S_LeaveToGame packet = new S_LeaveToGame()
            {
                userId = playerInfo.userId,
                id = playerInfo.id,
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

            Status = GameModeStatus.GameStart;
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

            Status = GameModeStatus.Countdown;

            JobTimer.Instance.Push(SendStartGame, DEFINE.START_COUNTDOWN_SEC * DEFINE.SEC_TO_MS);
        }


        public void OnSendRespawn(int id)
        {
            var player = GetEntityById(id);
            if (player == null)
                return;

            S_Respawn respawn = new S_Respawn()
            {
                id = id,
                stat = player.stat.ConvertToPStat(),
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

            var playerEntity = new PlayerEntity()
            {
                userId = inPacket.userId,
                id = inSessionId,
                heroKey = data.key,
                userName = inPacket.userName,
                broadcaster = _broadcaster,
                defaultStat = new EntityStat(data)
            };

            _playersDict.Add(inSessionId, playerEntity);

            S_JoinToGame packet = new S_JoinToGame();
            packet.joinPlayerList = new List<S_JoinToGame.JoinPlayer>();

            foreach (var player in _playersDict.Values)
                packet.joinPlayerList.Add(player.CreateJoinPlayerPacket());

            _broadcaster.Broadcast(packet);

            Status = GameModeStatus.Ready;
        }


        public void OnRecvReady(CS_ReadyToGame inPacket)
        {
            var entity = GetEntityById(inPacket.id);
            if (entity == null)
                return;

            if(entity is PlayerEntity player)
            {
                player.isReady = true;
                _broadcaster.Broadcast(inPacket);

                // 모두 레디라면..? 게임시작
                if (CanStartGame() == true)
                    OnSendCountDown();
            }
        }


        public void OnRecvSelect(CS_SelectHero inPacket)
        {
            var entity = GetEntityById(inPacket.id);
            if (entity == null)
                return;

            if (entity is PlayerEntity player)
            {
                player.heroKey = inPacket.heroKey;
            }

            _broadcaster.Broadcast(inPacket);
        }

        public void OnRecvMoveRequest(MoveRequest inPacket)
        {
            var player = GetEntityById(inPacket.id);
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
            var fromPlayer = GetEntityById(inPacket.id);
            if (fromPlayer == null)
                return;

            var targetEntity = GetEntityById(inPacket.targetId);
            if (targetEntity == null)
                return;

            fromPlayer.Attack(new AttackParam()
            {
                target = targetEntity,
            });
        }


        public void OnRecvIncreaseStatRequest(IncreaseStatRequest inPacket)
        {
            var entity = GetEntityById(inPacket.id);
            if (entity == null)
                return;

            var res = new IncreaseStatResponse();
            res.id = entity.id;
            res.result = DEFINE.SUCCESS;

            int  increaseValue = inPacket.increase;
            if (entity is PlayerEntity  playerEntity)
            {
                //NOTE : 현재 1골드 당 해당 스탯 1 증가. 
                //TODO : 골드량 대 스탯 증가량 비례 값은 데이터 시트로 관리할 예정. 
                if (increaseValue > playerEntity.gold)
                {
                    res.result = DEFINE.ERROR;
                    _broadcaster.Broadcast(res);
                    return;
                }

                playerEntity.gold -= increaseValue;

                switch ((StatType)inPacket.type)
                {
                    case StatType.Str:
                        playerEntity.upgradeStat.AddStr(increaseValue);
                        break;
                    case StatType.Def:
                        playerEntity.upgradeStat.AddDef(increaseValue);
                        break;
                    case StatType.Hp:
                        playerEntity.upgradeStat.AddMaxHp(increaseValue, true);
                        break;
                    default:
                        Logger.GetInstance().Error($"Wrong Stat Type {(StatType)inPacket.type}");
                        break; ;
                }

                res.increase = increaseValue;
                res.usedGold = increaseValue;
            }

            entity.UpdateStat();

            entity.GetStat(out var originEStat, out var addEStat);
            res.originStat = originEStat.ConvertToPStat();
            res.addStat = addEStat.ConvertToPStat();


            _broadcaster.Broadcast(res);
            //TODO : Send IncreaseStatResponse + result 값 처리
        }


        public void OnRecvPickRewardRequest(PickRewardRequest req)
        {
            var entity = GetEntityById(req.id);
            if (entity == null)
                return;

            var worldId = req.worldId;

            var res = new PickRewardResponse();
            res.id = entity.id;
            res.worldId = worldId;

            if (entity is PlayerEntity player)
            {
                var rewardData = _worldMap.PickReward(worldId);
                if (rewardData.Equals(default(RewardData)))
                {
                    Logger.GetInstance().Warn($"no reward in world id {worldId}");
                    return;
                }

                var broadcast = new UpdateRewardBroadcast();
                broadcast.worldId = worldId;
                broadcast.status = (int)RewardState.Pick;
                broadcast.rewardType = rewardData.rewardType;

                switch ((RewardType)rewardData.rewardType)
                {
                    case RewardType.Gold:
                        {
                            res.gold = broadcast.gold = rewardData.count;
                            break;
                        }
                    case RewardType.Item:
                        {
                            // Item Layer 나누기. (PItem , GameDataItem, InvenItem, PInvenItem)
                            var toEquipItem = new PItem()
                            {
                                //NOTE : 임시 값. 
                                itemKey = rewardData.subType,
                                level = 1,
                                str = 999,
                                def = 999,
                                hp = 999,
                                cooltime = 12,
                                canUse = true
                            };
                            res.item = broadcast.item = toEquipItem;

                            int slot = player.EquipItem(toEquipItem);
                            if (slot > 0)
                                res.itemSlotId = slot;
                            break;
                        }
                    default:
                        break;
                }

                _broadcaster.Broadcast(broadcast);
                _broadcaster.Broadcast(res);         //TODO : Send PickRewardResponse
            }
        }
    }
}