using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class GameMode
    {
        private Dictionary<int, PlayerEntity> _playersDict = new Dictionary<int, PlayerEntity>();
        private Dictionary<int, MonsterGroup> _monsterGroupDict = new Dictionary<int, MonsterGroup>();

        private WorldMap _worldMap;
        private int _mId = DEFINE.MONSTER_ID;

        private GameRoom _room;

        public EGameModeStatus Status { get; private set; } = EGameModeStatus.NONE;

        public GameMode(GameRoom room)
        {
            _worldMap = new WorldMap();
            _room = room;
        }

        public void OnSessionRemove(int sessionId)
        {
            var playerInfo = _playersDict.Values.FirstOrDefault(x => x.id == sessionId);
            if (playerInfo == null)
            {
                return;
            }

            _playersDict.Remove(playerInfo.id);

            S_LeaveToGame packet = new S_LeaveToGame()
            {
                userId = playerInfo.userId,
                id = playerInfo.id,
            };

            _room.Broadcast(packet);
        }

        private void UpdateModeStatus(EGameModeStatus updatedStatus)
        {
            Logger.Instance.Warn($"<-----{updatedStatus}----->");

            switch (updatedStatus)
            {
                case EGameModeStatus.READY:
                    {
                        var packet = new S_JoinToGame() { joinPlayerList = new List<S_JoinToGame.JoinPlayer>() };
                        _playersDict.AsParallel().ForAll(p => packet.joinPlayerList.Add(new S_JoinToGame.JoinPlayer()
                        {
                            userId = p.Value.userId,
                            userName = p.Value.userName,
                            id = p.Value.id,
                            heroKey = p.Value.heroKey,
                        }));

                        _room.Broadcast(packet);
                    }
                    break;
                case EGameModeStatus.COUNT_DOWN:
                    {
                        var countdown = new S_Countdown()
                        {
                            countdownSec = DEFINE.START_COUNTDOWN_SEC
                        };

                        _room.Broadcast(countdown);

                        JobTimer.Instance.Push(StartLoadGame, DEFINE.START_COUNTDOWN_SEC * DEFINE.SEC_TO_MS);
                    }
                    break;
                case EGameModeStatus.LOAD_GAME:
                    {
                        int tempChapterKey = 1;
                        if (GameData.Instance.ChapaterDataDict.TryGetValue(tempChapterKey, out var chapterData) == false)
                        {
                            Logger.Instance.Error("chapterdata is not found");
                            return;
                        }

                        _worldMap.LoadMap(chapterData.mapData);

                        var gameStart = new S_StartGame()
                        {
                            playTimeSec = chapterData.playTimeSec,
                            playerList = SpawnPlayer(),
                            monsterList = SpawnMonster(chapterData.phase1Array)
                        };

                        _room.Broadcast(gameStart);

                        JobTimer.Instance.Push(() =>
                        {
                            _monsterGroupDict.AsParallel().ForAll(mg => mg.Value.UpdateStat());
                            _playersDict.AsParallel().ForAll(p => p.Value.UpdateStat());
                        });

                        SpawnPhaseRegist(tempChapterKey);
                    }
                    break;
                case EGameModeStatus.PLAY_START:
                    {
                        JobTimer.Instance.Push(() =>
                        {
                            _monsterGroupDict.AsParallel().ForAll(mg => mg.Value.OnPlayStart());
                        });
                    }
                    break;
                case EGameModeStatus.FINISH_GAME:
                    {

                    }
                    break;
                default:
                    break;
            }

            if (Status != updatedStatus)
            {
                _room.Broadcast(new UpdateGameModeStatusBroadcast()
                {
                    status = (int)updatedStatus
                });
            }

            Status = updatedStatus;
        }

        public List<MonsterSpawn> SpawnMonster(int[] groupKeyArr)
        {
            var monsterList = new List<MonsterSpawn>();

            foreach (var groupKey in groupKeyArr)
            {
                if (GameData.Instance.MonstersGroupDict.TryGetValue(groupKey, out var group) == true)
                {
                    var monsterKey = group.monsterGroups.Split(':');
                    var spawnData = _worldMap.SpawnList.AsQueryable().FirstOrDefault(x => x.spawnType == (int)EWorldMapSpawnType.MONSTER &&
                                                                         x.groupId == group.groupId);

                    if (spawnData == null)
                    {
                        Logger.Instance.Error("spawnData is null or empty!");
                        continue;
                    }

                    var pivotIter = spawnData.pivotList.GetEnumerator();

                    foreach (var id in monsterKey)
                    {
                        if (GameData.Instance.MonstersDict.TryGetValue(int.Parse(id)/*temp*/, out var data) == true &&
                            pivotIter.MoveNext() == true)
                        {
                            var pos = pivotIter.Current;

                            MonsterGroup monsterGroup = null;

                            if (_monsterGroupDict.TryGetValue(group.groupId, out var mg) == true)
                            {
                                monsterGroup = mg;
                            }
                            else
                            {
                                monsterGroup = new MonsterGroup(group.groupId, group.respawnTimeSec);
                                _monsterGroupDict.Add(group.groupId, monsterGroup);
                            }

                            Logger.Instance.Debug($"monster id {id}, reward id {data.rewardIds}");
                            var monsterEntity = new MonsterEntity(_room, monsterGroup, _worldMap, data)
                            {
                                id = _mId++,
                                groupId = group.groupId,
                                monsterId = int.Parse(id),
                                currentPos = pos,
                                targetPos = pos,
                                spawnPos = pos,
                                grade = data.grade,
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

        public void SpawnPhaseRegist(int chapterKey)
        {
            if (!GameData.Instance.ChapaterDataDict.TryGetValue(chapterKey, out var phaseData))
            {
                return;
            }

            for (int phase_idx = 1 ; phase_idx <= DEFINE.SPAWN_PHASE_MAX ; phase_idx++)
            {
                (int[] phaseArr, int ms) dataByPhase = phase_idx switch
                {
                    //1 => (phaseData.phase1Array, phaseData.phaseSecArray[phase_idx -1]), -> StartGame 에서 처리 함.
                    2 => (phaseData.phase2Array, phaseData.phaseSecArray[phase_idx -1]),
                    3 => (phaseData.phase3Array, phaseData.phaseSecArray[phase_idx -1]),
                    4 => (phaseData.phase4Array, phaseData.phaseSecArray[phase_idx -1]),
                    _ => (null, 0)
                };

                if(dataByPhase.phaseArr == null)
                {
                    continue;
                }

                JobTimer.Instance.Push(() =>
                {
                    var spawnMoster = new UpdateSpawnMonsterBroadcast()
                    {
                        monsterList = SpawnMonster(dataByPhase.phaseArr)
                    };

                    _room.Broadcast(spawnMoster);
                }, dataByPhase.ms);
            }
        }

        public List<PlayerSpawn> SpawnPlayer()
        {
            var playerList = new List<PlayerSpawn>();
            var playerSpawn = _worldMap.SpawnList.FirstOrDefault(x => x.spawnType == (int)EWorldMapSpawnType.PLAYER);
            if (playerSpawn == null)
            {
                Logger.Instance.Error("player spawn null or empty!");
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
                return GetPlayerEntityById(id);
            }
            else
            {
                return GetMonsterEntityById(id);
            }
        }

        public PlayerEntity GetPlayerEntityById(int id)
        {
            if (_playersDict.TryGetValue(id, out var player) && player != null)
            {
                return player;
            }

            Logger.Instance.Error($"PlayerEntity is null or Empty id {id}");
            return null;
        }

        public MonsterEntity GetMonsterEntityById(int id)
        {
            var group = _monsterGroupDict.Values.FirstOrDefault(x => x.HaveEntityInGroup(id));
            if (group == null)
            {
                Logger.Instance.Error("MonsterGroup is null or Empty");
                return null;
            }

            var monster = group.GetMonsterEntity(id);
            if (monster == null)
            {
                Logger.Instance.Error("MonsterEntity is null or Empty");
                return null;
            }

            return monster;
        }

        public bool CanJoinRoom()
        {
            return Status == EGameModeStatus.READY && _playersDict.Count < DEFINE.PLAYER_MAX_COUNT;
        }

        public bool CanLoadGame()
        {
            return _playersDict.Values.All(x => x.clientStatus == EClientStatus.SELECT_READY);
        }

        public bool CanPlayStart()
        {
            return _playersDict.Values.All(x => x.clientStatus == EClientStatus.PLAY_READY);
        }

        public void StartLoadGame()
        {
            UpdateModeStatus(EGameModeStatus.LOAD_GAME);
        }

        public void OnRecvJoin(C_JoinToGame req, int sessionId)
        {
            var data = GameData.Instance.HerosDict.Values.FirstOrDefault();
            if (data == null)
            {
                return;
            }

            var playerEntity = new PlayerEntity(_room, data)
            {
                userId = req.userId,
                id = sessionId,
                heroKey = data.key,
                userName = req.userName,
            };

            _playersDict.Add(sessionId, playerEntity);

            UpdateModeStatus(EGameModeStatus.READY);
        }

        public void OnRecvSelect(CS_SelectHero req)
        {
            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            player.heroKey = req.heroKey;
            _room.Broadcast(req);
        }

        public void OnRecvReady(CS_ReadyToGame req)
        {
            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            player.SelectReady();
            _room.Broadcast(req);

            if (CanLoadGame())
            {
                UpdateModeStatus(EGameModeStatus.COUNT_DOWN);
            }
        }

        public void OnPlayStartRequest(PlayStartRequest req)
        {
            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            player.PlayReady();
            _room.Send(player.id, new PlayStartResponse() { id = player.id });

            if (CanPlayStart())
            {
                UpdateModeStatus(EGameModeStatus.PLAY_START);
            }
        }

        public void OnRecvMoveRequest(MoveRequest req)
        {
            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            player.currentPos = req.currentPos;
            player.targetPos = req.targetPos;

            if (player.currentPos.IsSame(player.targetPos))
            {
                player.MoveStop(new IdleParam()
                {
                    currentPos = req.currentPos,
                    timestamp = req.timestamp
                });
            }
            else
            {
                player.Move(new MoveParam()
                {
                    currentPos = req.currentPos,
                    targetPos = req.targetPos,
                    speed = req.speed,
                    timestamp = req.timestamp
                });
            }
        }


        public void OnRecvAttack(CS_Attack req)
        {
            var fromPlayer = GetPlayerEntityById(req.id);
            if (fromPlayer == null)
            {
                return;
            }

            var targetEntity = GetEntityById(req.targetId);
            if (targetEntity == null)
            {
                return;
            }

            fromPlayer.Attack(new AttackParam()
            {
                target = targetEntity,
            });
        }


        public void OnRecvIncreaseStatRequest(IncreaseStatRequest req)
        {
            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            var res = new IncreaseStatResponse();
            res.id = player.id;

            int increaseValue = req.increase;
            //NOTE : 현재 1골드 당 해당 스탯 1 증가. 
            //TODO : 골드량 대 스탯 증가량 비례 값은 데이터 시트로 관리할 예정. 
            int currentGold = player.Inventory.GetCurrencyByType(ECurrency.GOLD);
            if (currentGold < increaseValue)
            {
                res.result = DEFINE.ERROR;
                _room.Send(req.id, res);
                return;
            }

            player.Inventory.SpendCurrency(ECurrency.GOLD, increaseValue);

            switch ((EStatType)req.type)
            {
                case EStatType.STR:
                    player.upgradeStat.AddStr(increaseValue);
                    break;
                case EStatType.DEF:
                    player.upgradeStat.AddDef(increaseValue);
                    break;
                case EStatType.HP:
                    player.upgradeStat.AddMaxHp(increaseValue, true);
                    break;
                default:
                    Logger.Instance.Error($"Wrong Stat Type {(EStatType)req.type}");
                    break; ;
            }

            res.increase = increaseValue;
            res.usedGold = increaseValue;

            player.UpdateStat();

            _room.Send(player.id, res);
        }


        public void OnRecvPickRewardRequest(PickRewardRequest req)
        {
            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            var worldId = req.worldId;

            var res = new PickRewardResponse();
            res.id = player.id;
            res.worldId = worldId;

            var rewardData = _worldMap.PickReward(worldId);
            if (rewardData.Equals(default(RewardData)))
            {
                Logger.Instance.Warn($"no reward in world id {worldId}");
                return;
            }

            var rewardBroadcast = new UpdateRewardBroadcast();
            rewardBroadcast.worldId = worldId;
            rewardBroadcast.status = (int)ERewardState.PICK;
            rewardBroadcast.rewardType = rewardData.rewardType;
            res.rewardType = rewardData.rewardType;

            switch ((ERewardType)rewardData.rewardType)
            {
                case ERewardType.GOLD:
                    {
                        var amount = rewardData.count;
                        res.gold = rewardBroadcast.gold = amount;
                        player.Inventory.EarnCurrency(ECurrency.GOLD, amount);
                        break;
                    }
                case ERewardType.ITEM:
                    {
                        if (GameData.Instance.ItemDict.TryGetValue(rewardData.subType, out var mastItem))
                        {
                            var toEquipItem = new PDropItem()
                            {
                                itemKey = mastItem.key,
                            };

                            res.item = rewardBroadcast.item = toEquipItem;

                            int slot = player.EquipItem(mastItem);
                            if (slot < 0)
                            {
                                Logger.Instance.Warn("Pick -> Equip Failed");
                            }
                        }
                        break;
                    }
                default:
                    break;
            }

            _room.Broadcast(rewardBroadcast);
            _room.Send(player.id, res);
            player.UpdateStat();
        }


        public void OnCheatRequest(int id, CheatRequest req)
        {
            var player = GetPlayerEntityById(id);
            if (player == null)
            {
                return;
            }

            var result = player.CheatExecuter.Execute(req);
            _room.Send(player.id, new CheatResponse()
            {
                result = result ? DEFINE.SUCCESS : DEFINE.ERROR,
            });
        }
    }
}