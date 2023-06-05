using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


[CustomEditor(typeof(WorldMap), true), CanEditMultipleObjects]
public class WorldMapEditor : Editor
{

    private string mapName = "normal";
    private List<Vector3> canGoTile;
    private List<WorldSpawn.SpawnData> spawnList;

    private bool showCanGoTile;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = false;

        // Toggle button for the CanGoTile list
        showCanGoTile = EditorGUILayout.Foldout(showCanGoTile, "Show CanGoTile");

        if (showCanGoTile && canGoTile != null)
        {
            for (int i = 0; i < canGoTile.Count; i++)
                EditorGUILayout.Vector3Field($"Tile {i}", canGoTile[i]);
        }

        GUI.enabled = true;


        if (GUILayout.Button("Show WorldMap Data", GUILayout.Height(40)) == true)
        {
            var wm = (WorldMap)target;

            canGoTile = wm.GetBackgroundCanGoTileList();

            wm.SetupToWorldObject();
            spawnList = wm.GetWorldObject<WorldSpawn>().Select(x => x.GetSpawnData()).ToList();
        }

        if (GUILayout.Button("Save JsonData", GUILayout.Height(40)) == true)
        {
            var N = new SimpleJSON.JSONClass();

            N["world_name"] = mapName;
            N["can_go_tile"] = new SimpleJSON.JSONArray();
            foreach (Vector3 tile in canGoTile)
            {
                ((SimpleJSON.JSONArray)N["can_go_tile"]).Add(tile.x + "," + tile.y + "," + tile.z);
            }

            N["spawn_list"] = new SimpleJSON.JSONArray();
            foreach (var spawn in spawnList)
            {
                var spawnJson = new SimpleJSON.JSONClass();
                spawnJson["spawn_type"] = spawn.spawnType.ToString();
                spawnJson["group_id"]   = spawn.groupId.ToString();
                spawnJson["pivot_list"] = new SimpleJSON.JSONArray();

                foreach (Vector3 pivot in spawn.pivotList)
                {
                    ((SimpleJSON.JSONArray)spawnJson["pivot_list"]).Add(pivot.x + "," + pivot.y + "," + pivot.z);
                }

                ((SimpleJSON.JSONArray)N["spawn_list"]).Add(spawnJson);
            }

            // Saving the JSON data to a file
            string path = $"{Application.dataPath}/mapData.txt"; // update this path
            System.IO.File.WriteAllText(path, N.ToString());
        }
    }
}