using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GreedyEnhanceSample : MonoBehaviour
{
    public List<EnhanceStone> enhanceStones = new List<EnhanceStone>();                             // 4개 고정

    public int currenLevel = 1;
    int needExp
    {
        get
        {
            return 8 * (currenLevel + 1) * (currenLevel + 1);
        }
    }

    private Dictionary<int, EnhanceStone> stoneDictionary = new Dictionary<int, EnhanceStone>();        // 경험치를 키값으로 딕셔너리
    private List<EnhanceStone> EPerGList = new List<EnhanceStone>();                                    // 경험치 / 골드 효율 (내림 차순)
    private List<EnhanceStone> expSortedList = new List<EnhanceStone>();                                // 경험치 크기 순으로 (내림 차순)

    // 사용할 강화석 개수
    Dictionary<EnhanceStone, int> usedCount = new Dictionary<EnhanceStone, int>();
    int needGold = 0;
    int getExp = 0;

    public Text levelText;
    public Text result;

    #region 초기화
    private void Start()
    {
        SortingLists();
        Enhance();
    }

    void SortingLists()
    {
        foreach(EnhanceStone stone in enhanceStones)
        {
            stoneDictionary[stone.exp] = stone;
        }

        EnhanceStone[] arr = enhanceStones.ToArray();
        if (arr != null)
        {
            StartQuickSort(arr, 0, arr.Length - 1);
            EPerGList = new List<EnhanceStone>(arr);
        }

        expSortedList = new List<EnhanceStone>(enhanceStones.ToArray());
        expSortedList.Reverse();

        // 정렬 완료
    }

    #endregion

    #region 퀵 정렬
    public static void StartQuickSort(EnhanceStone[] arr, int low, int high)
    {
        if (low < high)
        {
            int pivotIndex = Partition(arr, low, high);

            StartQuickSort(arr, low, pivotIndex - 1);           // 피벗 왼쪽 정렬
            StartQuickSort(arr, pivotIndex + 1, high);          // 피벗 오른쪽 정렬
        }
    }

    public static int Partition(EnhanceStone[] arr, int low, int high)
    {
        float pivot = arr[high].ExpPerGold;
        int i = (low - 1);

        for (int j = low; j < high; j++)
        {
            if (arr[j].ExpPerGold <= pivot)
            {
                i++;
                // swap
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
        // pivot 자리 교환
        (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
        return i + 1;
    }
    #endregion


    // Brute Force
    public void BruteForce()
    {
        // 최고 효율 = (들어간 돈 + 남은 경험치)가 작을수록 좋다!.?
        int best = int.MaxValue;
        usedCount = new Dictionary<EnhanceStone, int>();

        int aMax = needExp / enhanceStones[0].exp + 1;
        int bMax = needExp / enhanceStones[1].exp + 1;
        int cMax = needExp / enhanceStones[2].exp + 1;
        int dMax = needExp / enhanceStones[3].exp + 1;

        needGold = 0;
        getExp = 0;

        for (int a = 0; a < aMax; a++)
            for (int b = 0; b < bMax; b++)
                for (int c = 0; c < cMax; c++)
                    for (int d = 0; d < dMax; d++)
                    {
                        int currentExp = enhanceStones[0].exp * a + enhanceStones[1].exp * b + enhanceStones[2].exp * c + enhanceStones[3].exp * d;
                        if (currentExp < needExp) continue;         // 레벨업이 불가능한 경우 넘기기

                        int currentGold = enhanceStones[0].price * a + enhanceStones[1].price * b + enhanceStones[2].price * c + enhanceStones[3].price * d;
                        int total = currentExp - needExp + currentGold;      // 남은 경험치 + 골드

                        if(total < best)
                        {
                            best = total;
                            usedCount[enhanceStones[0]] = a;
                            usedCount[enhanceStones[1]] = b;
                            usedCount[enhanceStones[2]] = c;
                            usedCount[enhanceStones[3]] = d;          // 개수를 저장

                            getExp = currentExp;
                            needGold = currentGold;
                        }
                    }

        if(best == int.MaxValue)
        {
            Debug.LogWarning("레벨업 실패..??");
            return;
        }

        UpdateText();
    }

    // 경험치 최적화
    public void CalculateMinExpWaste()
    {
        if (!stoneDictionary.ContainsKey(3) || !stoneDictionary.ContainsKey(5)) return;     // 하급이나 중급 강화석이 없으면 무시

        // 강화석 소, 중만 사용 (8 배수)
        needGold = 0;
        getExp = 0;
        usedCount = new Dictionary<EnhanceStone, int>();

        int count = 0;

        while(getExp < needExp)        // 목표치 넘을때 까지 반복
        {
            getExp += 8;       // 소, 중의 합이 8 고정이라는 가정으로 계산
            needGold += stoneDictionary[3].price;
            needGold += stoneDictionary[5].price;

            if (!usedCount.ContainsKey(stoneDictionary[3])) usedCount[stoneDictionary[3]] = 0;
            usedCount[stoneDictionary[3]] += 1;

            if (!usedCount.ContainsKey(stoneDictionary[5])) usedCount[stoneDictionary[5]] = 0;
            usedCount[stoneDictionary[5]] += 1;

            count++;
        }

        UpdateText();
    }

    public void OrderByExpPerGold()
    {
        usedCount = new Dictionary<EnhanceStone, int>();

        int moreNeed = needExp;
        needGold = 0;
        getExp = 0;

        foreach (EnhanceStone s in EPerGList)
        {
            int use = moreNeed / s.exp;       // 해당 코인으로 줄 수 있는 최대 개수

            if (!usedCount.ContainsKey(s))
                usedCount[s] = 0;

            needGold += use * s.price;
            usedCount[s] += use;
            moreNeed -= use * s.exp;
        }

        while(moreNeed > 0)
        {
            moreNeed -= stoneDictionary[3].exp;
            needGold += stoneDictionary[3].price;
            usedCount[stoneDictionary[3]] += 1;
        }

        getExp = needExp - moreNeed;        // 목표치에 초과치 더해줌

        UpdateText();
    }

    public void SortByExpAmount()
    {
        usedCount = new Dictionary<EnhanceStone, int>();

        needGold = 0;
        getExp = 0;

        int moreNeed = needExp;

        foreach (EnhanceStone s in expSortedList)
        {
            int use = moreNeed / s.exp;       // 해당 코인으로 줄 수 있는 최대 개수

            if (!usedCount.ContainsKey(s))
                usedCount[s] = 0;

            needGold += use * s.price;
            usedCount[s] += use;
            moreNeed -= use * s.exp;
        }

        // 남은건 강화석 소로 채우기
        while (moreNeed > 0)
        {
            moreNeed -= stoneDictionary[3].exp;
            needGold += stoneDictionary[3].price;
            usedCount[stoneDictionary[3]] += 1;
        }

        getExp = needExp - moreNeed;        // 목표치에 초과치 더해줌

        UpdateText();
    }

    public void Enhance()
    {
        if (needExp <= getExp)
            currenLevel++;

        if (levelText != null)
            levelText.text = $"현재 레벨 {currenLevel}";

        getExp = 0;
        needGold = 0;
        usedCount.Clear();
        UpdateText();
    }

    void UpdateText()
    {
        string resultText = "";
        resultText += $"예상 경험치 : {getExp}/{needExp}" +
        $"\n필요 골드 : {needGold}" +
        $"\n\n사용할 강화석";

        foreach (var s in usedCount)
        {
            resultText += $"\n{s.Key.stoneName} : {s.Value}개";
        }

        if (result != null)
            result.text = resultText;

        Debug.Log(resultText);
    }
}
