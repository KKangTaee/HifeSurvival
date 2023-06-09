﻿using System.Collections.Generic;

namespace Server
{
    public class PlayerInventory
    {
        private PlayerEntity _ownerPlayer;
        private InvenItem[] _itemSlotArr = new InvenItem[DEFINE.PLAYER_ITEM_SLOT];
        private Dictionary<ECurrency, int> _currencyDict = new Dictionary<ECurrency, int>();

        public PlayerInventory(PlayerEntity player)
        {
            _ownerPlayer = player;
        }

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
                if (0 <= slot)
                {
                    equippedItem = new InvenItem(slot, item.key);
                    _itemSlotArr[slot] = equippedItem;
                }
            }
            else
            {
                if (equippedItem.Level < DEFINE.MAX_ITEM_LEVEL)
                {
                    equippedItem.Upgrade();
                }
                else
                {
                    //TODO : Gold 지급. (시트 참조)
                    EarnCurrency(ECurrency.GOLD, 19941111);
                }
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
            foreach (var item in _itemSlotArr)
            {
                if (item == null)
                {
                    continue;
                }

                stat += item.Stat;
            }

            return stat;
        }

        public void EarnCurrency(ECurrency currencyType, int amount)
        {
            if(_currencyDict.TryGetValue(currencyType, out int currentAmount))
            {
                _currencyDict[currencyType] += amount;
            }
            else
            {
                _currencyDict.Add(currencyType, amount);
            }

            UpdateCurrency();
        }

        public void SpendCurrency(ECurrency currencyType, int amount)
        {
            if (_currencyDict.TryGetValue(currencyType, out int currentAmount))
            {
                var resultAmount = currentAmount - amount;
                if (resultAmount < 0)
                {
                    resultAmount = 0;
                }

                _currencyDict[currencyType] = resultAmount;
            }
            else
            {
                _currencyDict.Add(currencyType, 0);
            }

            UpdateCurrency();
        }

        public int GetCurrencyByType(ECurrency currencyType)
        {
            if(_currencyDict.TryGetValue(currencyType, out var amount))
            {
                return amount;
            }

            return 0;
        }

        public void UpdateCurrency()
        {
            var currencyList = new List<PCurrency>();
            foreach (var currency in _currencyDict)
            {
                currencyList.Add(new PCurrency()
                {
                    currencyType = (int)currency.Key,
                    count = currency.Value
                });
            }

            var currencyBroadcast = new UpdatePlayerCurrency()
            {
                currencyList = currencyList,
            };

            _ownerPlayer.Room.Send(_ownerPlayer.ID, currencyBroadcast);
        }
    }
}
