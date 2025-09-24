using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Search : MonoBehaviour
{
    [Header("UI References (TMP)")]
    public TMP_InputField inputItemCount;
    public TMP_InputField inputSearchCount;
    public TMP_Text resultText;

    private List<Item> items = new List<Item>();

    private long sortSteps = 0;     // 정렬 비교 횟수
    private long linearSteps = 0;   // 선형 탐색 비교 횟수
    private long binarySteps = 0;   // 이진 탐색 비교 횟수

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnFindButton()
    {
        if (!int.TryParse(inputItemCount.text, out int itemCount)) itemCount = 10000;
        if (!int.TryParse(inputSearchCount.text, out int searchCount)) searchCount = 10000;
   
        // 아이템 초기화
        for(int i = 0; i < itemCount; i++)
        {
            items.Add(new Item($"Item_{Random.Range(0, itemCount):D5}", 1));
        }

        // 탐색 대상 생성
        List<string> targets = new List<string>();
        for(int i = 0; i < searchCount; i++)
        {
            targets.Add($"Item_{Random.Range(0, itemCount):D5}");
        }

        // 1. 선형 탐색
        linearSteps = 0;
        foreach(var t in targets)
        {
            linearSteps += FindItemLinearSteps(t);
        }

        // 퀵소트 + 이진 탐색
        sortSteps = 0;
        QuickSort(items, 0, items.Count - 1);

        binarySteps = 0;
        foreach(var t in targets)
        {
            binarySteps += FindItemBinarySteps(t);
        }

        // 결과 출력
        resultText.text =
            $"Item Count : {itemCount}\n" +
            $"Search Count : {searchCount}\n\n\n" +
            $"Linear Search Total Comparisons : {linearSteps}\n\n" +
            $"Quick Sort Comparisons : {sortSteps}\n" +
            $"Binary Search Total Comparisons : {binarySteps}\n" +
            $"Total (Sort + Binary) : {sortSteps + binarySteps}";
    }

    // 선형 탐색 비교 횟수 반환
    int FindItemLinearSteps(string target)
    {
        int steps = 0;
        foreach(Item item in items)
        {
            steps++;
            if (item.itemName == target)
                return steps;
        }
        return steps;
    }

    // 이진 탐색 (비교 횟수 반환)
    int FindItemBinarySteps(string target)
    {
        int steps = 0;
        int left = 0;
        int right = items.Count - 1;

        while(left <= right)
        {
            steps++;
            int mid = (left + right) / 2;
            int cmp = items[mid].itemName.CompareTo(target);

            if (cmp == 0) return steps;
            else if (cmp < 0) left = mid + 1;
            else right = mid - 1;
        }
        return steps;
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

        for(int j = left; j < right; j++)
        {
            sortSteps++;        // 비교 1회
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
