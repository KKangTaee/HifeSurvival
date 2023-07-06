using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public class WorldMap
    {
        public class WorldSpawnData
        {
            public int spawnType;
            public int groupId;
            public List<PVec3> pivotList;
        }

        public class WorldItemData
        {
            public int worldId;
            public RewardData itemData;
        }

        public HashSet<PVec3> CanGoTiles { get; private set; }

        public List<WorldSpawnData> SpawnList { get; private set; }

        private ConcurrentDictionary<int, WorldItemData> _dropItemDict = new ConcurrentDictionary<int, WorldItemData>();

        private int _dropID = 0;

        public void ParseJson(string mapData)
        {
            if (mapData == null)
            {
                Logger.Instance.Error("mapData is null or empty!");
                return;
            }

            var N = JSON.Parse(mapData);

            CanGoTiles = new HashSet<PVec3>();
            foreach (JSONNode node in N["can_go_tile"].AsArray)
            {
                string[] partArr = node.Value.Split(',');
                var tile = new PVec3()
                {
                    x = float.Parse(partArr[0]),
                    y = float.Parse(partArr[1]),
                    z = float.Parse(partArr[2])
                };
                CanGoTiles.Add(tile);
            }

            SpawnList = new List<WorldSpawnData>();
            foreach (JSONNode node in N["spawn_list"].AsArray)
            {
                var spawnData = new WorldSpawnData();
                spawnData.spawnType = node["spawn_type"].AsInt;
                spawnData.groupId = node["group_id"].AsInt;

                spawnData.pivotList = new List<PVec3>();
                foreach (JSONNode pivotNode in node["pivot_list"].AsArray)
                {
                    string[] partArr = pivotNode.Value.Split(',');
                    PVec3 pivot = new PVec3()
                    {
                        x = float.Parse(partArr[0]),
                        y = float.Parse(partArr[1]),
                        z = float.Parse(partArr[2])
                    };
                    spawnData.pivotList.Add(pivot);
                }
                SpawnList.Add(spawnData);
            }
        }

        public void LoadMap(string mapData)
        {
            ParseJson(mapData);
        }

        public UpdateRewardBroadcast DropItem(string rewardData, PVec3 dropPos)
        {
            var itemDataStr = PacketExtensionHelper.FilterRewardIdsByRandomProbability(rewardData);
            if (itemDataStr == null)
            {
                return null;
            }

            var itemData = RewardData.Parse(itemDataStr).FirstOrDefault();

            var newWorldId = Interlocked.Increment(ref _dropID);
            WorldItemData worldItem = new WorldItemData()
            {
                worldId = newWorldId,
                itemData = itemData,
            };

            Logger.Instance.Debug($"Drop Item ID {newWorldId}");
            if (!_dropItemDict.TryAdd(newWorldId, worldItem))
            {
                Logger.Instance.Error($"World ID Get Failed ID : {newWorldId}");
            }

            var broadcast = new UpdateRewardBroadcast();
            broadcast.worldId = newWorldId;
            broadcast.status = (int)ERewardState.DROP;
            broadcast.rewardType = worldItem.itemData.rewardType;
            broadcast.pos = dropPos;

            switch ((ERewardType)worldItem.itemData.rewardType)
            {
                case ERewardType.GOLD:
                    {
                        broadcast.gold = worldItem.itemData.count;
                        break;
                    }
                case ERewardType.ITEM:
                    {
                        if (GameData.Instance.ItemDict.TryGetValue(worldItem.itemData.subType, out var mastItem))
                        {
                            broadcast.item = new PDropItem()
                            {
                                itemKey = mastItem.key,
                            };
                        }
                        else
                        {
                            Logger.Instance.Error($"Invalid Item Key : {worldItem.itemData.subType}");
                        }

                        break;
                    }
                default:
                    break;
            }

            return broadcast;
        }

        public RewardData PickReward(int worldId)
        {
            if (_dropItemDict.TryGetValue(worldId, out var worldItem) == false)
            {
                Logger.Instance.Error($"invalid worldId : {worldId}");
                return default;
            }

            _dropItemDict.TryRemove(worldId, out _);
            return worldItem.itemData;
        }
    }
}