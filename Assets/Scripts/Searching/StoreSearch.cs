using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreSearch : MonoBehaviour
{
    public InventoryItemUI uiPrefab;
    public Transform scrollView;
    public TMP_InputField targetNameInput;
    private List<Item> items = new List<Item>();
    private Dictionary<string, GameObject> uiDict = new Dictionary<string, GameObject>();

    private bool isSorted = false;

    // Start is called before the first frame update
    void Start()
    {
        // 초기 아이템 생성
        for(int i = 0; i < 100; i++)
        {
            Item item = new Item($"Item_{i:D2}", UnityEngine.Random.Range(0, 100));

            items.Add(item);
            InventoryItemUI ui = Instantiate(uiPrefab, scrollView);

            ui.UpdateUI(item);

            uiDict.Add(item.itemName, ui.gameObject);
        }
    }

    public void ClickLinearButton()
    {
        if (targetNameInput.text == "")
        {
            NoneTarget();
            return;
        }

        foreach (GameObject ui in uiDict.Values)
        {
            ui.SetActive(false);
        }

        FindItemLinearSteps(targetNameInput.text);
    }

    public void ClickBinaryButton()
    {
        if(targetNameInput.text == "")
        {
            NoneTarget();
            return;
        }

        foreach (GameObject ui in uiDict.Values)
        {
            ui.SetActive(false);
        }

        if (!isSorted)
        {
            QuickSort(items, 0, items.Count - 1);
            isSorted = true;
        }
        FindItemBinarySteps(targetNameInput.text);
    }

    void NoneTarget()
    {
        foreach (GameObject ui in uiDict.Values)
        {
            ui.SetActive(true);
        }
    }

    void FindItemLinearSteps(string target)
    {
        foreach (Item item in items)
        {
            if (item.itemName == target)
            {
                if (uiDict[item.itemName])
                {
                    uiDict[item.itemName].SetActive(true);
                }
                return;
            }
        }
    }

    void FindItemBinarySteps(string target)
    {
        int left = 0;
        int right = items.Count - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;
            int cmp = items[mid].itemName.CompareTo(target);

            if (cmp == 0)
            {
                if (uiDict[items[mid].itemName])
                {
                    uiDict[items[mid].itemName].SetActive(true);
                }
                return;
            }
            else if (cmp < 0) left = mid + 1;
            else right = mid - 1;
        }
    }

    void QuickSort(List<Item> list, int left, int right)
    {
        if (left >= right) return;

        int pivotIndex = Patition(list, left, right);
        QuickSort(list, left, pivotIndex - 1);
        QuickSort(list, pivotIndex + 1, right);
    }

    int Patition(List<Item> list, int left, int right)
    {
        Item pivot = list[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (list[j].itemName.CompareTo(pivot.itemName) <= 0)
            {
                i++;
                Swap(list, i, j);
            }
        }
        Swap(list, i + 1, right);
        return i + 1;
    }

    void Swap(List<Item> list, int a, int b)
    {
        Item temp = list[a];
        list[a] = list[b];
        list[b] = temp;
    }
}
