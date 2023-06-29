﻿using System;
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
                if (item.itemKey == _itemSlotArr[i].itemKey)
                {
                    equippedItem = _itemSlotArr[i];
                    break;
                }
            }

            if (equippedItem == null)
            {
                var nextSlot = NextItemSlot();
                if(nextSlot > 0)
                {
                    _itemSlotArr[nextSlot] = new InvenItem(nextSlot,item);
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
