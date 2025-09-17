using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListInventory : MonoBehaviour
{
    public InventoryItemUI uiPrefab;
    public Transform inventoryScrollView;

    public TMP_InputField inputField;

    public List<Item> items = new List<Item>();
    private List<InventoryItemUI> inventoryUI = new List<InventoryItemUI>();

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Add()
    {
        if (inputField.text != null)
        {
            AddItem(inputField.text);
        }
    }

    public void Remove()
    {
        if (inputField.text != null)
        {
            RemoveItem(inputField.text);
        }
    }

    public void AddItem(string itemName)
    {
        Item item = new Item(itemName, 1);
        items.Add(item);
        UpdateInventory();
    }

    public void RemoveItem(string itemName)
    {
        for(int i = items.Count - 1; i >= 0; i--)
        {
            if(items[i] != null && items[i].itemName == itemName)
            {
                items.RemoveAt(i);
            }
        }
        UpdateInventory();
    }

    private void UpdateInventory()
    {
        for (int i = 0; i < inventoryUI.Count; i++)
        {
            inventoryUI[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < items.Count; i++)
        {
            if(i < inventoryUI.Count)
            {
                if (items[i] != null)
                {
                    inventoryUI[i].gameObject.SetActive(true);
                    inventoryUI[i].UpdateUI(items[i]);
                }
            }
            else
            {
                InventoryItemUI ui = Instantiate(uiPrefab, inventoryScrollView);
                ui.UpdateUI(items[i]);
                inventoryUI.Add(ui);;
            }
        }
    }
}
