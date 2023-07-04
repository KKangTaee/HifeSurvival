using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Server
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
        public int threadCheck = 0;


        //------------------
        // Static Datas
        //------------------

        [Obsolete]
        public ConcurrentDictionary<string, Systems> SystemsDict { get; private set; }
        public ConcurrentDictionary<int, Heros> HerosDict { get; private set; }
        public ConcurrentDictionary<int, Monsters> MonstersDict { get; private set; }
        public ConcurrentDictionary<int, MonstersGroup> MonstersGroupDict { get; private set; }
        public ConcurrentDictionary<int, Item> ItemDict { get; private set; }
        public ConcurrentDictionary<int, ChapterData> ChapaterDataDict { get; private set; }

        public async Task Init()
        {
            // 구글 스프레드 시트의 모든 이름 가져오기
            var waiter = new AsyncWaiting();
            List<string> sheetNameList = new List<string>();

            ServerRequestManager.Instance.AddRequestData(new ServerRequestManager.ServerRequestData()
            {
                URL = $"{sheetsApiUrl}/{sheetId}?key={apiKey}",
                doneCallback = (jsonStr) =>
                {
                    JSONNode sheetsInfoJson = JSON.Parse(jsonStr);
                    JSONArray sheetsArray = sheetsInfoJson["sheets"].AsArray;

                    foreach (JSONNode sheet in sheetsArray)
                        sheetNameList.Add(sheet["properties"]["title"].Value);

                    waiter.Signal();
                }
            });

            await waiter.Wait();
            waiter.Reset();

            // 배치로 모든 스프레드 시트의 데이터 가져오기.
            string ranges = string.Join("&", sheetNameList.Select(sheetName => $"ranges={Uri.EscapeDataString(sheetName)}"));
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
                        string[] partArr = trimmed.Split('!');
                        var rangeValue = partArr[0];

                        //if (rangeValue.Equals("systems"))
                        //{
                        //    SystemsDict = JsonToDictionaryGeneric.ParseJsonToDictionary<Systems>(node.ToString());
                        //}
                        //else
                        if (rangeValue.Equals("heros"))
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

            BakeData();
        }

        private bool BakeData()
        {
            foreach (var chapDataKey in ChapaterDataDict.Keys)
            {
                if(ChapaterDataDict.TryGetValue(chapDataKey, out var data))
                {
                    var pList = new List<int>();

                    data.phase1.Split(":").ToList().ForEach(p => pList.Add(int.Parse(p)));
                    data.phase1 = null;
                    data.phase1Array = pList.ToArray();
                    pList.Clear();

                    data.phase2.Split(":").ToList().ForEach(p => pList.Add(int.Parse(p)));
                    data.phase2 = null;
                    data.phase2Array = pList.ToArray();
                    pList.Clear();

                    data.phase3.Split(":").ToList().ForEach(p => pList.Add(int.Parse(p)));
                    data.phase3 = null;
                    data.phase3Array = pList.ToArray();
                    pList.Clear();

                    data.phase4.Split(":").ToList().ForEach(p => pList.Add(int.Parse(p)));
                    data.phase4 = null;
                    data.phase4Array = pList.ToArray();
                    pList.Clear();
                }
            }

            return true;
        }


        public class JsonToDictionaryGeneric
        {
            public string json;

            [Serializable]
            public class SheetData
            {
                public string range;
                public string majorDimension;
                public List<List<string>> valueList;
            }

            public static ConcurrentDictionary<int, T> ParseJsonToDictionary<T>(string jsonString) where T : class, new()
            {
                List<List<string>> rawDataList = JsonToRawData(jsonString);
                ConcurrentDictionary<int, T> resultDict = new ConcurrentDictionary<int, T>();

                // 첫 번째 행은 헤더이므로 1부터 시작
                for (int i = 1; i < rawDataList.Count; i++)
                {
                    T item = new T();
                    var itemType = item.GetType();

                    int fieldIndex = 0;
                    foreach (var field in itemType.GetFields())
                    {
                        object value = null;

                        if (field.FieldType == typeof(int))
                        {
                            value = int.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            value = float.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(double))
                        {
                            value = double.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(long))
                        {
                            value = long.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(short))
                        {
                            value = short.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(uint))
                        {
                            value = uint.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(ulong))
                        {
                            value = ulong.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(ushort))
                        {
                            value = ushort.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(byte))
                        {
                            value = byte.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(sbyte))
                        {
                            value = sbyte.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(char))
                        {
                            value = rawDataList[i][fieldIndex][0];
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            value = bool.Parse(rawDataList[i][fieldIndex]);
                        }
                        else if (field.FieldType == typeof(string))
                        {
                            value = rawDataList[i][fieldIndex];
                        }

                        // 다른 필드 유형이 필요한 경우 여기에 추가
                        if (value != null)
                            field.SetValue(item, value);

                        fieldIndex++;
                    }

                    var key = itemType.GetField("key").GetValue(item).ToString();

                    if(int.TryParse(key, out var numberKey))
                    {
                        if (!resultDict.TryAdd(numberKey, item))
                        {
                            Logger.GetInstance().Warn($"DATA LOAD CHECK - duplicated {numberKey}");
                        }
                    }
                    else
                    {
                        Logger.GetInstance().Warn($"DATA LOAD CHECK - Key must be int :  {key}");
                    }
                }

                return resultDict;
            }


            private static List<List<string>> JsonToRawData(string jsonString)
            {
                JSONNode sheetJson = JSONNode.Parse(jsonString);
                List<List<string>> valueList = new List<List<string>>();

                JSONArray rows = sheetJson["values"].AsArray;
                foreach (JSONNode row in rows)
                {
                    List<string> rowDataList = new List<string>();
                    foreach (JSONNode cell in row.AsArray)
                    {
                        rowDataList.Add(cell.Value);
                    }
                    valueList.Add(rowDataList);
                }

                return valueList;
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
            public float bodyRange;
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
            public float bodyRange;
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


            public int[] phase1Array;
            public int[] phase2Array;
            public int[] phase3Array;
            public int[] phase4Array;
        }
    }
}