using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public class EntityItem
    {
        public int itemSlotId;
        public int itemKey_Static;
        public int level;
        public int str;
        public int def;
        public int hp;
        public int cooltime;
        public bool canUse;
    }

    public struct ItemData
    {

        //--------------
        // enums
        //--------------

        public enum EItemType
        {
            GOLD,

            ITEM,
        }

        public int rewardType;
        public int subType;
        public int count;

        public static ItemData[] Parse(string inItemIds)
        {
            var itemIdsSet = inItemIds.Split(',');

            var itemDataArr = new ItemData[itemIdsSet.Length];

            for (int i = 0; i < itemIdsSet.Length; i++)
            {
                var split = itemIdsSet[i].Split(':');

                if (split?.Length != 3)
                {
                    HSLogger.GetInstance().Error($"itemData is wrong! : {itemIdsSet[i]}");
                    return null;
                }

                if (int.TryParse(split[0], out var item_type) == false ||
                   int.TryParse(split[1], out var sub_type) == false ||
                   int.TryParse(split[2], out var count) == false)
                {
                    HSLogger.GetInstance().Error($"itemData is wrong! : {itemIdsSet[i]}");
                    return null;
                }

                itemDataArr[i] = new ItemData()
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