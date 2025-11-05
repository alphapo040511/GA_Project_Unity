using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Card Data", menuName = "Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    public int damage;
    public CardType type;
}

public enum CardType
{ 
    Attack,                     // 공격
    Enhance,                    // 강화 (다음 데미지 2배)
    Duplicate                   // 복제 (이전 카드 사용)
}
