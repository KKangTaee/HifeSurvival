using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Server
{
    public class GameData
    {
        private static GameData _instance;

        public static GameData Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameData();

                return _instance;
            }
        }

        private string apiKey = "AIzaSyABnRmQck9SP3Gv7syjremXAjDBDOky8so";
        private string sheetId = "104ZnnXWWorMZOAhuY0o1o1xIL2H41opJlrJLsSEk_C4";
        private string sheetsApiUrl = "https://sheets.googleapis.com/v4/spreadsheets";

        [Obsolete]
        public ConcurrentDictionary<string, Systems> SystemsDict { get; private set; }
        public ConcurrentDictionary<int, HeroData> HerosDict { get; private set; }
        public ConcurrentDictionary<int, MonsterData> MonstersDict { get; private set; }
        public ConcurrentDictionary<int, MonstersGroupData> MonstersGroupDict { get; private set; }
        public ConcurrentDictionary<int, ItemData> ItemDict { get; private set; }
        public ConcurrentDictionary<int, ItemUpgradeData> ItemUpgradeDict { get; private set; }
        public ConcurrentDictionary<int, ItemSkillData> ItemSkillDict { get; private set; }
        public ConcurrentDictionary<int, FormulaData> FormulaDict { get; private set; }
        public ConcurrentDictionary<int, ChapterData> ChapaterDataDict { get; private set; }

        public ConcurrentDictionary<int, ConcurrentDictionary<int, ItemUpgradeData>> ItemUpgradeByLevelDict { get; private set; }

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
                            HerosDict = JsonToDictionaryGeneric.ParseJsonToDictionary<HeroData>(node.ToString());
                        }
                        else if (rangeValue.Equals("monsters"))
                        {
                            MonstersDict = JsonToDictionaryGeneric.ParseJsonToDictionary<MonsterData>(node.ToString());
                        }
                        else if (rangeValue.Equals("monsters_group"))
                        {
                            MonstersGroupDict = JsonToDictionaryGeneric.ParseJsonToDictionary<MonstersGroupData>(node.ToString());
                        }
                        else if (rangeValue.Equals("item"))
                        {
                            ItemDict = JsonToDictionaryGeneric.ParseJsonToDictionary<ItemData>(node.ToString());
                        }
                        else if (rangeValue.Equals("item_upgrade"))
                        {
                            ItemUpgradeDict = JsonToDictionaryGeneric.ParseJsonToDictionary<ItemUpgradeData>(node.ToString());
                        }
                        else if (rangeValue.Equals("item_skill"))
                        {
                            ItemSkillDict = JsonToDictionaryGeneric.ParseJsonToDictionary<ItemSkillData>(node.ToString());
                        }
                        else if (rangeValue.Equals("formula"))
                        {
                            FormulaDict = JsonToDictionaryGeneric.ParseJsonToDictionary<FormulaData>(node.ToString());
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
                if (ChapaterDataDict.TryGetValue(chapDataKey, out var data))
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


        public ItemUpgradeData GetItemUpgadeDataByLevel(int itemKey, int itemLevel)
        {
            if (ItemUpgradeByLevelDict.TryGetValue(itemKey, out var levelDict))
            {
                if(levelDict.TryGetValue(itemLevel, out var data))
                {
                    return data;
                }
            }
            return null;
        }

    }
}