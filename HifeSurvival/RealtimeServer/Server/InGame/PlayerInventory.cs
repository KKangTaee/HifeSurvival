using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class PlayerInventory
    {
        private InvenItem[] _itemSlotArr = new InvenItem[DEFINE.PLAYER_ITEM_SLOT];

        public int EquipItem(PItem item)
        {
            InvenItem equippedItem = null;
            int slot = -1;

            for (int i = 0; i < DEFINE.PLAYER_ITEM_SLOT; i++)
            {
                var slotItem = _itemSlotArr[i];
                if (slotItem == null)
                    continue;

                if (item.itemKey == slotItem.itemKey)
                {
                    equippedItem = slotItem;
                    break;
                }
            }

            if (equippedItem == null)
            {
                slot = NextItemSlot();
                if(slot >= 0)
                {
                    _itemSlotArr[slot] = new InvenItem(slot, item);
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
                if (_itemSlotArr[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public EntityStat TotalItemStat()
        {
            var stat = new EntityStat();
            foreach(var item in _itemSlotArr)
            {
                if (item == null)
                    continue;

                stat += item.stat;
            }

            return stat;
        }
    }
}
