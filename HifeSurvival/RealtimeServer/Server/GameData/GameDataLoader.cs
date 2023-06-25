using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Server.GameData
{
    public class GameDataLoader
    {
        private static GameDataLoader _instance;

        public static GameDataLoader Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameDataLoader();

                return _instance;
            }
        }


        //------------------
        // variables
        //------------------

        private string apiKey = "AIzaSyABnRmQck9SP3Gv7syjremXAjDBDOky8so";
        private string sheetId = "104ZnnXWWorMZOAhuY0o1o1xIL2H41opJlrJLsSEk_C4";
        private string sheetsApiUrl = "https://sheets.googleapis.com/v4/spreadsheets";


        //------------------
        // Static Datas
        //------------------

        public Dictionary<string, Heros> HerosDict { get; private set; }
        public Dictionary<string, Systems> SystemsDict { get; private set; }
        public Dictionary<string, Monsters> MonstersDict { get; private set; }
        public Dictionary<string, MonstersGroup> MonstersGroupDict { get; private set; }
        public Dictionary<string, Item> ItemDict { get; private set; }
        public Dictionary<string, ChapterData> ChapaterDataDict { get; private set; }

        public async Task Init()
        {
            // 구글 스프레드 시트의 모든 이름 가져오기
            var waiter = new AsyncWaiting();
            List<string> sheetNames = new List<string>();

            ServerRequestManager.Instance.AddRequestData(new ServerRequestManager.ServerRequestData()
            {
                URL = $"{sheetsApiUrl}/{sheetId}?key={apiKey}",
                doneCallback = (jsonStr) =>
                {
                    JSONNode sheetsInfoJson = JSON.Parse(jsonStr);
                    JSONArray sheetsArray = sheetsInfoJson["sheets"].AsArray;

                    foreach (JSONNode sheet in sheetsArray)
                        sheetNames.Add(sheet["properties"]["title"].Value);

                    waiter.Signal();
                }
            });

            await waiter.Wait();
            waiter.Reset();

            // 배치로 모든 스프레드 시트의 데이터 가져오기.
            string ranges = string.Join("&", sheetNames.Select(sheetName => $"ranges={Uri.EscapeDataString(sheetName)}"));
            string batchGetUrl = $"{sheetsApiUrl}/{sheetId}/values:batchGet?{ranges}&key={apiKey}";

            ServerRequestManager.Instance.AddRequestData(new ServerRequestManager.ServerRequestData()
            {
                URL = batchGetUrl,
                doneCallback = (jsonStr) =>
                {
                    JSONNode batchDataJson = JSONNode.Parse(jsonStr);

                    Console.WriteLine(jsonStr);

                    foreach (JSONNode node in batchDataJson["valueRanges"].AsArray)
                    {
                        string trimmed = node["range"].ToString().Trim('\"');
                        string[] parts = trimmed.Split('!');
                        var rangeValue = parts[0];

                        if (rangeValue.Equals("systems"))
                        {
                            SystemsDict = JsonToDictionaryGeneric.ParseJsonToDictionary<Systems>(node.ToString());
                        }
                        else if (rangeValue.Equals("heros"))
                        {
                            HerosDict = JsonToDictionaryGeneric.ParseJsonToDictionary<Heros>(node.ToString());
                        }
                        else if (rangeValue.Equals("monsters"))
                        {
                            MonstersDict = JsonToDictionaryGeneric.ParseJsonToDictionary<Monsters>(node.ToString());
                        }
                        else if (rangeValue.Equals("monsters_group"))
                        {
                            MonstersGroupDict = JsonToDictionaryGeneric.ParseJsonToDictionary<MonstersGroup>(node.ToString());
                        }
                        else if (rangeValue.Equals("item"))
                        {
                            ItemDict = JsonToDictionaryGeneric.ParseJsonToDictionary<Item>(node.ToString());
                        }
                        else if (rangeValue.Equals("chapter_data"))
                        {
                            ChapaterDataDict = JsonToDictionaryGeneric.ParseJsonToDictionary<ChapterData>(node.ToString());
                        }
                    }

                    waiter.Signal();
                }
            });


            await waiter.Wait();
        }


        public class JsonToDictionaryGeneric
        {
            public string json;

            [Serializable]
            public class SheetData
            {
                public string range;
                public string majorDimension;
                public List<List<string>> values;
            }


            public static Dictionary<string, T> ParseJsonToDictionary<T>(string jsonString) where T : class, new()
            {
                List<List<string>> rawData = JsonToRawData(jsonString);
                Dictionary<string, T> resultDictionary = new Dictionary<string, T>();

                // 첫 번째 행은 헤더이므로 1부터 시작
                for (int i = 1; i < rawData.Count; i++)
                {
                    T item = new T();
                    var itemType = item.GetType();

                    int fieldIndex = 0;
                    foreach (var field in itemType.GetFields())
                    {
                        object value = null;

                        if (field.FieldType == typeof(int))
                        {
                            value = int.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            value = float.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(double))
                        {
                            value = double.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(long))
                        {
                            value = long.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(short))
                        {
                            value = short.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(uint))
                        {
                            value = uint.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(ulong))
                        {
                            value = ulong.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(ushort))
                        {
                            value = ushort.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(byte))
                        {
                            value = byte.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(sbyte))
                        {
                            value = sbyte.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(char))
                        {
                            value = rawData[i][fieldIndex][0];
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            value = bool.Parse(rawData[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(string))
                        {
                            value = rawData[i][fieldIndex];
                        }

                        // 다른 필드 유형이 필요한 경우 여기에 추가
                        if (value != null)
                            field.SetValue(item, value);

                        fieldIndex++;
                    }

                    string key = itemType.GetField("key").GetValue(item).ToString();

                    resultDictionary.Add(key, item);
                }

                return resultDictionary;
            }


            private static List<List<string>> JsonToRawData(string jsonString)
            {
                JSONNode sheetJson = JSONNode.Parse(jsonString);
                List<List<string>> values = new List<List<string>>();

                JSONArray rows = sheetJson["values"].AsArray;
                foreach (JSONNode row in rows)
                {
                    List<string> rowData = new List<string>();
                    foreach (JSONNode cell in row.AsArray)
                    {
                        rowData.Add(cell.Value);
                    }
                    values.Add(rowData);
                }

                return values;
            }
        }


        [Serializable]
        public class Systems
        {
            public string key;
            public string value;
        }

        [Serializable]
        public class Heros
        {
            public int key;
            public string name;
            public int str;
            public int def;
            public int hp;
            public float attackSpeed;
            public float moveSpeed;
            public float attackRange;
            public float detectRange;
            public string desc;
        }

        [Serializable]
        public class Monsters
        {
            public int key;
            public string name;
            public int grade;
            public int str;
            public int def;
            public int hp;
            public float attackSpeed;
            public float moveSpeed;
            public float attackRange;
            public float detectRange;
            public string rewardIds;
            public string desc;
        }

        [Serializable]
        public class MonstersGroup
        {
            public int key;
            public int groupId;
            public string monsterGroups;
            public int respawnTime;
            public int enabled;
        }

        [Serializable]
        public class Item
        {
            public int key;
            public string name;
            public int grade;
            public int str;
            public int def;
            public int hp;
            public float attackSpeed;
            public float moveSpeed;
            public float attackRange;
            public float detectRange;
        }

        [Serializable]
        public class ChapterData
        {
            public int key;
            public string name;
            public string mapData;
            public string phase1;
            public string phase2;
            public string phase3;
            public string phase4;
            public int playTimeSec;
        }
    }
}