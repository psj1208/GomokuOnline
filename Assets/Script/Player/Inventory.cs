using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<itemSlot> items;

    private void Start()
    {
        items = new List<itemSlot>();
    }

    public void AddItem(BaseItem item)
    {
        var existItem = FindItemAvailable(item);

        if (existItem != null)
        {
            existItem.Amount++;
        }
        else
        {
            items.Add(new itemSlot(item));
        }
    }

    private itemSlot FindItemAvailable(BaseItem item)
    {
        if (!item.CanStack)
            return null;

        foreach (var slot in items)
        {
            if (slot.Item.ItemId == item.ItemId && slot.Amount < slot.Item.MaxStack)
            {
                return slot;
            }
        }

        return null;
    }
}

public class itemSlot
{
    private BaseItem item;
    public BaseItem Item {  get { return item; } }
    private int amount;
    public int Amount { get { return amount; } set { amount = value; } }

    public itemSlot(BaseItem item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }
}
