using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class PlayerInventory
    {
        public InvenItem[] itemSlot = new InvenItem[DEFINE.PLAYER_ITEM_SLOT];

        public int EquipItem(PItem item)
        {
            InvenItem equippedItem = null;
            int slot = -1;

            for (int i = 0; i < DEFINE.PLAYER_ITEM_SLOT; i++)
            {
                if (item.itemKey == itemSlot[i].itemKey)
                {
                    equippedItem = itemSlot[i];
                    break;
                }
            }

            if (equippedItem == null)
            {
                var nextSlot = NextItemSlot();
                if(nextSlot > 0)
                {
                    itemSlot[nextSlot] = new InvenItem(nextSlot,item);
                    slot = nextSlot;
                }
            }
            else
            {
                equippedItem.LevelUp(item);
                slot = equippedItem.slot;
            }

            return slot;
        }

        public int NextItemSlot()
        {
            for (int i = 0; i < DEFINE.PLAYER_ITEM_SLOT; i++)
            {
                if (itemSlot[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public EntityStat TotalItemStat()
        {
            var stat = new EntityStat();
            foreach(var item in itemSlot)
            {
                if (item == null)
                    continue;

                stat += item.stat;
            }

            return stat;
        }
    }
}
