using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class InvenItem
    {
        public int slot;
        public int itemKey;
        public int level;

        public int cooltime;
        public bool canUse;

        public EntityStat stat;

        public InvenItem(int slot , in PItem item)
        {
            this.slot = slot;
            itemKey = item.itemKey;
            level = item.level;

            stat = new EntityStat(item);
        }

        public void LevelUp(in PItem item)
        {
            level++;
            stat += new EntityStat(item);
        }
    }
}
