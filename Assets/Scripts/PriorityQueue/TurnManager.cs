using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public Transform content;
    public TextMeshProUGUI logText;

    public Button autoButton;
    public TextMeshProUGUI buttonText;

    private int actionCount = 0;

    private void Awake()
    {
        instance = this;            // �ӽ÷� ����
    }

    public Action OnUpdateTick;

    private SimplePriorityQueue<TurnCharacter> charHeap = new SimplePriorityQueue<TurnCharacter>();

    bool isAuto = false;
    Coroutine automode;

    private void Start()
    {
        autoButton.onClick.AddListener(ChangeAutoMode);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            ChangeAutoMode();
        }

        if (Input.GetMouseButtonDown(0) && !isAuto)
        {
            CheckTick();
        }
    }

    void CheckTick()
    {
        // ���� �ൿ ���� ĳ���Ͱ� �����ϸ� ��ų ����
        if (charHeap.Count > 0)
        {
            UseSkill();
        }
        else        // ���� ��� ƽ ������Ʈ
        {
            OnUpdateTick?.Invoke();
        }
    }

    void UseSkill()
    {
        TurnCharacter character = charHeap.Dequeue();
        character.UseSkill();

        TextMeshProUGUI tmp = Instantiate(logText, content);
        tmp.transform.SetAsFirstSibling();
        tmp.text = $"[Action {++actionCount}]{character.characterName} ��ų �ߵ�";
        //Debug.Log($"[Action {++actionCount}]{character.characterName} ��ų �ߵ�");
    }

    public void EnqueueCharacter(TurnCharacter character)
    {
        charHeap.Enqueue(character, -character.speed);                  // ���ǵ尡 Ŭ ���� ���� ����ϵ��� ���������� ����
    }

    public void ChangeAutoMode()
    {
        isAuto = !isAuto;

        if (isAuto)
        {
            buttonText.text = "�ڵ� ������..";
            autoButton.image.color = Color.yellow;
            StartAutoMode();
        }
        else
        {
            buttonText.text = "�ڵ� ����";
            autoButton.image.color = Color.white;
            if (automode != null)
                StopCoroutine(automode);
        }
    }

    void StartAutoMode()
    {
        if (automode != null)
            StopCoroutine(automode);

        automode = StartCoroutine(AutoPlay());
    }

    IEnumerator AutoPlay()
    {
        while(true)
        {
            CheckTick();
            yield return new WaitForSeconds(1f);
        }
    }
}
