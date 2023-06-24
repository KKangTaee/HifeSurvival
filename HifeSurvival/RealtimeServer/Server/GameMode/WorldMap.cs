using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;
using Server.Helper;

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

        public enum ESpawnType
        {
            PLAYER,

            MONSTER,

            ITEM,

            NONE,
        }


        public HashSet<PVec3> CanGoTiles { get; private set; }

        // 몬스터 혹은 플레이어의 스폰 위치정보
        public List<WorldSpawnData> SpawnList { get; private set; }

        // 월드맵에 스폰된 아이템
        public Dictionary<int, WorldItemData> ItemDict { get; private set; } = new Dictionary<int, WorldItemData>();

        private object _dropLock = new object();
        private int _dropID = 0;

        private IBroadcaster _broadcaster = null;

        public WorldMap(IBroadcaster broadcaster)
        {
            _broadcaster = broadcaster;
        }

        public void ParseJson(string inMapData)
        {
      
            if (inMapData == null)
            {
                Logger.GetInstance().Error("mapData is null or empty!");
                return;
            }

            var N = SimpleJSON.JSON.Parse(inMapData);

            CanGoTiles = new HashSet<PVec3>();
            foreach (SimpleJSON.JSONNode node in N["can_go_tile"].AsArray)
            {
                string[] parts = node.Value.Split(',');
                PVec3 tile = new PVec3()
                {
                    x = float.Parse(parts[0]),
                    y = float.Parse(parts[1]),
                    z = float.Parse(parts[2])
                };
                CanGoTiles.Add(tile);
            }

            // Parse spawn list
            SpawnList = new List<WorldSpawnData>();
            foreach (SimpleJSON.JSONNode node in N["spawn_list"].AsArray)
            {
                var spawnData = new WorldSpawnData();
                spawnData.spawnType = node["spawn_type"].AsInt;
                spawnData.groupId = node["group_id"].AsInt;

                spawnData.pivotList = new List<PVec3>();
                foreach (SimpleJSON.JSONNode pivotNode in node["pivot_list"].AsArray)
                {
                    string[] parts = pivotNode.Value.Split(',');
                    PVec3 pivot = new PVec3()
                    {
                        x = float.Parse(parts[0]),
                        y = float.Parse(parts[1]),
                        z = float.Parse(parts[2])
                    };
                    spawnData.pivotList.Add(pivot);
                }
                SpawnList.Add(spawnData);
            }
        }

        public void LoadMap(string inMapData)
        {
            ParseJson(inMapData);
        }

        public void DropItem(string inRewardData, PVec3 dropPos)
        {
            var itemDataStr = PacketExtensionHelper.FilterRewardIdsByRandomProbability(inRewardData);

            if (itemDataStr == null)
                return;

            var itemData = RewardData.Parse(itemDataStr).FirstOrDefault();

            WorldItemData worldItem = null;
            lock (_dropLock)
            {
                worldItem = new WorldItemData()
                {
                    worldId = _dropID++,
                    itemData = itemData,
                };

                ItemDict.Add(worldItem.worldId, worldItem);
            }

            S_DropReward dropItem = new S_DropReward()
            {
                worldId = worldItem.worldId,
                rewardType = worldItem.itemData.rewardType,
                pos = dropPos,
            };

            _broadcaster.Broadcast(dropItem);

            return;
        }


        public RewardData PickReward(int inWorldId)
        {
            lock (_dropLock)
            {
                if (ItemDict.TryGetValue(inWorldId, out var worldItem) == false)
                {
                    Logger.GetInstance().Error($"[{nameof(PickReward)}] worldId is wrong : {inWorldId}");
                    return default;
                }

                ItemDict.Remove(inWorldId);
                return worldItem.itemData;
            }
        }

    }
}