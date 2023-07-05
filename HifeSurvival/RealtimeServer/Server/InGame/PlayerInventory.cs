using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class PlayerInventory
    {
        private InvenItem[] _itemSlotArr = new InvenItem[DEFINE.PLAYER_ITEM_SLOT];

        public InvenItem EquipItem(in ItemData item)
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

            if (equippedItem == null)
            {
                int slot = NextItemSlot();
                if(slot >= 0)
                {
                    equippedItem = new InvenItem(slot, item.key);
                    _itemSlotArr[slot] = equippedItem;
                }
            }
            else
            {
                equippedItem.Upgrade();
            }

            return equippedItem;
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
