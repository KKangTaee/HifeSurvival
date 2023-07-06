using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class InvenItem
    {
        public int Slot { get; private set; }
        public int ItemKey { get; private set; }
        public int Level { get; private set; }
        public int CurrentStack { get; private set; }
        public int MaxStack { get; private set; }

        public ItemSkill Skill { get; private set; }
        public EntityStat Stat { get; private set; }

        public InvenItem(int slot, int itemKey)
        {
            Slot = slot;
            ItemKey = itemKey;
            Upgrade();
        }

        public void Upgrade()
        {
            var nextLevel = Level + 1;
            var upgradeData = GameData.Instance.GetItemUpgadeDataByLevel(ItemKey, nextLevel);
            if (upgradeData == null)
            {
                Logger.Instance.Error("Item Upgrade Failed");
                return;
            }

            if (!GameData.Instance.ItemSkillDict.TryGetValue(upgradeData.skillKey, out var skilldata))
            {
                Logger.Instance.Error("Item Upgrade Failed (Skill Data Invalid)");
                return;
            }

            CurrentStack++;
            MaxStack = upgradeData.needStack;
            if (CurrentStack < upgradeData.needStack)
            {
                return;
            }

            Skill = new ItemSkill(skilldata);
            Stat = new EntityStat(upgradeData);

            Level = nextLevel;
            CurrentStack = 0;
        }
    }
}
