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

        // 몬스터 혹은 플레이어의 스폰 위치정보
        public List<WorldSpawnData> SpawnList { get; private set; }

        // 월드맵에 스폰된 아이템
        public ConcurrentDictionary<int, WorldItemData> ItemDict { get; private set; } = new ConcurrentDictionary<int, WorldItemData>();

        private int _dropID = 0;

        public void ParseJson(string mapData)
        {
            if (mapData == null)
            {
                Logger.GetInstance().Error("mapData is null or empty!");
                return;
            }

            var N = JSON.Parse(mapData);

            CanGoTiles = new HashSet<PVec3>();
            foreach (JSONNode node in N["can_go_tile"].AsArray)
            {
                string[] partArr = node.Value.Split(',');
                PVec3 tile = new PVec3()
                {
                    x = float.Parse(partArr[0]),
                    y = float.Parse(partArr[1]),
                    z = float.Parse(partArr[2])
                };
                CanGoTiles.Add(tile);
            }

            // Parse spawn list
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

            WorldItemData worldItem = new WorldItemData()
            {
                worldId = Interlocked.Increment(ref _dropID),
                itemData = itemData,
            };

            Logger.GetInstance().Debug($"Drop Item ID {worldItem.worldId}");
            ItemDict.TryAdd(worldItem.worldId, worldItem);

            var broadcast = new UpdateRewardBroadcast();
            broadcast.worldId = worldItem.worldId;
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
                        broadcast.item = new PItem()
                        {
                            //NOTE : 임시 값. 
                            itemKey = worldItem.itemData.subType,
                            level = 1,
                            str = 999,
                            def = 999,
                            hp = 999,
                            cooltime = 12,
                            canUse = true
                        };
                        break;
                    }
                default:
                    break;
            }

            return broadcast;
        }

        public RewardData PickReward(int worldId)
        {
            if (ItemDict.TryGetValue(worldId, out var worldItem) == false)
            {
                Logger.GetInstance().Error($"invalid worldId : {worldId}");
                return default;
            }

            ItemDict.TryRemove(worldId, out _);
            return worldItem.itemData;
        }

    }
}