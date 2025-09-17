using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SortTest : MonoBehaviour
{
    public GameObject readyText;

    public TextMeshProUGUI selection;
    public TextMeshProUGUI bubble;
    public TextMeshProUGUI quick;

    public int arrSize = 100;
    int[] originalArray;

    private bool isReady;

    private void Start()
    {
        Init();
    }

    [ContextMenu("Initialize")]
    public void Init()
    {
        StartCoroutine(GenerateRandomArray(arrSize));                   // 정확한 비교를 위해 동일한 배열 사용
    }

    public void SelectionSort()
    {
        if (!isReady) return;

        Stopwatch sw = new Stopwatch();
        sw.Reset();
        sw.Start();
        SelectionSortTest.StartSelectionSort(originalArray.ToArray());
        sw.Stop();

        long selectionTime = sw.ElapsedMilliseconds;
        if (selection != null)
        {
            selection.text = $"{selectionTime} ms";
        }
    }


    public void BubbleSort()
    {
        if (!isReady) return;

        Stopwatch sw = new Stopwatch();
        sw.Reset();
        sw.Start();
        BubbleSortTest.StartBubbleSort(originalArray.ToArray());
        sw.Stop();

        long bubbleTime = sw.ElapsedMilliseconds;
        if (bubble != null)
        {
            bubble.text = $"{bubbleTime} ms";
        }
    }


    public void QuickSort()
    {
        if (!isReady) return;

        Stopwatch sw = new Stopwatch();
        sw.Reset();
        sw.Start();
        QuickSortTest.StartQuickSort(originalArray.ToArray(), 0, originalArray.Length - 1);
        sw.Stop();

        long quickTime = sw.ElapsedMilliseconds;
        if (quick != null)
        {
            quick.text = $"{quickTime} ms";
        }
    }

    IEnumerator GenerateRandomArray(int size)
    {
        readyText.SetActive(false);
        isReady = false;

        originalArray = new int[size];
        System.Random rand = new System.Random();
        for (int i = 0; i < size; i++)
        {
            originalArray[i] = rand.Next(0, 10000);

            if(i % 10000 == 0)
            {
                yield return null;
            }
        }

        readyText.SetActive(true);
        isReady = true;
    }
}
