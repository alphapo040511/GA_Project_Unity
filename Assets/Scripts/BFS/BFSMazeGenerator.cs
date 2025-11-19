using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BFSMazeGenerator : MazeGenerator
{
    public GameObject playerPrefab;
    private GameObject player;
    Vector2Int?[,] parent;

    bool findBFSPath = false;
    Coroutine playerMoveRoutine;

    private void Start()
    {
        player = Instantiate(playerPrefab, new Vector3(start.x, 1, start.y), Quaternion.identity);
        Generate();
    }

    public void MovePlayer()
    {
        if(playerMoveRoutine != null)
        {
            // 이미 재생중
            return;
        }

        playerMoveRoutine = StartCoroutine(MoveToPath());
    }

    IEnumerator MoveToPath()
    {
        if (escapeRoute.Count <= 1)
        {
            playerMoveRoutine = null;
            yield break;
        }

        List<Vector2Int> routeCopy = new List<Vector2Int>(escapeRoute.ToArray());
        routeCopy.RemoveAt(0);
        while (routeCopy.Count > 0)
        {
            Vector2Int route = routeCopy[0];
            Vector3 targetPos = new Vector3(route.x, 1, route.y);
            Vector3 dir = targetPos - player.transform.position;

            Vector3 startPos = player.transform.position;
            player.transform.forward = dir;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2f;
                player.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            player.transform.position = targetPos;
            routeCopy.RemoveAt(0);
        }

        playerMoveRoutine = null;
    }

    public override void Generate()
    {
        findBFSPath = false;

        if (generateRoutine != null)
        {
            Debug.LogWarning("맵 생성 시도중, 잠시 후 재시도 희망");
            return;
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (playerMoveRoutine != null)
        {
            StopCoroutine(playerMoveRoutine);
            playerMoveRoutine = null;
            player.transform.position = new Vector3(start.x, 1, start.y);
        }

        generateRoutine = StartCoroutine(TryGenerate());

        escapeRoute = FindPathBFS();

        CheckFarPoint();
    }

    void CheckFarPoint()
    {
        int best = 0;
        List<Vector2Int> farPos = new List<Vector2Int>();
        for (int x = 1; x < map.GetLength(0) - 1; x++)
        {
            for(int y = 1; y < map.GetLength(1) - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (pos == start) continue;     // 시작 지점이랑 동일하면 넘어감
                if (map[x, y]) continue;        // 벽이면 넘어감

                int distance = FindPathDeath(pos);

                if (distance > best)
                {
                    Debug.Log($"최장 거리 변경 [{distance}]");
                    farPos.Clear();
                    farPos.Add(pos);
                    best = distance;
                }
                else if(distance == best)
                {
                    farPos.Add(pos);
                    Debug.Log($"최장 거리 추가 [{distance}] / 개수 : {farPos.Count}");
                }
            }
        }

        foreach (var far in farPos)
        {
            GameObject route = GameObject.CreatePrimitive(PrimitiveType.Cube);
            route.transform.SetParent(transform);
            route.transform.position = new Vector3(far.x, 1.5f, far.y);
            route.transform.localScale = new Vector3(1, 3, 1);
            route.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
    }

    public override void ShowRoute()
    {
        if (showRoutine != null || findBFSPath == false) return;
        showRoutine = StartCoroutine(ShowEscapeRoute());
    }

    List<Vector2Int> FindPathBFS()
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);


        visited = new bool[w, h];
        parent = new Vector2Int?[w, h];

        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        visited[start.x, start.y] = true;

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();

            // 목표 도착
            if (cur == goal)
            {
                Debug.Log("BFS: Goal 도착");
                findBFSPath = true;
                return ReconstructPath();
            }

            // 네 방향 이웃 탐색
            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(nx, ny)) continue;        // 바운더리 확인
                if (map[nx, ny]) continue;         // 벽 확인
                if (visited[nx, ny]) continue;          // 이미 방문

                visited[nx, ny] = true;
                parent[nx, ny] = cur;                   // 경로 복원용 부모
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }
        Debug.Log("BFS: 경로 없음");
        return null;
    }

    int FindPathDeath(Vector2Int endPos)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);


        bool[,] tempVisited = new bool[w, h];
        Vector2Int?[,] parents = new Vector2Int?[w, h];

        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        tempVisited[start.x, start.y] = true;

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();

            // 목표 도착
            if (cur == endPos)
            {
                findBFSPath = true;
                return SearchingParent(endPos, parents);
            }

            // 네 방향 이웃 탐색
            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(nx, ny)) continue;        // 바운더리 확인
                if (map[nx, ny]) continue;         // 벽 확인
                if (tempVisited[nx, ny]) continue;          // 이미 방문

                tempVisited[nx, ny] = true;
                parents[nx, ny] = cur;                   // 경로 복원용 부모
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }
        return 0;
    }

    int SearchingParent(Vector2Int endPos, Vector2Int?[,] parents)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = endPos;

        // goal -> start 방향으로 parent 따라가기
        while (cur.HasValue)
        {
            path.Add(cur.Value);
            cur = parents[cur.Value.x, cur.Value.y];
        }

        return path.Count;
    }

    bool InBounds(int x, int y)
    {
        return x > 0 && y > 0 &&
            x < map.GetLength(0) &&
            y < map.GetLength(1);
    }

    List<Vector2Int> ReconstructPath()
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        // goal -> start 방향으로 parent 따라가기
        while (cur.HasValue)
        {
            path.Add(cur.Value);
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse(); // start -> goal 순으로 반전
        Debug.Log($"경로 깊이: {path.Count}");
        foreach (var p in path)
        {
            Debug.Log(p);
        }
        return path;
    }
}
