using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class PlayerInventory
    {
        private InvenItem[] _itemSlotArr = new InvenItem[DEFINE.PLAYER_ITEM_SLOT];

        public int EquipItem(in ItemData item)
        {
            InvenItem equippedItem = null;
            
            for (int i = 0; i < DEFINE.PLAYER_ITEM_SLOT; i++)
            {
                var slotItem = _itemSlotArr[i];
                if (slotItem == null)
                {
                    continue;
                }

                if (item.key == slotItem.ItemKey)
                {
                    equippedItem = slotItem;
                    break;
                }
            }

            int slot = -1;
            if (equippedItem == null)
            {
                slot = NextItemSlot();
                if(slot >= 0)
                {
                    _itemSlotArr[slot] = new InvenItem(slot, item.key);
                }
            }
            else
            {
                equippedItem.Upgrade();
                slot = equippedItem.Slot;
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

                stat += item.Stat;
            }

            return stat;
        }
    }
}
