
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
            Initialize();
        }

        public void Initialize()
        {
            var startLevel = 1;
            var startData = GameData.Instance.GetItemUpgadeDataByLevel(ItemKey, startLevel);
            if (startData == null)
            {
                Logger.Instance.Error("Item Init Failed");
                return;
            }

            if (!GameData.Instance.ItemSkillDict.TryGetValue(startData.skillKey, out var skilldata))
            {
                Logger.Instance.Error("Item Init Failed (Skill Data Invalid)");
                return;
            }

            Level = startLevel;
            CurrentStack = 0;
            MaxStack = startData.needStack;

            Skill = new ItemSkill(skilldata);
            Stat = new EntityStat(startData);
        }

        public void Upgrade()
        {
            CurrentStack++;
            if (CurrentStack < MaxStack)
            {
                return;
            }

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

            CurrentStack = 0;
            MaxStack = upgradeData.needStack;

            Skill = new ItemSkill(skilldata);
            Stat = new EntityStat(upgradeData);

            Level = nextLevel;
        }
    }
}
