using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public class WorldMap
    {
        public class SpawnData
        {
            public int spawnType;
            public int groupId;
            public List<Vec3> pivotList;
        }

        public enum ESpawnType
        {
            PLAYER,

            MONSTER,

            ITEM,

            NONE,
        }


        public HashSet<Vec3> CanGoTiles { get; private set; }
        public List<SpawnData> SpawnList { get; private set; }

        public void Init()
        {
            ParseJson();
        }

        public void ParseJson()
        {
            var mapData = StaticData.Instance.SystemsDict["map_data"].value;

            if (mapData == null)
            {
                Logger.GetInstance().Error("mapData is null or empty!");
                return;
            }

            var N = SimpleJSON.JSON.Parse(mapData);

            // Parse world name
            // mapName = N["world_name"];

            // Parse can go tile list
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
            SpawnList = new List<SpawnData>();
            foreach (SimpleJSON.JSONNode node in N["spawn_list"].AsArray)
            {
                var spawnData = new SpawnData();
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
    }
}