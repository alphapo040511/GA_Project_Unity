using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class RankingData
{
    public string playerName;
    public int score;

    public string changeText(int index)
    {
        return $"#{index} | Name : {playerName} | SCORE : {score}";
    }
}


public class Ranking : MonoBehaviour
{
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI textPrefab;
    public Transform originView;
    public Transform sortView;

    public int arrSize = 100;
    private RankingData[] rankingDatas;
    private List<TextMeshProUGUI> sortTexts = new List<TextMeshProUGUI>();

    private void Start()
    {
        StartCoroutine(NewRanking());
    }

    public IEnumerator NewRanking()
    {
        rankingDatas = new RankingData[arrSize];

        for (int i = 0; i < arrSize; i++)
        {
            TextMeshProUGUI originText = Instantiate(textPrefab, originView);
            TextMeshProUGUI text = Instantiate(textPrefab, sortView);

            sortTexts.Add(text);

            RankingData newData = new RankingData();
            newData.playerName = $"Player_{i}";
            newData.score = UnityEngine.Random.Range(0, 10000);

            rankingDatas[i] = newData;

            originText.text = newData.changeText(i);
            text.text = newData.changeText(i);

            yield return null;
        }
    }

    public void StartSelectSort()
    {
        ClearSortView();
        StartCoroutine(Select());
    }

    public void StartBubbleSort()
    {
        ClearSortView();
        StartCoroutine(Bubble());
    }

    public void StartQuickSort()
    {
        ClearSortView();
        StartCoroutine(StartQuickSort(rankingDatas, 0, rankingDatas.Length - 1));
    }

    IEnumerator Select()
    {
        stateText.text = "Ready";
        yield return new WaitForSeconds(1);

        stateText.text = "Working";

        RankingData[] datas = rankingDatas.ToArray();
        int n = datas.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int minIndex = i;
            for (int j = i + 1; j < n; j++)
            {
                if (datas[j].score < datas[minIndex].score)
                {
                    minIndex = j;
                }
            }
            (datas[minIndex], datas[i]) = (datas[i], datas[minIndex]);      // swap

            // 텍스트 업데이트
            sortTexts[minIndex].text = datas[minIndex].changeText(minIndex);
            sortTexts[i].text = datas[i].changeText(i);             

            yield return new WaitForSeconds(0.1f);
        }

        stateText.text = "Done";
    }

    IEnumerator Bubble()
    {
        stateText.text = "Ready";
        yield return new WaitForSeconds(1);

        stateText.text = "Working";

        RankingData[] datas = rankingDatas.ToArray();

        int n = datas.Length;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (datas[j].score > datas[j + 1].score)
                {
                    // swap
                    (datas[j], datas[j + 1]) = (datas[j + 1], datas[j]);
                    swapped = true;

                    // 텍스트 업데이트
                    sortTexts[j].text = datas[j].changeText(j);
                    sortTexts[j + 1].text = datas[j + 1].changeText(j + 1);
                }

                yield return new WaitForSeconds(0.1f);
            }
            // 이미 정렬 된 경우 조기 종료
            if (!swapped) break;
        }

        stateText.text = "Done";
    }

    IEnumerator StartQuickSort(RankingData[] datas, int low, int high)
    {
        stateText.text = "Ready";
        yield return new WaitForSeconds(1);

        stateText.text = "Working";

        if (low < high)
        {
            int pivotIndex = 0;
            StartCoroutine(Partition(datas, low, high, result => pivotIndex = result));

            StartQuickSort(datas, low, pivotIndex - 1);           // 피벗 왼쪽 정렬
            StartQuickSort(datas, pivotIndex + 1, high);          // 피벗 오른쪽 정렬

            yield return new WaitForSeconds(0.1f);
        }

        stateText.text = "Done";
    }

    IEnumerator Partition(RankingData[] datas, int low, int high, Action<int> callback)
    {
        int pivot = datas[high].score;
        int i = (low - 1);

        for (int j = low; j < high; j++)
        {
            if (datas[j].score <= pivot)
            {
                i++;
                // swap
                (datas[i], datas[j]) = (datas[j], datas[i]);

                // 텍스트 업데이트
                sortTexts[i].text = datas[i].changeText(i);
                sortTexts[j].text = datas[j].changeText(j);
                }

            yield return new WaitForSeconds(0.1f);
        }
        // pivot 자리 교환
        (datas[i + 1], datas[high]) = (datas[high], datas[i + 1]);
        callback.Invoke(i + 1);
    }

    void ClearSortView()
    {
        stateText.text = "Initializing...";
        for (int i = 0; i < rankingDatas.Length; i++)
        {
            sortTexts[i].text = rankingDatas[i].changeText(i);
        }

        Debug.Log("초기화 완료");
    }
}
