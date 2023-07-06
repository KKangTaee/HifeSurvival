using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public struct RewardData
    {
        public int rewardType;
        public int subType;
        public int count;

        public static RewardData[] Parse(string inItemIds)
        {
            var itemIdsSet = inItemIds.Split(',');
            var itemDataArr = new RewardData[itemIdsSet.Length];

            for (int i = 0; i < itemIdsSet.Length; i++)
            {
                var split = itemIdsSet[i].Split(':');
                if (split?.Length != 3)
                {
                    Logger.Instance.Error($"itemData is wrong! : {itemIdsSet[i]}");
                    return null;
                }

                if (int.TryParse(split[0], out var item_type) == false ||
                   int.TryParse(split[1], out var sub_type) == false ||
                   int.TryParse(split[2], out var count) == false)
                {
                    Logger.Instance.Error($"itemData is wrong! : {itemIdsSet[i]}");
                    return null;
                }

                itemDataArr[i] = new RewardData()
                {
                    rewardType = item_type,
                    subType = sub_type,
                    count = count,
                };
            }

            return itemDataArr;
        }

        public override string ToString()
        {
            return $"{rewardType}:{subType}:{count}";
        }
    }

}