using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string[] inventory = new string[5];
        inventory[0] = "Potion";
        inventory[1] = "Sword";

        Debug.Log(inventory[0]);
        Debug.Log(inventory[1]);
        Debug.Log(inventory[2]);

        for(int i = 0; i < inventory.Length; i++)
        {
            Debug.Log($"{i} 번째 인벤토리 : {inventory}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
