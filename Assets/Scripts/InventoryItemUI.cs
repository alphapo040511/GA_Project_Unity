using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI quantityText;

    public void UpdateUI(Item item)
    {
        itemNameText.text = item.itemName;
        quantityText.text = $"Count : {item.quantity}";
    }

    public void Empty()
    {
        itemNameText.text = "Empty";
        quantityText.text = $"Count : --";
    }
}
