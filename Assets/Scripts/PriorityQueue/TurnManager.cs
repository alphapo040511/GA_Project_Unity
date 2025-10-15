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
        instance = this;            // 임시로 선언
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
        // 현재 행동 가능 캐릭터가 존재하면 스킬 실행
        if (charHeap.Count > 0)
        {
            UseSkill();
        }
        else        // 없을 경우 틱 업데이트
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
        tmp.text = $"[Action {++actionCount}]{character.characterName} 스킬 발동";
        //Debug.Log($"[Action {++actionCount}]{character.characterName} 스킬 발동");
    }

    public void EnqueueCharacter(TurnCharacter character)
    {
        charHeap.Enqueue(character, -character.speed);                  // 스피드가 클 수록 먼저 사용하도록 음수값으로 저장
    }

    public void ChangeAutoMode()
    {
        isAuto = !isAuto;

        if (isAuto)
        {
            buttonText.text = "자동 전투중..";
            autoButton.image.color = Color.yellow;
            StartAutoMode();
        }
        else
        {
            buttonText.text = "자동 전투";
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
