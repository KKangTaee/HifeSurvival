using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class WorldMap
    {
        public class WorldSpawnData
        {
            public int spawnType;
            public int groupId;
            public List<Vec3> pivotList;
        }

        public class WorldItemData
        {
            public int worldId;
            public string itemData;
        }

        public enum ESpawnType
        {
            PLAYER,

            MONSTER,

            ITEM,

            NONE,
        }


        public HashSet<Vec3> CanGoTiles { get; private set; }

        // 몬스터 혹은 플레이어의 스폰 위치정보
        public List<WorldSpawnData> SpawnList { get; private set; }

        // 월드맵에 스폰된 아이템
        public Dictionary<int, WorldItemData> ItemDict { get; private set; } = new Dictionary<int, WorldItemData>();

        private int _mHashCode = 0;

        public void Init()
        {
            ParseJson();
        }

        public void ParseJson()
        {
            var mapData = StaticData.Instance.SystemsDict["map_data"].value;

            if (mapData == null)
            {
                HSLogger.GetInstance().Error("mapData is null or empty!");
                return;
            }

            var N = SimpleJSON.JSON.Parse(mapData);

            CanGoTiles = new HashSet<Vec3>();
            foreach (SimpleJSON.JSONNode node in N["can_go_tile"].AsArray)
            {
                string[] parts = node.Value.Split(',');
                Vec3 tile = new Vec3()
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

                spawnData.pivotList = new List<Vec3>();
                foreach (SimpleJSON.JSONNode pivotNode in node["pivot_list"].AsArray)
                {
                    string[] parts = pivotNode.Value.Split(',');
                    Vec3 pivot = new Vec3()
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

        public WorldItemData DropItem(string inRewardData)
        {
            var itemData = inRewardData.FilterRewardIdsByRandomProbability();

            if (itemData == null)
                return null;

            WorldItemData worldItem = new WorldItemData()
            {
                worldId  = _mHashCode++,
                itemData = itemData
            };

            ItemDict.Add(worldItem.worldId, worldItem);

            return worldItem;
        }


        public void GetItem(int inWorldId, int inItemSlotId, ref PlayerEntity inEntity)
        {

        }
    }
}