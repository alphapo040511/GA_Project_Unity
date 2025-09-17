using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArrayInventory : MonoBehaviour
{
    public int inventorySize = 10;
    public InventoryItemUI uiPrefab;
    public Transform inventoryScrollView;

    public TMP_InputField inputField;

    public Item[] items;
    private InventoryItemUI[] inventoryUI;

    // Start is called before the first frame update
    void Start()
    {
        items = new Item[inventorySize];
        inventoryUI = new InventoryItemUI[inventorySize];

        for(int i = 0; i < inventorySize; i++)
        {
            InventoryItemUI ui = Instantiate(uiPrefab, inventoryScrollView);
            ui.gameObject.name = $"inventory {i}";
            ui.Empty();
            inventoryUI[i] = ui;
        }
    }

    public void Add()
    {
        if(inputField.text != null)
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
        for(int i = 0; i < items.Length; i++)
        {
            if(items[i] == null)
            {
                items[i] = new Item(itemName, 1);
                Debug.Log($"{itemName} �߰��� ({i} ��° ����)");
                UpdateInventory();
                return;
            }
        }
        Debug.Log("�κ��丮�� ���� á���ϴ�.");
    }

    public void RemoveItem(string itemName)
    {
        for (int i = items.Length - 1
            ; i >= 0 ; i--)
        {
            if (items[i] != null && items[i].itemName == itemName)
            {
                Debug.Log($"{itemName} ������ ({i} ��° ����)");
                items[i] = null;
                UpdateInventory();
                return;
            }
        }
        Debug.Log($"�κ��丮�� {itemName}��(��) �����ϴ�.");
    }

    private void UpdateInventory()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (items[i] == null)
            {
                inventoryUI[i].Empty();
            }
            else
            {
                inventoryUI[i].UpdateUI(items[i]);
            }
        }
    }
}
