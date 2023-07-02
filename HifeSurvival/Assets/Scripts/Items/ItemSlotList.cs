using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemSlotList : MonoBehaviour
{
    [SerializeField] ItemSlot[] _itemSlotArr;


    //---------------
    // unity event
    //---------------

    private void Awake()
    {
        foreach(var slot in _itemSlotArr)
            slot.RemoveItem();
    }


    public void EquipItem(EntityItem inEntityItem)
    {
       var emptySlot = _itemSlotArr.FirstOrDefault(x=>x.IsEquipping == false);

       if(emptySlot == null)
       {
            Debug.LogError($"[{nameof(EquipItem)} itemSlot is full!");
            return;
       }

        Debug.Log("임시 호출!");

       emptySlot.EquipItem(inEntityItem);
    }

    public void RemoveItem(int inItemSlotId)
    {
        var itemSlot = _itemSlotArr.FirstOrDefault(x=>x.ItemInfo.slotId == inItemSlotId);

        if(itemSlot == null)
        {
            Debug.LogError($"[{nameof(RemoveItem)} itemSlot is full!");
            return;
        }

        itemSlot.RemoveItem();
    }
}
