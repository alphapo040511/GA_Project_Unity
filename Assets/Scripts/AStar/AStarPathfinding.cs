using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class AStarPathfinding : MonoBehaviour
{
    public BiomeMapGenerator mapGenerator;

    [Header("Maze Settings")]
    public int width = 15;
    public int depth = 15;

    [Header("Color Settings")]
    public Color wallColor;
    public Color groundColor;
    public Color forestColor;
    public Color mudColor;
    public Color enemyColor;

    [Header("UI References")]
    public Toggle avoidWallsToggle;
    public Toggle avoidEnemysToggle;
    private bool avoidWalls
    {
        get
        {
            if (avoidWallsToggle != null)
                return avoidWallsToggle.isOn;
            else
                return false;
        }
    }
    private bool avoidEnemys
    {
        get
        {
            if (avoidEnemysToggle != null)
                return avoidEnemysToggle.isOn;
            else
                return false;
        }
    }

    int[,] map;

    Vector2Int start;
    Vector2Int goal;

    bool showed = false;
    List<Vector2Int> path = new List<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = GetComponent<BiomeMapGenerator>();
        GenerateMaze();
    }

    public void ShowPath()
    {
        if (showed) return;

        StartCoroutine(ShowPath(path));
        showed = true;
    }

    public async void GenerateMaze()
    {
        StopAllCoroutines();
        showed = false;

        start = new Vector2Int(1, 1);
        goal = new Vector2Int(width - 2, depth - 2);
        map = await mapGenerator.GenerateRandomMap(width, depth, start, goal);

        if (map == null)
        {
            Debug.LogWarning("맵 생성 실패. 재시도 바람.");
            return;
        }

        ShowMap();

        path = new List<Vector2Int>();
        path = await AStar(map, start, goal);

        if (path == null)
            Debug.Log("경로 없음");
        else
        {
            Debug.Log($"경로 길이: {path.Count}");
            foreach (var p in path)
                Debug.Log(p);
        }
    }

    async Task<List<Vector2Int>> AStar(int[,] map, Vector2Int start, Vector2Int goal)
    {
        await Task.Yield();   // 한 프레임 쉬기

        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] gCost = new int[w, h];
        bool[,] visited = new bool[w, h];
        Vector2Int?[,] parent = new Vector2Int?[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = int.MaxValue;

        gCost[start.x, start.y] = 0;

        List<Vector2Int> open = new List<Vector2Int>();
        open.Add(start);


        Vector2Int[] dirs =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        while (open.Count > 0)
        {
            int bestIndex = 0;
            int bestF = F(open[0], gCost, goal);
            for (int i = 1; i < open.Count; i++)     // F가 가장 작은 노드 선택
            {
                int f = F(open[i], gCost, goal);
                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }
            Vector2Int cur = open[bestIndex];
            open.RemoveAt(bestIndex);

            if (visited[cur.x, cur.y]) continue;
            visited[cur.x, cur.y] = true;

            if (cur == goal)     // 도착
                return Reconstruct(parent, start, goal);

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 0) continue;
                if (visited[nx, ny]) continue;

                int moveCost = TileCost(map[nx, ny]);
                int newG = gCost[nx, ny] + moveCost;

                if (newG < gCost[nx, ny])
                {
                    gCost[nx, ny] = newG;
                    parent[nx, ny] = cur;


                    if (!open.Contains(new Vector2Int(nx, ny)))
                        open.Add(new Vector2Int(nx, ny));
                }
            }
        }
        return null;
    }

    int F(Vector2Int pos, int[,] gCost, Vector2Int goal)
    {
        return gCost[pos.x, pos.y] + H(pos, goal);
    }

    int H(Vector2Int a, Vector2Int b)
    {
        int h = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        if (avoidWalls)                         // 벽을 회피하는 경우   
            if (HasWallAround(a)) h += 2;       // 벽이 하나라도 있다면

        if (avoidEnemys)
            h += GetEnemyThreatLevel(a);

        return h;
    }

    bool HasWallAround(Vector2Int pos)
    {
        for(int x = -1; x <= 1; x++)
            for(int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;             // 자기 자신 제외

                Vector2Int cur = new Vector2Int(pos.x + x, pos.y + y);
                if (!InBounds(map, cur.x, cur.y)) continue;         // 맵 안에 있는지 확인
                if (map[cur.x, cur.y] == 0) return true;
            }

        return false;      // 주변 8칸중 벽이 없다면 
    }

    int GetEnemyThreatLevel(Vector2Int pos)
    {
        int level = 0;
        for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > 2) continue;              // 반경 2칸만 확인

                Vector2Int cur = new Vector2Int(pos.x + x, pos.y + y);
                if (!InBounds(map, cur.x, cur.y)) continue;                         // 맵 안에 있는지 확인
                if (map[cur.x, cur.y] == 4)         // 적이 있는 타일인 경우
                {
                    level += 3 - (Mathf.Abs(x) + Mathf.Abs(y));           // 거리값 반비례로 더하기   (2칸 +1, 1칸 + 2, 0칸 + 3)
                }
            }

        return level;
    }

    void ShowMap()
    {
        ClearMap();

        for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (new Vector2Int(x, y) == start)
                {
                    CreateCube(x, y, new Vector3(1, 0.1f, 1), Color.blue);
                    continue;
                }
                else if (new Vector2Int(x, y) == goal)
                {
                    CreateCube(x, y, new Vector3(1, 0.1f, 1), Color.yellow);
                    continue;
                }


                switch (map[x, y])
                {
                    case 1:
                        CreateCube(x, y, new Vector3(1, 0.1f, 1), groundColor);    // 땅 생성
                        break;
                    case 2:
                        CreateCube(x, y, new Vector3(1, 0.1f, 1), forestColor);    // 숲 생성
                        break;
                    case 3:
                        CreateCube(x, y, new Vector3(1, 0.1f, 1), mudColor);    // 진흙 생성
                        break;
                    case 4:
                        CreateCube(x, y, new Vector3(1, 0.1f, 1), groundColor);    // 땅 생성
                        CreateEnemy(x, y);      // 적 생성
                        break;
                    default:
                        CreateCube(x, y, new Vector3(1, 1, 1), wallColor);    // 벽 생성
                        break;
                }
            }
    }

    IEnumerator ShowPath(List<Vector2Int> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int pos = path[i];
            if (i == 0)
                CreateCube(pos.x, pos.y, Vector3.one * 0.5f, Color.blue);
            else if (i == path.Count - 1)
                CreateCube(pos.x, pos.y, Vector3.one * 0.5f, Color.red);
            else
                CreateCube(pos.x, pos.y, Vector3.one * 0.5f, Color.yellow);

            yield return new WaitForSeconds(0.1f);
        }
    }

    void CreateCube(int x, int y, Vector3 size, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(x, size.y / 2f, y);
        go.transform.localRotation = quaternion.identity;
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().material.color = color;
        go.name = $"{x},{y}_block";
    }

    void CreateEnemy(int x, int y)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(x, 0.5f, y);
        go.transform.localRotation = quaternion.identity;
        go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        go.GetComponent<MeshRenderer>().material.color = enemyColor;
        go.name = $"{x},{y}_Enemy";
    }

    void ClearMap()
    {
        foreach (Transform c in transform)
        {
            Destroy(c.gameObject);
        }
    }

    int TileCost(int tile)
    {
        switch (tile)
        {
            case 1: return 1;   // 평지
            case 2: return 3;   // 숲
            case 3: return 5;   // 진흙
            case 4: return 1;   // (적이 있는 타일 = 평지)
            default: return int.MaxValue;   // 0=벽 포함
        }
    }

    bool InBounds(int[,] map, int x, int y)
    {
        return x >= 0 && y >= 0 &&
               x < map.GetLength(0) &&
               y < map.GetLength(1);
    }

    List<Vector2Int> Reconstruct(Vector2Int?[,] parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        while (cur.HasValue)
        {
            path.Add(cur.Value);
            if (cur.Value == start) break;
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse();
        return path;
    }
}
