using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enhance Stone", menuName = "Enhance Stone")]
public class EnhanceStone : ScriptableObject
{
    public string stoneName;
    public int exp;
    public int price;
    public float ExpPerGold
    {
        get
        {
            if (exp == 0) return 0;

            return (float)exp / (float)price;
        }
    }
}
