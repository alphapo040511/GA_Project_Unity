using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TurnCharacter : MonoBehaviour
{
    public TextMeshPro tmp;
    public string characterName = "전사";
    public int speed = 5;
    private int gauge = 0;

    Coroutine bouncing;

    private void Start()
    {
        TurnManager.instance.OnUpdateTick += UpdateTick;
        TurnManager.instance.EnqueueCharacter(this);
        tmp.text = $"{characterName} | <color=green>행동 가능</color>";
    }

    public void UseSkill()
    {
        tmp.text = $"{characterName} | <color=red>스킬 사용!</color>";

        if (bouncing != null)
            StopCoroutine(bouncing);

        bouncing = StartCoroutine(Bouncing());
    }

    public void UpdateTick()
    {
        gauge += speed;
        if(gauge >= 100)
        {
            gauge -= 100;
            // 행동 가능

            tmp.text = $"{characterName} | <color=blue>행동 가능</color>";
            TurnManager.instance.EnqueueCharacter(this);
        }
        else
        {
            tmp.text = $"{characterName} | 다음 행동까지 {(100 - gauge) / speed}";
        }
    }

    IEnumerator Bouncing()
    {
        float bouncingSpeed = 4f;

        Vector3 pos = transform.position;

        float height = 0.5f;

        while(height < 1.2f)
        {
            height += Time.deltaTime * bouncingSpeed;
            pos.y = height;
            transform.position = pos;
            yield return null;
        }

        while (height > 0.5f)
        {
            height -= Time.deltaTime * bouncingSpeed;
            pos.y = height;
            transform.position = pos;
            yield return null;
        }

        while (height < 1.2f)
        {
            height += Time.deltaTime * bouncingSpeed;
            pos.y = height;
            transform.position = pos;
            yield return null;
        }

        while (height > 0.5f)
        {
            height -= Time.deltaTime * bouncingSpeed;
            pos.y = height;
            transform.position = pos;
            yield return null;
        }

        pos.y = 0.5f;

        transform.position = pos;
    }
}
