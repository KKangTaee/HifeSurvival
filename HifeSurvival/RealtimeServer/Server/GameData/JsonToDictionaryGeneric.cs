using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Server
{
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

                if (int.TryParse(key, out var numberKey))
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
}
