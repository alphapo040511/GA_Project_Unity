using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public int quantity;

    public Item(string itemName, int quantity = 1)
    {
        this.itemName = itemName;
        this.quantity = quantity;
    }
}
