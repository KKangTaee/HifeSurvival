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
                _instance ??= new GameData();
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
            var loadSheetTimeStamp = ServerTime.GetCurrentTimestamp();
            var waiter = new AsyncWaiting();
            var sheetNameList = new List<string>();

            ServerRequestManager.Instance.AddRequestData(new ServerRequestManager.ServerRequestData()
            {
                URL = $"{sheetsApiUrl}/{sheetId}?key={apiKey}",
                doneCallback = (jsonStr) =>
                {
                    var sheetsInfoJson = JSON.Parse(jsonStr);
                    var sheetsArray = sheetsInfoJson["sheets"].AsArray;

                    foreach (JSONNode sheet in sheetsArray)
                    {
                        sheetNameList.Add(sheet["properties"]["title"].Value);
                    }

                    waiter.Signal();
                }
            });

            await waiter.Wait();
            waiter.Reset();

            var dataLoadStartTimestamp = ServerTime.GetCurrentTimestamp();
            Logger.Instance.DataCheckInfo($"Load Sheet Success  elapsed {dataLoadStartTimestamp - loadSheetTimeStamp} ms");

            string ranges = string.Join("&", sheetNameList.Select(sheetName => $"ranges={Uri.EscapeDataString(sheetName)}"));
            string batchGetUrl = $"{sheetsApiUrl}/{sheetId}/values:batchGet?{ranges}&key={apiKey}";

            ServerRequestManager.Instance.AddRequestData(new ServerRequestManager.ServerRequestData()
            {
                URL = batchGetUrl,
                doneCallback = (jsonStr) =>
                {
                    var batchDataJson = JSONNode.Parse(jsonStr);

                    foreach (JSONNode node in batchDataJson["valueRanges"].AsArray)
                    {
                        string trimmed = node["range"].ToString().Trim('\"');
                        string[] partArr = trimmed.Split('!');
                        string rangeValue = partArr[0];

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

            var loadedTimeStamp = ServerTime.GetCurrentTimestamp();
            Logger.Instance.DataCheckInfo($"Data Load Success  elapsed {loadedTimeStamp - dataLoadStartTimestamp} ms");

            bool isSuccess = BakeAndValidCheckData();
            if (isSuccess)
            {
                var validTimeStamp = ServerTime.GetCurrentTimestamp();
                Logger.Instance.DataCheckInfo($"Data Bake And Valid Check Success  elapsed {validTimeStamp - loadedTimeStamp} ms");
            }
            else
            {
                Logger.Instance.DataCheckError("Data Bake And Valid Check Failed");
            }
        }

        private bool BakeAndValidCheckData()
        {
            bool isSuccess = true;
            foreach (var chapDataKey in ChapaterDataDict.Keys)
            {
                if (ChapaterDataDict.TryGetValue(chapDataKey, out var data))
                {
                    var groupKeyList = new List<int>();

                    data.phase1.Split(":").ToList().ForEach(p => groupKeyList.Add(int.Parse(p)));
                    data.phase1 = null;
                    data.phase1GkeyArr = groupKeyList.ToArray();
                    groupKeyList.Clear();

                    data.phase2.Split(":").ToList().ForEach(p => groupKeyList.Add(int.Parse(p)));
                    data.phase2 = null;
                    data.phase2GkeyArr = groupKeyList.ToArray();
                    groupKeyList.Clear();

                    data.phase3.Split(":").ToList().ForEach(p => groupKeyList.Add(int.Parse(p)));
                    data.phase3 = null;
                    data.phase3GkeyArr = groupKeyList.ToArray();
                    groupKeyList.Clear();

                    data.phase4.Split(":").ToList().ForEach(p => groupKeyList.Add(int.Parse(p)));
                    data.phase4 = null;
                    data.phase4GkeyArr = groupKeyList.ToArray();

                    var phaseSecList = new List<int>();

                    data.phaseSec.Split(":").ToList().ForEach(p => phaseSecList.Add(int.Parse(p) * DEFINE.SEC_TO_MS));
                    data.phaseSec = null;
                    data.phaseSecArray = phaseSecList.ToArray();

                    if(phaseSecList.Count != DEFINE.SPAWN_PHASE_MAX)
                    {
                        Logger.Instance.DataCheckError($"phase sec need {DEFINE.SPAWN_PHASE_MAX}, chapter key {chapDataKey}");
                        isSuccess = false;
                    }
                }
            }

            ItemUpgradeByLevelDict = new ConcurrentDictionary<int, ConcurrentDictionary<int, ItemUpgradeData>>();
            foreach (var itemUpgradeData in ItemUpgradeDict)
            {
                if (ItemUpgradeByLevelDict.TryGetValue(itemUpgradeData.Value.itemKey, out var levelDict))
                {
                    if (levelDict.TryGetValue(itemUpgradeData.Value.level, out var data))
                    {
                        Logger.Instance.DataCheckError("duplicated data in itemupgrade");
                        isSuccess = false;
                    }
                    else
                    {
                        levelDict.TryAdd(itemUpgradeData.Value.level, itemUpgradeData.Value);
                    }
                }
                else
                {
                    var t = new ConcurrentDictionary<int, ItemUpgradeData>();
                    t.TryAdd(itemUpgradeData.Value.level, itemUpgradeData.Value);
                    ItemUpgradeByLevelDict.TryAdd(itemUpgradeData.Value.itemKey, t);
                }
            }


            foreach (var chapData in ChapaterDataDict)
            {
                var groupKeyList = new List<int>();
                groupKeyList.AddRange(chapData.Value.phase1GkeyArr);
                groupKeyList.AddRange(chapData.Value.phase2GkeyArr);
                groupKeyList.AddRange(chapData.Value.phase3GkeyArr);
                groupKeyList.AddRange(chapData.Value.phase4GkeyArr);

                var groupIdDuplicatedCheck = new HashSet<int>();
                foreach (var groupKey in groupKeyList)
                {
                    if (MonstersGroupDict.TryGetValue(groupKey, out var mgData))
                    {
                        if (!groupIdDuplicatedCheck.Add(mgData.groupId))
                        {
                            Logger.Instance.DataCheckError($"duplicated Group Id - chapter key {chapData.Key}, group Key {groupKey},  group Id {mgData.groupId}");
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        Logger.Instance.DataCheckError($"Invalid goup key - chapter key {chapData.Key}, group key {groupKey}");
                        isSuccess = false;
                    }
                }
            }

            return isSuccess;
        }

        public ItemUpgradeData GetItemUpgadeDataByLevel(int itemKey, int itemLevel)
        {
            if (ItemUpgradeByLevelDict.TryGetValue(itemKey, out var levelDict))
            {
                if (levelDict.TryGetValue(itemLevel, out var data))
                {
                    return data;
                }
            }
            return null;
        }

    }
}