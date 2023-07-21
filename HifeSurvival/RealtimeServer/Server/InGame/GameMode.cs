using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class GameMode
    {
        private int _winner_id;
        private Dictionary<int, PlayerEntity> _playersDict = new Dictionary<int, PlayerEntity>();
        private Dictionary<int, MonsterGroup> _monsterGroupDict = new Dictionary<int, MonsterGroup>();

        private WorldMap _worldMap;
        private int _mId = 0;

        private GameRoom _room;

        public EGameModeStatus Status { get; private set; } = EGameModeStatus.NONE;

        public GameMode(GameRoom room)
        {
            _worldMap = new WorldMap();
            _room = room;
        }

        public void OnSessionRemove(int sessionId)
        {
            var playerInfo = _playersDict.Values.FirstOrDefault(x => x.ID == sessionId);
            if (playerInfo == null)
            {
                return;
            }

            _playersDict.Remove(playerInfo.ID);

            var packet = new S_LeaveToGame()
            {
                id = playerInfo.ID,
            };

            _room.Broadcast(packet);
        }

        private void UpdateModeStatus(EGameModeStatus updatedStatus)
        {
            Logger.Instance.Warn($"<-----{updatedStatus}----->");

            int param1 = 0;

            switch (updatedStatus)
            {
                case EGameModeStatus.READY:
                    {
                        var packet = new S_JoinToGame() { joinPlayerList = new List<S_JoinToGame.JoinPlayer>() };
                        _playersDict.ToList().ForEach(p => packet.joinPlayerList.Add( p.Value.MakeJoinPlayer()));

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

                        int playTimeSec = chapterData.playTimeSec;
                        var gameStart = new S_StartGame()
                        {
                            playTimeSec = playTimeSec,
                            playerList = SpawnPlayer(),
                            monsterList = SpawnMonster(chapterData.phase1GkeyArr)
                        };

                        _room.Broadcast(gameStart);

                        JobTimer.Instance.Push(() =>
                        {
                            _monsterGroupDict.ToList().ForEach(mg => mg.Value.UpdateStat());
                            _playersDict.ToList().ForEach(p => p.Value.UpdateStat());
                        });

                        SpawnPhaseRegist(tempChapterKey);
                    }
                    break;
                case EGameModeStatus.PLAY_START:
                    {
                        JobTimer.Instance.Push(() =>
                        {
                            _monsterGroupDict.ToList().ForEach(mg => mg.Value.OnPlayStart());
                            _playersDict.ToList().ForEach(p => p.Value.ClientStatus = EClientStatus.PLAYING);
                        });

                        int tempChapterKey = 1;
                        if (GameData.Instance.ChapaterDataDict.TryGetValue(tempChapterKey, out var chapterData) == false)
                        {
                            Logger.Instance.Error("chapterdata is not found");
                            return;
                        }

                        int playTimeSec = chapterData.playTimeSec;

                        JobTimer.Instance.Push(() =>
                        {
                            UpdateModeStatus(EGameModeStatus.PLAY_FINISH);
                        }, playTimeSec * DEFINE.SEC_TO_MS);
                    }
                    break;
                case EGameModeStatus.PLAY_FINISH:
                    {
                        JobTimer.Instance.Push(() =>
                        {
                            _playersDict.ToList().ForEach(p => p.Value.TerminateGamePlayer());
                            _monsterGroupDict.Clear();
                        });

                        UpdateModeStatus(EGameModeStatus.FINISH_GAME);
                    }
                    break;
                case EGameModeStatus.FINISH_GAME:
                    {
                        param1 = _winner_id;

                        JobTimer.Instance.Push(() =>
                        {
                            GameRoomManager.Instance.TerminateRoom(_room.RoomId);
                        });
                    }
                    break;
                default:
                    break;
            }

            if (Status != updatedStatus)
            {
                _room.Broadcast(new UpdateGameModeStatusBroadcast()
                {
                    status = (int)updatedStatus,
                    param1 = param1
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
                    var spawnData = _worldMap.SpawnList.AsQueryable()
                        .Where(x => x.spawnType == (int)EWorldMapSpawnType.MONSTER && x.groupId == group.groupId)
                        .FirstOrDefault();

                    if (spawnData == null)
                    {
                        Logger.Instance.Error("spawnData is null or empty!");
                        continue;
                    }

                    var pivotIter = spawnData.pivotList.GetEnumerator();
                    Logger.Instance.Info($"Spawn Pivot Count : {spawnData.pivotList.Count}");

                    foreach (var key in monsterKey)
                    {
                        if (!GameData.Instance.MonstersDict.TryGetValue(int.Parse(key)/*temp*/, out var data))
                        {
                            Logger.Instance.Error($"Spawn Monster Key is Invalid - key {key}");
                            continue;
                        }

                        if(!pivotIter.MoveNext())
                        {
                            Logger.Instance.Warn($"Can't Spawn ,,, Not Enough Pivot");
                            continue;
                        }

                        var pos = pivotIter.Current;
                        int monsterId = _mId++;

                        if(DEFINE.PC_BEGIN_ID <= _mId)
                        {
                            _mId = 0;
                        }

                        if (!_monsterGroupDict.TryGetValue(group.groupId, out var monsterGroup))
                        {
                            monsterGroup = new MonsterGroup(group.groupId, group.respawnTimeSec);
                            _monsterGroupDict.Add(group.groupId, monsterGroup);
                        }

                        Logger.Instance.Debug($"monster key {key}, reward id {data.rewardIds}");
                        var monsterEntity = new MonsterEntity(_room, monsterId, monsterGroup, data, pos);
                        monsterGroup.Add(monsterEntity);

                        monsterList.Add(monsterEntity.MakeSpawnData());
                    }
                }
            }

            return monsterList;
        }

        //NOTE : drop 된 reason 기록하기. 
        public UpdateRewardBroadcast DropItem(string rewardIds, PVec3 dropPos)
        {
            return _worldMap.DropItem(rewardIds, dropPos);
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
                    2 => (phaseData.phase2GkeyArr, phaseData.phaseSecArray[phase_idx -1]),
                    3 => (phaseData.phase3GkeyArr, phaseData.phaseSecArray[phase_idx -1]),
                    4 => (phaseData.phase4GkeyArr, phaseData.phaseSecArray[phase_idx -1]),
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

                    dataByPhase.phaseArr.ToList().ForEach(k =>
                   {
                       if (GameData.Instance.MonstersGroupDict.TryGetValue(k, out var mgdata))
                       {
                           var monsterList = _monsterGroupDict.AsQueryable().Where(mg => mgdata.groupId == mg.Key).ToList();

                           monsterList.ForEach(smg => smg.Value.UpdateStat());
                           monsterList.ForEach(smg => smg.Value.OnPlayStart());
                       }
                   });

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
            foreach (var player in _playersDict.Values)
            {
                if (!pivotIter.MoveNext())
                {
                    continue;
                }

                var pos = pivotIter.Current;
                player.InitGamePlayer(pos);

                playerList.Add(player.MakePlayerSpawn());
            }

            return playerList;
        }

        public void SetWinnerID(int id)
        {
            if(_winner_id > 0)
            {
                Logger.Instance.Error($"Already Winner Set ! {_winner_id}, try set {id}");
                return;
            }

            _winner_id = id;
            UpdateModeStatus(EGameModeStatus.PLAY_FINISH);
            return;
        }

        public Entity GetEntityById(int id)
        {
            // PC_BEGIN_ID 작으면 몬스터라고 상정.
            if (id < DEFINE.PC_BEGIN_ID)
            {
                return GetMonsterEntityById(id);
            }
            else
            {
                return GetPlayerEntityById(id);
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
            return _playersDict.Values.All(x => x.ClientStatus == EClientStatus.SELECT_READY);
        }

        public bool CanPlayStart()
        {
            return _playersDict.Values.All(x => x.ClientStatus == EClientStatus.PLAY_READY);
        }

        public void StartLoadGame()
        {
            UpdateModeStatus(EGameModeStatus.LOAD_GAME);
        }

        public void OnRecvJoin(C_JoinToGame req, int sessionId)
        {
            /* 진입점. 
             *  1. 진입점은 로비 플레이어로 변경해야한다. 
             *  2. 세션 ID 는 플레이어 고유 ID 가 될 수 없다. 
             *  3. 플레이어 고유 ID 는 게임 내의 ID 값들과 같은 대역대를 사용한다. ( 0 ~ 9999, 10000 ~)
             *  4. DB에 저장되는 유저 ID 값은 10000 대역대부터 생성되도록 작업한다. 
             */

            var playerEntity = new PlayerEntity(_room, sessionId, req.userId, req.userName, /*req.heroKey*/ 1);
            playerEntity.ClientStatus = EClientStatus.ENTERED_ROOM;

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

            if (!GameData.Instance.HerosDict.TryGetValue(req.heroKey, out var heroData))
            {
                return;
            }

            player.ChangeHeroKey(heroData.key);
            _room.Broadcast(req);
        }

        public void OnRecvReady(CS_ReadyToGame req)
        {
            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            player.ClientStatus = EClientStatus.SELECT_READY;
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

            player.ClientStatus = EClientStatus.PLAY_READY;
            _room.Send(player.ID, new PlayStartResponse() { id = player.ID });

            if (CanPlayStart())
            {
                UpdateModeStatus(EGameModeStatus.PLAY_START);
            }
        }

        public void OnRecvMoveRequest(MoveRequest req)
        {
            if (IsPlayControlLocked())
            {
                return;
            }

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
            if (IsPlayControlLocked())
            {
                return;
            }

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
            if (IsPlayControlLocked())
            {
                return;
            }

            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            var res = new IncreaseStatResponse();
            res.id = player.ID;

            int increaseValue = req.increase;
            //NOTE : 현재 1골드 당 해당 스탯 1 증가. 
            //TODO : 골드량 대 스탯 증가량 비례 값은 데이터 시트로 관리할 예정. 
            int currentGold = player.Inventory?.GetCurrencyByType(ECurrency.GOLD) ?? 0;
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
                    player.UpgradedStat.AddStr(increaseValue);
                    break;
                case EStatType.DEF:
                    player.UpgradedStat.AddDef(increaseValue);
                    break;
                case EStatType.HP:
                    player.UpgradedStat.AddMaxHp(increaseValue, true);
                    break;
                default:
                    Logger.Instance.Error($"Wrong Stat Type {(EStatType)req.type}");
                    break; ;
            }

            res.increase = increaseValue;
            res.usedGold = increaseValue;

            player.UpdateStat();

            _room.Send(player.ID, res);
        }


        public void OnRecvPickRewardRequest(PickRewardRequest req)
        {
            if (IsPlayControlLocked())
            {
                return;
            }

            var player = GetPlayerEntityById(req.id);
            if (player == null)
            {
                return;
            }

            var worldId = req.worldId;

            var res = new PickRewardResponse();
            res.id = player.ID;
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
                        player.Inventory?.EarnCurrency(ECurrency.GOLD, amount);
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
            _room.Send(player.ID, res);
            player.UpdateStat();
        }

        public bool IsPlayControlLocked()
        {
            return Status != EGameModeStatus.PLAY_START;
        }


        public void OnCheatRequest(int id, CheatRequest req)
        {
            var player = GetPlayerEntityById(id);
            if (player == null)
            {
                return;
            }

            var result = player.CheatExecuter.Execute(req);
            _room.Send(player.ID, new CheatResponse()
            {
                result = result ? DEFINE.SUCCESS : DEFINE.ERROR,
            });
        }
    }
}