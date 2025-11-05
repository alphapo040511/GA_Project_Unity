using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSequnceData
{
    public List<CardData> usedCards = new List<CardData>();
    public int totalDamage;
    public int[] order;

    public CardSequnceData()
    {
        usedCards = new List<CardData>();
        totalDamage = 0;
    }
}

public class CardSequnceTester : MonoBehaviour
{
    public CardSequnceData bestSequnce;

    private List<bool[]> useableList = new List<bool[]>();
    public List<int[]> sequnceList = new List<int[]>();

    private List<CardData> deck = new List<CardData>();

    private void Awake()
    {
        //UsableCardCombosSetting();          // 카드의 사용 여부 조합 저장
        PlayableCombosSetting();
    }

    // Start is called before the first frame update
    void Start()
    {
        deck = DeckManager.instance.CardDraw();
        CheckDeck(deck);
        CheckAllDamage();
        LogDamage();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (bestSequnce != null)
            {
                for (int i = 0; i < bestSequnce.usedCards.Count; i++)
                {
                    DeckManager.instance.UseCard(bestSequnce.usedCards[i]);
                }
                bestSequnce = null;
            }
            deck = DeckManager.instance.CardDraw();

            CheckDeck(deck);
            CheckAllDamage();
            LogDamage();
        }
    }

    void LogDamage()
    {
        if (bestSequnce != null)
        {
            Debug.Log($"최종 데미지 = {bestSequnce.totalDamage}");
            Debug.Log($"--- 카드 사용 순서---");
            for (int i = 0; i < bestSequnce.usedCards.Count; i++)
            {
                Debug.Log($"[{i + 1} 번째] {bestSequnce.usedCards[i].cardName} | 데미지 : {bestSequnce.usedCards[i].damage}");
            }
        }
    }


    void CheckAllDamage()
    {
        int count = 0;
        foreach (int[] sequnce in sequnceList)
        {
            int mana = 20;
            CardData lastCard = null;
            CardSequnceData record = new CardSequnceData();

            count++;

            for (int n = 0; n < sequnce.Length; n++)
            {
                int i = sequnce[n];

                if (mana - deck[i].cost < 0) continue;            // 마나가 부족한 경우 continue;

                mana -= deck[i].cost;                           // 마나 사용

                if (deck[i].type == CardType.Attack)
                {
                    if (lastCard != null && lastCard.type == CardType.Enhance)
                        record.totalDamage += (int)(deck[i].damage * 1.5f);         // 앞 카드가 강화일 경우 1.5배
                    else
                        record.totalDamage += deck[i].damage;
                }
                else if(deck[i].type == CardType.Duplicate && lastCard != null)     // 현재 카드가 복제고, 앞 카드가 있는 경우
                {
                    record.totalDamage += lastCard.damage;
                }

                record.order = sequnce;
                lastCard = deck[i];
                record.usedCards.Add(deck[i]);
            }

            string log = "";

            log += $"[{count} 번째 조합] {record.totalDamage}";
            for (int i = 0; i < record.usedCards.Count; i++)
            {
                log += $"[{i + 1}] {record.usedCards[i].cardName}";
            }

            Debug.Log(log);


            if (bestSequnce == null || record.totalDamage > bestSequnce.totalDamage)
            {
                bestSequnce = record;
            }
        }
    }



    // 카드의 사용 여부 조합(사용 안함)
    void UsableCardCombosSetting()
    {
        for(int i = 0; i < 2; i ++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    for (int n = 0; n < 2; n++)
                    {
                        bool[] useable = new bool[4]        //  사용 여부 확인(각 자리가 1이면 사용)
                        {
                            i == 1,
                            j == 1,
                            k == 1,
                            n == 1
                        };

                        useableList.Add(useable);
                    }
                }
            }
        }
    }

    void PlayableCombosSetting()
    {
        for(int i = 0; i < 4; i++)
        {
            sequnceList.Add(new int[1] {i});
            for (int j = 0; j < 4; j++)
            {
                if (j == i) continue;           // 이전 단계에 사용한 적이 있다면 넘기기
                sequnceList.Add(new int[2] { i, j});                           // 순서 리스트에 저장

                for (int k = 0; k < 4; k++)
                {
                    if (k == i || k == j) continue;           // 이전 단계에 사용한 적이 있다면 넘기기
                    sequnceList.Add(new int[3] { i, j, k});                           // 순서 리스트에 저장

                    for (int n = 0; n < 4; n++)
                    {
                        if (n == i || n == j || n == k) continue;           // 이전 단계에 사용한 적이 있다면 넘기기

                        sequnceList.Add(new int[4] { i, j, k, n });                           // 순서 리스트에 저장
                    }
                }
            }
        }
    }

    void CheckDeck(List<CardData> hands)
    {
        for(int i = 0; i < hands.Count; i++)
        {
            CardData target = hands[i];
            if (target.type == CardType.Attack)
                Debug.Log($"[{i + 1} 번 카드] {target.cardName} | Cost {target.cost} | 데미지 {target.cost} | Type {target.type}");
            else
                Debug.Log($"[{i + 1} 번 카드] {target.cardName} | Cost {target.cost} | Type {target.type}");
        }
    }
}
