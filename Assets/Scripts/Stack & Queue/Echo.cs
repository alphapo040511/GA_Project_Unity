using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Echo : MonoBehaviour
{
    public TextMeshProUGUI frameText;

    public float speed = 5f;
    public GameObject afterEffect;
    public float recodeDuration = 3f;
    public float chronoBreakspeed = 5f;

    private Queue<(Vector3, bool)> moveQueue = new Queue<(Vector3, bool)>();
    private Stack<(Vector3, float)> moveHistory = new Stack<(Vector3, float)>();
    private Queue<Vector3> afterEffectQueue = new Queue<Vector3>();


    private Vector3 targetPos;
    private bool isRecording = true;

    private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
        StartRecord();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            Vector3 move = new Vector3(x, 0, y).normalized * speed * Time.deltaTime;
            targetPos += move;

            if (x != 0 || y != 0 || Input.GetKeyDown(KeyCode.R))
                moveQueue.Enqueue((targetPos, false));                              // 움직였거나 R키가 눌린 경우 true 값 저장

            if (Input.GetKeyDown(KeyCode.R))                                        // 역행을 누른 경우 역행 한 기록을 저장
            {
                if (moveHistory.Count > 0)
                {
                    int count = 0;
                    while (moveHistory.Count > 0)                                   // 배속 만큼 큐에서 더 꺼내기
                    {
                        Vector3 target;
                        float recodeTime;

                        (target, recodeTime) = moveHistory.Pop();

                        if (count % chronoBreakspeed == 0 || moveHistory.Count == 1)    // 배속 단위 프레임 마다 또는 마지막 프레임일 경우
                        {
                            moveQueue.Enqueue((target, true));                          // 기록 저장
                            targetPos = target;
                        }

                        if (recodeTime <= Time.time - recodeDuration)       // 정해진 시간만큼 돌아가면
                        {
                            moveHistory.Clear();                            // 움직임 기록 삭제
                            break;
                        }

                        count++;
                    }
                }
            }
            else
            {
                moveHistory.Push((targetPos, Time.time));
            }

            // 프레임 개수 표시
            frameText.text = $"Queue Count {moveQueue.Count}";

            if (Input.GetKey(KeyCode.Space))        // Space 누르면 녹화 종료
            {
                EndRecord();
            }

            return;
        }

        // 녹화 중이 아닐 때


        if (moveQueue.Count > 0)
        {
            Vector3 target;
            bool pressR = false;
            (target, pressR) = moveQueue.Dequeue();
            transform.position = target;

            afterEffectQueue.Enqueue(transform.position);       // 잔상 큐에 저장

            if (pressR)
            {
                renderer.material.color = Color.blue;
                afterEffect.SetActive(false);
            }
            else                                                    // 역행이 아닌 경우 에만
            {
                renderer.material.color = Color.white;
                afterEffect.SetActive(true);
            }
        }

    }

    public void StartRecord()
    {
        renderer.material.color = Color.white;
        frameText.enabled = true;
        isRecording = true;
        moveQueue.Clear();
        moveHistory.Clear();
        afterEffectQueue.Clear();
        StopAllCoroutines();
    }

    public void EndRecord()
    {
        afterEffect.transform.position = transform.position;
        frameText.enabled = false;
        isRecording = false;
        StartCoroutine(AfterEffect());
    }

    private IEnumerator AfterEffect()
    {
        yield return new WaitForSeconds(recodeDuration);

        while(afterEffectQueue.Count > 0)
        {
            afterEffect.transform.position = afterEffectQueue.Dequeue();
            yield return null;
        }
    }
}

